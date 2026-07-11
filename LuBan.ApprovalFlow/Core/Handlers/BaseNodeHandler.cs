/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.ApprovalFlow.Core.Handlers
*文件名： BaseNodeHandler
*版本号： V1.0.0.0
*唯一标识：101d1262-bf30-41c6-97d7-c8175d43d396
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/30 13:50:48
*描述：节点处理器基类，提供共享功能
*
*=================================================
*修改标记
*修改时间：2025/10/30 13:50:48
*修改人： yswenli
*版本号： V1.0.0.0
*描述：新增基类
* 
*****************************************************************************/

namespace LuBan.ApprovalFlow.Core.Handlers;

/// <summary>
/// 节点处理器基类：提供共享的节点处理功能
/// </summary>
public abstract class BaseNodeHandler : IFlowNodeHandler
{
    /// <summary>
    /// 判断是否能处理指定节点
    /// </summary>
    /// <param name="node">图节点</param>
    /// <returns>是否能处理</returns>
    public abstract bool CanHandle(GraphNode node);

    /// <summary>
    /// 处理节点
    /// </summary>
    /// <param name="ctx">节点处理上下文</param>
    /// <returns>处理结果</returns>
    public abstract Task<object?> HandleAsync(FlowNodeHandleContext ctx);

    /// <summary>
    /// 反序列化表单数据
    /// </summary>
    /// <param name="record">流程记录对象</param>
    /// <returns>反序列化后的表单数据字典，失败返回 null</returns>
    protected Dictionary<string, object>? DeserializeFormData(FlowRecord? record)
    {
        if (string.IsNullOrEmpty(record?.FormJson))
            return null;

        try
        {
            return SerializeUtil.Deserialize<Dictionary<string, object>>(record.FormJson!);
        }
        catch (Exception ex)
        {
            Logger.Warn($"Failed to deserialize form JSON: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 解析表单模型
    /// </summary>
    /// <param name="record">流程记录对象</param>
    /// <param name="formModelName">表单模型名称</param>
    /// <returns>元组：模型实例和模型类型</returns>
    protected (object? model, Type? modelType) ParseFormModel(FlowRecord? record, string formModelName)
    {
        if (string.IsNullOrEmpty(record?.FormJson) || string.IsNullOrWhiteSpace(formModelName))
            return (null, null);

        try
        {
            // 解析表单JSON，查找指定模型名称的属性
            using var doc = JsonDocument.Parse(record.FormJson!);
            if (doc.RootElement.TryGetProperty(formModelName!, out var sub))
            {
                // 根据模型名称解析类型
                var modelType = TypeResolveUtil.TryResolveTypeByName(formModelName!);
                if (modelType != null)
                {
                    // 反序列化为强类型模型
                    var model = SerializeUtil.Deserialize(sub.GetRawText(), modelType);
                    return (model, modelType);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Warn($"Failed to parse form model: {ex.Message}");
        }

        return (null, null);
    }

    /// <summary>
    /// 获取服务实例：优先通过依赖注入获取，失败则尝试直接创建实例
    /// </summary>
    /// <param name="targetType">目标服务类型</param>
    /// <returns>服务实例，获取失败返回 null</returns>
    protected object? GetServiceInstance(Type? targetType)
    {
        if (targetType == null)
            return null;

        // 优先尝试通过依赖注入获取服务实例
        object? serviceInstance = null;
        try { serviceInstance = targetType.GetRequiredService(); }
        catch (Exception ex) { Logger.Warn($"Failed to get service instance via DI: {ex.Message}"); }

        // 如果依赖注入失败，尝试通过反射创建实例
        if (serviceInstance == null)
        {
            try { serviceInstance = Activator.CreateInstance(targetType); }
            catch (Exception ex) { Logger.Warn($"Failed to create service instance: {ex.Message}"); }
        }

        return serviceInstance;
    }

    /// <summary>
    /// 准备方法参数：根据方法参数列表，从上下文中解析并填充参数值
    /// </summary>
    /// <param name="method">目标方法信息</param>
    /// <param name="ctx">节点处理上下文</param>
    /// <param name="formRoot">表单数据根字典</param>
    /// <param name="typedModel">强类型模型实例</param>
    /// <param name="typedModelType">强类型模型类型</param>
    /// <returns>方法参数数组</returns>
    protected object?[] PrepareMethodParameters(MethodInfo method, FlowNodeHandleContext ctx, Dictionary<string, object>? formRoot, object? typedModel, Type? typedModelType)
    {
        var parameters = method.GetParameters();
        var args = new object?[parameters.Length];

        // 遍历每个参数，解析对应的值
        for (int i = 0; i < parameters.Length; i++)
        {
            var p = parameters[i];
            args[i] = ResolveParameterValue(p, ctx, formRoot, typedModel, typedModelType);
        }

        return args;
    }

    /// <summary>
    /// 解析参数值：按优先级从多个数据源中查找匹配的参数值
    /// </summary>
    /// <param name="p">参数信息</param>
    /// <param name="ctx">节点处理上下文</param>
    /// <param name="formRoot">表单数据根字典</param>
    /// <param name="typedModel">强类型模型实例</param>
    /// <param name="typedModelType">强类型模型类型</param>
    /// <returns>解析后的参数值</returns>
    protected object? ResolveParameterValue(ParameterInfo p, FlowNodeHandleContext ctx, Dictionary<string, object>? formRoot, object? typedModel, Type? typedModelType)
    {
        // 1. 类型匹配的模型参数
        if (typedModel != null && typedModelType != null && p.ParameterType.IsAssignableFrom(typedModelType))
        {
            return typedModel;
        }
        // 2. 字典类型参数
        else if (p.ParameterType == typeof(Dictionary<string, object>) || p.ParameterType == typeof(IDictionary<string, object>))
        {
            return formRoot;
        }
        // 3. 记录ID参数
        else if (p.ParameterType == typeof(long))
        {
            return ctx.RecordId;
        }
        else if (p.ParameterType == typeof(int))
        {
            return (int)ctx.RecordId;
        }
        // 4. 角色列表参数
        else if ((p.ParameterType == typeof(IEnumerable<string>) || p.ParameterType == typeof(List<string>)) && ctx.Request != null)
        {
            return ctx.Request.ActorRoles ?? new List<string>();
        }
        // 5. 按参数名从多个数据源查找
        else
        {
            // 5.1 从节点属性中查找
            if (ctx.Node?.Properties != null && ctx.Node.Properties.TryGetValue(p.Name!, out var pv) && pv != null)
            {
                try { return Convert.ChangeType(pv, p.ParameterType); }
                catch (Exception ex)
                {
                    Logger.Warn($"Failed to convert parameter value '{p.Name}': {ex.Message}");
                    return pv;
                }
            }
            // 5.2 从表单数据中查找
            else if (formRoot != null && formRoot.TryGetValue(p.Name!, out var fv) && fv != null)
            {
                try { return Convert.ChangeType(fv, p.ParameterType); }
                catch (Exception ex)
                {
                    Logger.Warn($"Failed to convert form value '{p.Name}': {ex.Message}");
                    return fv;
                }
            }
            // 5.3 从流程上下文中查找
            else if (ctx.State.Context != null && ctx.State.Context.TryGetValue(p.Name!, out var cv) && cv != null)
            {
                try { return Convert.ChangeType(cv, p.ParameterType); }
                catch (Exception ex)
                {
                    Logger.Warn($"Failed to convert context value '{p.Name}': {ex.Message}");
                    return cv;
                }
            }
            // 5.4 特殊参数名：请求动作/边文本
            else if (string.Equals(p.Name, "action", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(p.Name, "edgeText", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(p.Name, "edgeValue", StringComparison.OrdinalIgnoreCase))
            {
                return ctx.Request?.Action;
            }
            // 5.5 特殊参数名：角色列表
            else if (string.Equals(p.Name, "actorRoles", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(p.Name, "roles", StringComparison.OrdinalIgnoreCase))
            {
                return ctx.Request?.ActorRoles ?? new List<string>();
            }
        }

        return null;
    }

    /// <summary>
    /// 更新模型字段：将指定值设置到模型的对应属性中
    /// </summary>
    /// <param name="typedModel">模型实例</param>
    /// <param name="typedModelType">模型类型</param>
    /// <param name="formModelField">字段名称</param>
    /// <param name="value">字段值</param>
    /// <returns>更新成功返回 true，否则返回 false</returns>
    protected bool UpdateModelField(object? typedModel, Type? typedModelType, string formModelField, object? value)
    {
        if (typedModel == null || typedModelType == null || string.IsNullOrWhiteSpace(formModelField))
            return false;

        try
        {
            // 查找目标属性
            var pi = typedModelType.GetProperty(formModelField!, BindingFlags.Public | BindingFlags.Instance);
            if (pi != null && pi.CanWrite && value != null)
            {
                // 类型转换处理
                object? setVal = null;
                if (pi.PropertyType.IsAssignableFrom(value.GetType()))
                    setVal = value;
                else
                    setVal = Convert.ChangeType(value, pi.PropertyType);

                // 设置属性值
                if (setVal != null)
                {
                    pi.SetValue(typedModel, setVal);
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Warn($"Failed to update model field '{formModelField}': {ex.Message}");
        }

        return false;
    }

    /// <summary>
    /// 保存表单数据：将更新后的模型序列化并保存到流程记录中
    /// </summary>
    /// <param name="record">流程记录对象</param>
    /// <param name="typedModel">模型实例</param>
    /// <param name="formModelName">模型名称</param>
    protected void SaveFormData(FlowRecord? record, object? typedModel, string formModelName)
    {
        if (record == null || typedModel == null || string.IsNullOrWhiteSpace(formModelName))
            return;

        try
        {
            // 反序列化现有的表单数据或创建新字典
            var rootDict = !string.IsNullOrEmpty(record.FormJson)
                ? SerializeUtil.Deserialize<Dictionary<string, object>>(record.FormJson!) ?? new()
                : new();

            // 更新指定模型的数据
            rootDict[formModelName!] = typedModel!;
            record.FormJson = SerializeUtil.Serialize(rootDict);
        }
        catch (Exception ex)
        {
            Logger.Warn($"Failed to save form data: {ex.Message}");
        }
    }
}