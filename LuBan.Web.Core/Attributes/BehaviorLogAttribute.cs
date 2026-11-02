/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.Web.Core.Attributes
*文件名： BehaviorLogAttribute
*描述：后台数据变更行为日志采集特性
*
*****************************************************************************/

using LuBan.Web.Core.Services;

namespace LuBan.Web.Core.Attributes;

/// <summary>
/// 后台数据变更行为日志采集特性
/// 标记在写接口上即可，无侵入采集“谁在何时对哪个对象改了哪些字段”。
/// 采集流程：动作执行前按主键抓旧行快照 → 执行 → 按主键抓新行快照 → 只记录变化字段，
/// 写入失败不影响接口返回。
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class BehaviorLogAttribute : BaseFilterAttribute
{
    /// <summary>
    /// 单条日志最多记录的字段数，防止大宽表产生超长 JSON
    /// </summary>
    private const int MaxDiffFields = 50;

    /// <summary>
    /// 模块名称，留空时取控制器上的 DisplayName
    /// </summary>
    public string? Module { get; set; }

    /// <summary>
    /// 业务实体类型，留空时按 Db{控制器名} 约定在控制器所在程序集内解析
    /// </summary>
    public Type? Entity { get; set; }

    /// <summary>
    /// 行为对象类型，默认 Other 表示不属于文章/活动/直播等业务对象
    /// </summary>
    public EnumBehaviorType BehaviorType { get; set; } = EnumBehaviorType.Other;

    /// <summary>
    /// 操作类型，默认 Other 表示按接口名自动推断
    /// </summary>
    public EnumBehaviorLogOperateType OperateType { get; set; } = EnumBehaviorLogOperateType.Other;

    /// <summary>
    /// 日志来源
    /// </summary>
    public EnumBehaviorLogSource Source { get; set; } = EnumBehaviorLogSource.Admin;

    /// <summary>
    /// 业务行没有字段变化时是否仍然记录一条日志。
    /// 用于授权类接口：只改中间表，被授权对象本身的值不变，但授权动作需要留痕
    /// </summary>
    public bool RecordWhenUnchanged { get; set; }

    /// <summary>
    /// 只记录请求载荷，不做业务行前后比对。
    /// 用于按外键批量授权/删除等没有单一业务行可快照的中间表接口
    /// </summary>
    public bool PayloadOnly { get; set; }

    /// <summary>
    /// 请求参数中指向业务行的主键字段名，默认为实体主键（Id）。
    /// 用于请求体用业务键命名的接口，如 appointmentId 指向预约行
    /// </summary>
    public string? KeyName { get; set; }

    /// <summary>
    /// 业务对象类型名称，用于只记载荷、无法从业务行推断对象名称的接口，
    /// 如授权类接口记为「角色菜单」；能解析到主键时页面会再追加「#主键」。
    /// 留空时依次退到业务行名称、实体中文名、模块名
    /// </summary>
    public string? ObjectType { get; set; }

    /// <summary>
    /// 构造，执行顺序设为最小，保证快照包住接口内的全部变更
    /// </summary>
    public BehaviorLogAttribute()
    {
        Order = int.MinValue;
    }

    /// <summary>
    /// 执行后不做处理
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public override Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        return next.Invoke();
    }

    /// <summary>
    /// 执行前后分别抓取数据快照并写入行为日志
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!BehaviorLogService.Enabled)
        {
            await next.Invoke();
            return;
        }

        var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
        var entityType = ResolveEntityType(descriptor, Entity);
        var operateType = ResolveOperateType(descriptor, OperateType);
        if (operateType == EnumBehaviorLogOperateType.Other)
        {
            operateType = ResolveStatusOperateType(descriptor, context.ActionArguments) ?? operateType;
        }

        var key = BehaviorLogSnapshot.ToLongKey(BehaviorLogSnapshot.ResolveKey(context.ActionArguments, entityType, KeyName));

        //新增时主键要等接口执行后才有；纯载荷记录不需要业务行快照
        Dictionary<string, string?>? before = null;
        if (!PayloadOnly && entityType != null && key != null && operateType != EnumBehaviorLogOperateType.Add)
        {
            try
            {
                before = await BehaviorLogSnapshot.CaptureAsync(entityType, key.Value);
            }
            catch (Exception ex)
            {
                Logger.Warn($"行为日志采集旧值失败[{entityType.Name}:{key}]", ex);
            }
        }

        ActionExecutedContext executedContext;
        try
        {
            executedContext = await next.Invoke();
        }
        catch
        {
            //接口异常时不记录变更，交由异常过滤器处理
            throw;
        }

        if (executedContext.Exception != null)
        {
            return;
        }

        //接口用返回值 false 表示业务未执行成功（如编码重复、前置校验不通过），此时不留痕
        if (ResolveResultValue(executedContext) is false)
        {
            return;
        }

        try
        {
            if (key == null)
            {
                //执行后再解析一次：新增接口通常把生成的Id回写到入参，或直接返回新Id
                key = BehaviorLogSnapshot.ToLongKey(BehaviorLogSnapshot.ResolveKey(context.ActionArguments, entityType, KeyName))
                    ?? BehaviorLogSnapshot.ToLongKey(ResolveResultValue(executedContext));
            }

            await WriteLogAsync(context, descriptor, entityType, operateType, key, before);
        }
        catch (Exception ex)
        {
            Logger.Warn("写入行为日志失败", ex);
        }
    }

    /// <summary>
    /// 组装并写入一条行为日志
    /// </summary>
    private async Task WriteLogAsync(
        ActionExecutingContext context,
        ControllerActionDescriptor? descriptor,
        Type? entityType,
        EnumBehaviorLogOperateType operateType,
        long? key,
        Dictionary<string, string?>? before)
    {
        string? beforeJson = null;
        string? afterJson = null;
        string? displayName = null;
        var meta = entityType == null ? null : BehaviorLogSnapshot.GetMeta(entityType);

        if (PayloadOnly)
        {
            afterJson = BehaviorLogSnapshot.BuildPayload(context.ActionArguments, MaxDiffFields);
        }
        else
        {
            if (meta == null)
            {
                return;
            }

            if (operateType == EnumBehaviorLogOperateType.Add)
            {
                Dictionary<string, string?>? inserted = null;
                if (key != null)
                {
                    try
                    {
                        inserted = await BehaviorLogSnapshot.CaptureAsync(entityType!, key.Value);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn($"行为日志采集新增行失败[{entityType!.Name}:{key}]", ex);
                    }
                }

                if (inserted != null)
                {
                    //拿到真实新行时按列记录，比直接存请求体更贴近入库结果
                    (beforeJson, afterJson, _) = BehaviorLogSnapshot.BuildDiff(meta, null, inserted, MaxDiffFields);
                    displayName = BehaviorLogSnapshot.ResolveDisplayName(meta, inserted);
                }
                else
                {
                    afterJson = BehaviorLogSnapshot.BuildFromInput(FindEntityArgument(context.ActionArguments), MaxDiffFields);
                }
            }
            else if (key == null)
            {
                if (!RecordWhenUnchanged)
                {
                    return;
                }
                afterJson = BehaviorLogSnapshot.BuildPayload(context.ActionArguments, MaxDiffFields);
            }
            else
            {
                var after = await BehaviorLogSnapshot.CaptureAsync(entityType!, key.Value);
                if (before == null && after == null)
                {
                    //对象在执行前后都不存在，说明没有实际变更
                    return;
                }
                var (diffBefore, diffAfter, fieldCount) = BehaviorLogSnapshot.BuildDiff(meta, before, after, MaxDiffFields);
                if (fieldCount == 0)
                {
                    //逻辑删除只改 is_delete，删除类动作仍需留痕；其余零变化视为未改动数据
                    if (!RecordWhenUnchanged && operateType != EnumBehaviorLogOperateType.Delete)
                    {
                        return;
                    }
                    afterJson = RecordWhenUnchanged ? BehaviorLogSnapshot.BuildPayload(context.ActionArguments, MaxDiffFields) : null;
                    displayName = BehaviorLogSnapshot.ResolveDisplayName(meta, before);
                }
                else
                {
                    beforeJson = diffBefore;
                    afterJson = diffAfter;
                    displayName = BehaviorLogSnapshot.ResolveDisplayName(meta, after) ?? BehaviorLogSnapshot.ResolveDisplayName(meta, before);
                }
            }
        }

        var module = ResolveModule(descriptor, Module);
        var log = new DbBehaviorLog
        {
            BehaviorType = BehaviorType == EnumBehaviorType.Other ? null : BehaviorType,
            OperationID = key,
            OperateType = operateType,
            Ip = context.HttpContext.GetClientIp(),
            Source = Source,
            Module = module,
            BeforeData = beforeJson,
            AfterData = afterJson,
            Remark = Truncate(ObjectType, 128) ?? displayName ?? meta?.DisplayName ?? module,
            CreateTime = DateTime.Now,
            CreateUserId = SessionUser.UserId > 0 ? SessionUser.UserId : null,
            CreateUserName = Truncate(SessionUser.RealName, 64),
            TenantId = SessionUser.TenantId,
            IsDelete = false
        };

        await BehaviorLogService.Instance.RecordAsync(log);
    }

    /// <summary>
    /// 取接口返回值中的新对象主键
    /// </summary>
    private static object? ResolveResultValue(ActionExecutedContext context)
    {
        var value = context.Result switch
        {
            ObjectResult objectResult => objectResult.Value,
            JsonResult jsonResult => jsonResult.Value,
            _ => null
        };

        //返回值可能已被统一结果过滤器包装成 Success/Fail，需取出其中的真实数据
        if (value != null && IsApiResultType(value.GetType()))
        {
            return value.GetType().GetProperty("Result", BindingFlags.Public | BindingFlags.Instance)?.GetValue(value);
        }
        return value;
    }

    /// <summary>
    /// 判断是否为统一返回结果类型（Success/Fail 及其泛型基类）
    /// </summary>
    private static bool IsApiResultType(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ApiResult<>))
        {
            return true;
        }
        return type.BaseType != null && IsApiResultType(type.BaseType);
    }

    /// <summary>
    /// 按 SetStatus 类接口入参中的状态值推断启用/停用
    /// </summary>
    private static EnumBehaviorLogOperateType? ResolveStatusOperateType(ControllerActionDescriptor? descriptor, IDictionary<string, object?> actionArguments)
    {
        var actionName = descriptor?.ActionName ?? string.Empty;
        if (!actionName.StartsWith("SetStatus", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        foreach (var argument in actionArguments.Values)
        {
            if (argument == null || argument.GetType().IsPrimitive || argument is string)
            {
                continue;
            }
            var property = argument.GetType().GetProperty("Status", BindingFlags.Public | BindingFlags.Instance);
            if (property == null || !property.CanRead)
            {
                continue;
            }
            try
            {
                var value = property.GetValue(argument);
                if (value == null)
                {
                    continue;
                }
                return Convert.ToInt32(value, CultureInfo.InvariantCulture) == 1
                    ? EnumBehaviorLogOperateType.Enable
                    : EnumBehaviorLogOperateType.Disable;
            }
            catch
            {
                return null;
            }
        }
        return null;
    }

    /// <summary>
    /// 解析业务实体类型，优先使用特性声明，其次按 Db{控制器名} 约定匹配
    /// </summary>
    private static Type? ResolveEntityType(ControllerActionDescriptor? descriptor, Type? declared)
    {
        if (declared != null)
        {
            return declared;
        }
        if (descriptor == null)
        {
            return null;
        }

        var controllerName = descriptor.ControllerTypeInfo.Name;
        if (controllerName.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
        {
            controllerName = controllerName[..^"Controller".Length];
        }
        var entityName = $"Db{controllerName}";

        try
        {
            var matched = descriptor.ControllerTypeInfo.Assembly
                .GetTypes()
                .FirstOrDefault(u => u.IsClass && !u.IsAbstract && string.Equals(u.Name, entityName, StringComparison.OrdinalIgnoreCase));
            if (matched != null)
            {
                return matched;
            }
        }
        catch (Exception ex)
        {
            Logger.Warn($"行为日志解析实体[{entityName}]失败", ex);
        }

        return WebApp.Types?.FirstOrDefault(u => u.IsClass && !u.IsAbstract && string.Equals(u.Name, entityName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 解析操作类型，优先使用特性声明，其次按接口名前缀推断
    /// </summary>
    private static EnumBehaviorLogOperateType ResolveOperateType(ControllerActionDescriptor? descriptor, EnumBehaviorLogOperateType declared)
    {
        if (declared != EnumBehaviorLogOperateType.Other)
        {
            return declared;
        }

        var actionName = descriptor?.ActionName ?? string.Empty;
        if (actionName.EndsWith("Async", StringComparison.OrdinalIgnoreCase))
        {
            actionName = actionName[..^"Async".Length];
        }

        return actionName.ToLowerInvariant() switch
        {
            var name when name.StartsWith("add") || name.StartsWith("create") || name.StartsWith("insert") => EnumBehaviorLogOperateType.Add,
            var name when name.StartsWith("delete") || name.StartsWith("remove") || name.StartsWith("clear") => EnumBehaviorLogOperateType.Delete,
            var name when name.StartsWith("enable") => EnumBehaviorLogOperateType.Enable,
            var name when name.StartsWith("disable") => EnumBehaviorLogOperateType.Disable,
            var name when name.StartsWith("reset") => EnumBehaviorLogOperateType.ResetPassword,
            var name when name.StartsWith("import") => EnumBehaviorLogOperateType.Import,
            var name when name.StartsWith("export") => EnumBehaviorLogOperateType.Export,
            var name when name.StartsWith("grant") || name.StartsWith("assign") || name.StartsWith("auth") => EnumBehaviorLogOperateType.Grant,
            var name when name.StartsWith("update") || name.StartsWith("edit") || name.StartsWith("save") || name.StartsWith("modify") => EnumBehaviorLogOperateType.Edit,
            _ => EnumBehaviorLogOperateType.Other
        };
    }

    /// <summary>
    /// 解析模块名称
    /// </summary>
    private static string? ResolveModule(ControllerActionDescriptor? descriptor, string? declared)
    {
        if (!string.IsNullOrWhiteSpace(declared))
        {
            return Truncate(declared, 64);
        }

        var displayName = descriptor?.ControllerTypeInfo.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            return Truncate(displayName, 64);
        }

        var controllerName = descriptor?.ControllerTypeInfo.Name;
        if (!string.IsNullOrWhiteSpace(controllerName))
        {
            return Truncate(controllerName.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)
                ? controllerName[..^"Controller".Length]
                : controllerName, 64);
        }
        return null;
    }

    /// <summary>
    /// 取请求参数中用于描述业务对象的参数，新增场景用于生成变更后数据
    /// </summary>
    private static object? FindEntityArgument(IDictionary<string, object?> actionArguments)
    {
        return actionArguments.Values
            .Where(u => u != null)
            .FirstOrDefault(u =>
            {
                var type = u!.GetType();
                return !type.IsPrimitive && !type.IsEnum && type != typeof(string) && !typeof(System.Collections.IEnumerable).IsAssignableFrom(type);
            });
    }

    /// <summary>
    /// 按列宽截断文本
    /// </summary>
    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}