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
*文件名： BehaviorLogSnapshot
*描述：行为日志字段快照与差异计算
*
*****************************************************************************/

using LuBan.Common.LogCom;

namespace LuBan.Web.Core.Attributes;

/// <summary>
/// 行为日志字段快照与差异计算，
/// 以数据库原始列为准做前后比对，避免实体导航属性、值转换器对结果的影响。
/// </summary>
internal static class BehaviorLogSnapshot
{
    /// <summary>
    /// 敏感字段掩码
    /// </summary>
    public const string Mask = "***";

    /// <summary>
    /// 实体元数据缓存
    /// </summary>
    private static readonly ConcurrentDictionary<Type, EntityMeta> _metaCache = new();

    /// <summary>
    /// 不参与差异比较的基础审计列
    /// </summary>
    private static readonly HashSet<string> _ignoredColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        "id",
        "create_time",
        "update_time",
        "create_user_id",
        "create_user_name",
        "update_user_id",
        "update_user_name",
        "is_delete",
        "tenant_id",
        "create_org_id",
        "create_org_name"
    };

    /// <summary>
    /// 不参与文本快照的审计字段（按属性名）
    /// </summary>
    private static readonly HashSet<string> _ignoredProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "Id",
        "CreateTime",
        "UpdateTime",
        "CreateUserId",
        "CreateUserName",
        "UpdateUserId",
        "UpdateUserName",
        "IsDelete",
        "TenantId",
        "CreateOrgId",
        "CreateOrgName"
    };

    /// <summary>
    /// 敏感字段关键字
    /// </summary>
    private static readonly string[] _sensitiveKeywords =
    [
        "password",
        "pwd",
        "secret",
        "salt",
        "token",
        "privatekey",
        "apikey",
        "accesskey"
    ];

    /// <summary>
    /// 业务对象名称候选属性，按顺序取第一个非空值作为日志备注
    /// </summary>
    private static readonly string[] _displayNameCandidates =
    [
        "Title",
        "LabelName",
        "BlockName",
        "Name",
        "RealName",
        "NickName",
        "Account",
        "Code",
        "UserName"
    ];

    /// <summary>
    /// 单个字段值写入日志时的最大字符数，超出部分截断。
    /// 只影响落库文本，前后比对仍用完整值，避免正文类大字段把日志表撑爆
    /// </summary>
    private const int MaxValueLength = 2000;

    /// <summary>
    /// 集合与嵌套对象展开的最大深度，超过后只取文本形式
    /// </summary>
    private const int MaxNestDepth = 2;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    /// <summary>
    /// 实体元数据
    /// </summary>
    internal sealed class EntityMeta
    {
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; init; } = string.Empty;

        /// <summary>
        /// 中文描述
        /// </summary>
        public string DisplayName { get; init; } = string.Empty;

        /// <summary>
        /// 主键列名
        /// </summary>
        public string PrimaryKey { get; init; } = LuBanOrmConst.PrimaryKey;

        /// <summary>
        /// 参与比对的列
        /// </summary>
        public List<ColumnMeta> Columns { get; init; } = [];
    }

    /// <summary>
    /// 列元数据
    /// </summary>
    internal sealed class ColumnMeta
    {
        /// <summary>
        /// 数据库列名
        /// </summary>
        public string ColumnName { get; init; } = string.Empty;

        /// <summary>
        /// 属性名
        /// </summary>
        public string PropertyName { get; init; } = string.Empty;

        /// <summary>
        /// 字段中文名
        /// </summary>
        public string Label { get; init; } = string.Empty;

        /// <summary>
        /// 是否敏感字段
        /// </summary>
        public bool Sensitive { get; init; }
    }

    /// <summary>
    /// 获取实体元数据
    /// </summary>
    /// <param name="entityType"></param>
    /// <returns></returns>
    public static EntityMeta GetMeta(Type entityType)
    {
        return _metaCache.GetOrAdd(entityType, BuildMeta);
    }

    /// <summary>
    /// 按主键抓取一行原始数据快照，记录不存在时返回 null
    /// </summary>
    /// <param name="entityType"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static async Task<Dictionary<string, string?>?> CaptureAsync(Type entityType, object id)
    {
        var meta = GetMeta(entityType);
        using var client = LuBanOrm.GetIsolatedClient(entityType);
        var table = await client.Ado.GetDataTableAsync(
            $"select * from {meta.TableName} where {meta.PrimaryKey} = @id",
            new SugarParameter("@id", id));

        if (table.Rows.Count == 0)
        {
            return null;
        }

        var row = table.Rows[0];
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (DataColumn column in table.Columns)
        {
            var value = row[column];
            values[column.ColumnName] = value == DBNull.Value ? null : ToText(value);
        }
        return values;
    }

    /// <summary>
    /// 计算前后差异，只保留发生变化的字段，返回与键一一对应的前后 JSON
    /// </summary>
    /// <param name="meta"></param>
    /// <param name="before"></param>
    /// <param name="after"></param>
    /// <param name="maxFields"></param>
    /// <returns></returns>
    public static (string? BeforeJson, string? AfterJson, int FieldCount) BuildDiff(EntityMeta meta, Dictionary<string, string?>? before, Dictionary<string, string?>? after, int maxFields)
    {
        var beforeJson = new Dictionary<string, string?>();
        var afterJson = new Dictionary<string, string?>();
        var usedLabels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var fieldCount = 0;

        foreach (var column in meta.Columns)
        {
            if (_ignoredColumns.Contains(column.ColumnName))
            {
                continue;
            }

            string? beforeValue = null;
            string? afterValue = null;
            before?.TryGetValue(column.ColumnName, out beforeValue);
            after?.TryGetValue(column.ColumnName, out afterValue);

            if (before != null && after != null && string.Equals(beforeValue, afterValue, StringComparison.Ordinal))
            {
                continue;
            }

            if (fieldCount >= maxFields)
            {
                break;
            }

            var label = column.Label;
            if (!usedLabels.Add(label))
            {
                label = $"{column.Label}({column.PropertyName})";
                usedLabels.Add(label);
            }

            var isSensitive = column.Sensitive && (beforeValue != null || afterValue != null);
            beforeJson[label] = isSensitive ? Mask : TruncateValue(beforeValue);
            afterJson[label] = isSensitive ? Mask : TruncateValue(afterValue);
            fieldCount++;
        }

        if (fieldCount == 0)
        {
            return (null, null, 0);
        }

        return (JsonSerializer.Serialize(beforeJson, _jsonOptions), JsonSerializer.Serialize(afterJson, _jsonOptions), fieldCount);
    }

    /// <summary>
    /// 从请求参数生成新增数据快照（此时对象主键通常尚未生成）
    /// </summary>
    /// <param name="source"></param>
    /// <param name="maxFields"></param>
    /// <returns></returns>
    public static string? BuildFromInput(object? source, int maxFields)
    {
        if (source == null)
        {
            return null;
        }

        var values = ToValueMap(source, maxFields);
        return values.Count == 0 ? null : JsonSerializer.Serialize(values, _jsonOptions);
    }

    /// <summary>
    /// 把请求对象的可读属性展开为「字段中文名 → 值」字典，敏感字段掩码
    /// </summary>
    /// <param name="source"></param>
    /// <param name="maxFields"></param>
    /// <returns></returns>
    private static Dictionary<string, string?> ToValueMap(object source, int maxFields, bool skipAudit = false, int depth = 0)
    {
        var values = new Dictionary<string, string?>();

        //dynamic 入参是 JsonElement，没有可反射的属性，按 JSON 键展开
        if (source is JsonElement jsonSource)
        {
            if (jsonSource.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in jsonSource.EnumerateObject())
                {
                    if (values.Count >= maxFields)
                    {
                        break;
                    }
                    values[property.Name] = TruncateValue(JsonElementToText(property.Value));
                }
            }
            else
            {
                var text = JsonElementToText(jsonSource);
                if (!string.IsNullOrEmpty(text))
                {
                    values["value"] = TruncateValue(text);
                }
            }
            return values;
        }

        var properties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
        {
            if (values.Count >= maxFields)
            {
                break;
            }
            if (!property.CanRead || property.GetIndexParameters().Length > 0)
            {
                continue;
            }
            //导航属性与忽略列不参与文本快照，避免写出类型名或无意义字段
            if (property.GetCustomAttribute<Navigate>() != null)
            {
                continue;
            }
            var sugarColumn = property.GetCustomAttribute<SugarColumn>();
            if (sugarColumn?.IsIgnore == true)
            {
                continue;
            }
            if (skipAudit && _ignoredProperties.Contains(property.Name))
            {
                continue;
            }

            object? value;
            try
            {
                value = property.GetValue(source);
            }
            catch
            {
                continue;
            }

            var label = ResolvePropertyLabel(property);
            if (values.ContainsKey(label))
            {
                label = $"{label}({property.Name})";
            }
            values[label] = IsSensitive(property.Name) && value != null ? Mask : TruncateValue(ToFieldText(value, maxFields, depth));
        }
        return values;
    }

    /// <summary>
    /// 把集合元素压缩成一行可读文本，如「机构Id=3,职位Id=5」
    /// </summary>
    /// <param name="item"></param>
    /// <param name="maxFields"></param>
    /// <returns></returns>
    private static string Describe(object item, int maxFields, int depth = 0)
    {
        var map = ToValueMap(item, maxFields, skipAudit: true, depth: depth + 1);
        return string.Join(",", map
            .Where(u => !string.IsNullOrWhiteSpace(u.Value) && u.Value != "0" && u.Value != "false")
            .Select(u => $"{u.Key}={u.Value}"));
    }

    /// <summary>
    /// 字段值转日志文本：集合与嵌套对象展开成可读内容，避免把类型全名写进日志
    /// </summary>
    /// <param name="value"></param>
    /// <param name="maxFields"></param>
    /// <param name="depth">嵌套深度，超过上限不再展开，防止对象自引用时无限递归</param>
    /// <returns></returns>
    private static string? ToFieldText(object? value, int maxFields, int depth = 0)
    {
        if (value == null)
        {
            return null;
        }
        if (IsSimpleValue(value))
        {
            return ToText(value);
        }
        if (value is JsonElement jsonElement)
        {
            return JsonElementToText(jsonElement);
        }
        if (depth >= MaxNestDepth)
        {
            return ToText(value);
        }
        if (value is System.Collections.IEnumerable enumerable)
        {
            var items = new List<string?>();
            var count = 0;
            foreach (var item in enumerable)
            {
                if (count++ >= maxFields)
                {
                    break;
                }
                items.Add(item switch
                {
                    null => null,
                    JsonElement element => JsonElementToText(element),
                    _ => IsSimpleValue(item) ? ToText(item) : Describe(item, maxFields, depth)
                });
            }
            return string.Join(",", items.Where(u => !string.IsNullOrEmpty(u)));
        }
        return Describe(value, maxFields, depth);
    }

    /// <summary>
    /// 判断是否为可直接转文本的基础类型
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static bool IsSimpleValue(object value)
    {
        var type = value.GetType();
        return type.IsPrimitive
            || type.IsEnum
            || value is string
            || value is decimal
            || value is DateTime
            || value is DateTimeOffset
            || value is Guid;
    }

    private static string ResolvePropertyLabel(PropertyInfo property)
    {
        var label = property.GetCustomAttribute<DescriptionAttribute>()?.Description;
        if (string.IsNullOrWhiteSpace(label))
        {
            label = property.GetCustomAttribute<SugarColumn>()?.ColumnDescription;
        }
        return string.IsNullOrWhiteSpace(label) ? property.Name : label!;
    }

    /// <summary>
    /// 从请求参数生成「请求载荷」快照，
    /// 用于中间表授权、按外键批量删除等没有单一业务行可做前后比对的场景
    /// </summary>
    /// <param name="actionArguments"></param>
    /// <param name="maxFields"></param>
    /// <returns></returns>
    public static string? BuildPayload(IDictionary<string, object?> actionArguments, int maxFields)
    {
        var values = new Dictionary<string, string?>();
        foreach (var argument in actionArguments)
        {
            if (values.Count >= maxFields)
            {
                break;
            }

            var value = argument.Value;
            if (value == null)
            {
                continue;
            }

            if (IsSimpleValue(value))
            {
                values[argument.Key] = TruncateValue(ToText(value));
                continue;
            }

            //dynamic 入参经 System.Text.Json 绑定后是 JsonElement，按 JSON 结构展开记录
            if (value is JsonElement jsonElement)
            {
                if (jsonElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var property in jsonElement.EnumerateObject())
                    {
                        if (values.Count >= maxFields)
                        {
                            break;
                        }
                        values[property.Name] = TruncateValue(JsonElementToText(property.Value));
                    }
                }
                else
                {
                    values[argument.Key] = TruncateValue(JsonElementToText(jsonElement));
                }
                continue;
            }

            if (value is System.Collections.IEnumerable enumerable && value is not string)
            {
                var items = new List<string?>();
                var count = 0;
                foreach (var item in enumerable)
                {
                    if (count++ >= maxFields)
                    {
                        break;
                    }
                    items.Add(item switch
                    {
                        null => null,
                        JsonElement element => JsonElementToText(element),
                        _ => IsSimpleValue(item) ? ToText(item) : Describe(item, maxFields)
                    });
                }
                values[argument.Key] = TruncateValue(string.Join(",", items.Where(u => !string.IsNullOrEmpty(u))));
                continue;
            }

            //单个请求对象直接展开为字段键，避免多一层无意义的参数名
            var map = ToValueMap(value, maxFields);
            foreach (var item in map)
            {
                if (values.Count >= maxFields)
                {
                    break;
                }
                values[item.Key] = item.Value;
            }
        }

        return values.Count == 0 ? null : JsonSerializer.Serialize(values, _jsonOptions);
    }

    /// <summary>
    /// 从快照中解析业务对象名称，用于日志备注
    /// </summary>
    /// <param name="meta"></param>
    /// <param name="snapshot"></param>
    /// <returns></returns>
    public static string? ResolveDisplayName(EntityMeta meta, Dictionary<string, string?>? snapshot)
    {
        if (snapshot == null)
        {
            return null;
        }

        foreach (var candidate in _displayNameCandidates)
        {
            var column = meta.Columns.FirstOrDefault(u => string.Equals(u.PropertyName, candidate, StringComparison.OrdinalIgnoreCase));
            if (column == null)
            {
                continue;
            }
            if (snapshot.TryGetValue(column.ColumnName, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                return value.Length <= 128 ? value : value[..128];
            }
        }
        return null;
    }

    /// <summary>
    /// 从请求参数中解析业务对象主键
    /// </summary>
    /// <param name="actionArguments"></param>
    /// <param name="entityType">业务实体类型，用于按「实体名+Id」匹配命名主键</param>
    /// <param name="keyName">指定的主键字段名，留空按实体主键 Id 解析</param>
    /// <returns></returns>
    public static object? ResolveKey(IDictionary<string, object?> actionArguments, Type? entityType = null, string? keyName = null)
    {
        //指定了主键字段名时先按该字段取值，用于请求体用业务键命名的接口
        if (!string.IsNullOrWhiteSpace(keyName))
        {
            var namedKey = ResolveNamedKey(actionArguments, keyName);
            if (namedKey != null)
            {
                return namedKey;
            }
        }

        foreach (var argument in actionArguments.Values)
        {
            if (argument == null)
            {
                continue;
            }

            //dynamic 入参经 System.Text.Json 绑定后是 JsonElement，先按其 id 属性取主键
            if (argument is JsonElement jsonArgument)
            {
                var jsonKey = ResolveJsonElementKey(jsonArgument);
                if (jsonKey != null)
                {
                    return jsonKey;
                }
                continue;
            }

            if (IsKeyValue(argument, out var direct))
            {
                return direct;
            }

            if (argument is System.Collections.IEnumerable enumerable && argument is not string)
            {
                foreach (var item in enumerable)
                {
                    if (item != null && IsKeyValue(item, out var itemKey))
                    {
                        return itemKey;
                    }
                }
            }

            //主键属性名与数据库列名大小写不一致（Id vs id），必须按忽略大小写匹配
            var property = argument.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(u => string.Equals(u.Name, LuBanOrmConst.PrimaryKey, StringComparison.OrdinalIgnoreCase));
            if (property == null || !property.CanRead)
            {
                continue;
            }

            object? value;
            try
            {
                value = property.GetValue(argument);
            }
            catch
            {
                continue;
            }

            if (value != null && IsKeyValue(value, out var key))
            {
                return key;
            }
        }

        //命名主键兜底：授权类入参常为 UserId 这类明确指向业务对象的键，没有统一的 Id 属性，
        //先按「实体名+Id」匹配，再退到实体主键名本身，避免误取 TenantId 等审计字段
        var candidateNames = new List<string>();
        if (entityType != null)
        {
            var entityName = entityType.Name;
            if (entityName.StartsWith("Db", StringComparison.OrdinalIgnoreCase))
            {
                entityName = entityName[2..];
            }
            candidateNames.Add($"{entityName}{KeySuffix}");
        }
        candidateNames.Add(LuBanOrmConst.PrimaryKey);

        var namedValues = new List<(string Name, object? Value)>();
        foreach (var argument in actionArguments.Values)
        {
            if (argument == null || argument.GetType().IsPrimitive || argument is string || argument is System.Collections.IEnumerable)
            {
                continue;
            }
            foreach (var property in argument.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!property.CanRead || property.GetIndexParameters().Length > 0)
                {
                    continue;
                }
                object? value;
                try
                {
                    value = property.GetValue(argument);
                }
                catch
                {
                    continue;
                }
                if (value != null && IsKeyValue(value, out _))
                {
                    namedValues.Add((property.Name, value));
                }
            }
        }

        foreach (var name in candidateNames)
        {
            var matched = namedValues.FirstOrDefault(u => string.Equals(u.Name, name, StringComparison.OrdinalIgnoreCase));
            if (matched.Value != null)
            {
                return matched.Value;
            }
        }

        return null;
    }

    /// <summary>
    /// 主键属性后缀
    /// </summary>
    private const string KeySuffix = "Id";

    /// <summary>
    /// 取对象主键的 long 形式，无法转换时返回 null。
    /// 只接受整数与纯数字字符串，避免把 bool（true=1）、小数、枚举、Guid 等误当成主键
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static long? ToLongKey(object? key)
    {
        if (key == null)
        {
            return null;
        }

        var type = key.GetType();
        if (type == typeof(string))
        {
            return long.TryParse((string)key, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) && parsed > 0
                ? parsed
                : null;
        }

        if (type != typeof(long) && type != typeof(int) && type != typeof(short) && type != typeof(ushort)
            && type != typeof(byte) && type != typeof(sbyte) && type != typeof(uint) && type != typeof(ulong))
        {
            return null;
        }

        try
        {
            var value = Convert.ToInt64(key, CultureInfo.InvariantCulture);
            return value > 0 ? value : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 把 JsonElement 转成日志文本，null 值返回 null，对象/数组保留原始 JSON 片段
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    private static string? JsonElementToText(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.String => element.GetString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => element.GetRawText()
        };
    }

    /// <summary>
    /// 从请求参数中按指定字段名取主键，字段名大小写不敏感，兼容 JsonElement 与普通对象
    /// </summary>
    /// <param name="actionArguments"></param>
    /// <param name="keyName"></param>
    /// <returns></returns>
    private static object? ResolveNamedKey(IDictionary<string, object?> actionArguments, string keyName)
    {
        foreach (var argument in actionArguments.Values)
        {
            if (argument == null)
            {
                continue;
            }

            if (argument is JsonElement jsonArgument)
            {
                if (jsonArgument.ValueKind == JsonValueKind.Object)
                {
                    foreach (var property in jsonArgument.EnumerateObject())
                    {
                        if (string.Equals(property.Name, keyName, StringComparison.OrdinalIgnoreCase)
                            && TryGetJsonElementKey(property.Value, out var jsonKey))
                        {
                            return jsonKey;
                        }
                    }
                }
                continue;
            }

            if (argument is System.Collections.IEnumerable && argument is not string)
            {
                continue;
            }

            var matched = argument.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(u => u.CanRead
                    && u.GetIndexParameters().Length == 0
                    && string.Equals(u.Name, keyName, StringComparison.OrdinalIgnoreCase));
            if (matched == null)
            {
                continue;
            }

            object? value;
            try
            {
                value = matched.GetValue(argument);
            }
            catch
            {
                continue;
            }

            if (value != null && IsKeyValue(value, out var key))
            {
                return key;
            }
        }
        return null;
    }

    /// <summary>
    /// 取 JsonElement 中的主键值：正整数返回 long，字符串原样返回，交由 ToLongKey 校验
    /// </summary>
    /// <param name="element"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    private static bool TryGetJsonElementKey(JsonElement element, out object? key)
    {
        if (element.ValueKind == JsonValueKind.Number && element.TryGetInt64(out var number) && number > 0)
        {
            key = number;
            return true;
        }
        if (element.ValueKind == JsonValueKind.String)
        {
            key = element.GetString();
            return key != null;
        }
        key = null;
        return false;
    }

    /// <summary>
    /// 从 JsonElement 入参中取主键，只认主键名属性，避免把 appointmentId 之类的业务键当主键
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    private static object? ResolveJsonElementKey(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, LuBanOrmConst.PrimaryKey, StringComparison.OrdinalIgnoreCase)
                && TryGetJsonElementKey(property.Value, out var key))
            {
                return key;
            }
        }
        return null;
    }

    private static bool IsKeyValue(object value, out object? key)
    {
        key = null;
        var type = value.GetType();
        if (type == typeof(long) || type == typeof(int) || type == typeof(short) || type == typeof(uint) || type == typeof(ulong))
        {
            key = value;
            return Convert.ToInt64(value, CultureInfo.InvariantCulture) > 0;
        }
        if (type == typeof(string))
        {
            var text = (string)value;
            if (!string.IsNullOrWhiteSpace(text))
            {
                key = text;
                return true;
            }
        }
        if (type == typeof(Guid))
        {
            key = value;
            return (Guid)value != Guid.Empty;
        }
        return false;
    }

    private static EntityMeta BuildMeta(Type entityType)
    {
        var sugarTable = entityType.GetCustomAttribute<SugarTable>();
        var meta = new EntityMeta
        {
            TableName = sugarTable?.TableName ?? LuBanOrm.GetTableName(entityType.Name),
            DisplayName = string.IsNullOrWhiteSpace(sugarTable?.TableDescription) ? entityType.Name : sugarTable!.TableDescription,
            PrimaryKey = LuBanOrmConst.PrimaryKey
        };

        var columns = new List<ColumnMeta>();
        try
        {
            var entityInfo = LuBanOrm.SqlSugarScope.EntityMaintenance.GetEntityInfo(entityType);
            foreach (var column in entityInfo.Columns)
            {
                var property = column.PropertyInfo;
                columns.Add(new ColumnMeta
                {
                    ColumnName = string.IsNullOrWhiteSpace(column.DbColumnName) ? column.PropertyName : column.DbColumnName,
                    PropertyName = column.PropertyName,
                    Label = ResolveLabel(property, column.PropertyName),
                    Sensitive = IsSensitive(column.PropertyName) || IsSensitive(column.DbColumnName)
                });
            }
        }
        catch (Exception ex)
        {
            Logger.Warn($"读取实体[{entityType.FullName}]元数据失败，降级为属性反射", ex);
            foreach (var property in entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!property.CanRead || property.GetIndexParameters().Length > 0)
                {
                    continue;
                }
                var sugarColumn = property.GetCustomAttribute<SugarColumn>();
                if (sugarColumn == null || sugarColumn.IsIgnore)
                {
                    //无列特性的是导航属性或计算属性，不参与数据库比对
                    continue;
                }
                columns.Add(new ColumnMeta
                {
                    ColumnName = string.IsNullOrWhiteSpace(sugarColumn.ColumnName) ? LuBanOrm.GetColumnName(property.Name) : sugarColumn.ColumnName,
                    PropertyName = property.Name,
                    Label = ResolveLabel(property, property.Name),
                    Sensitive = IsSensitive(property.Name)
                });
            }
        }

        meta.Columns.AddRange(columns.Where(u => !string.IsNullOrWhiteSpace(u.ColumnName)));
        return meta;
    }

    private static string ResolveLabel(PropertyInfo? property, string fallback)
    {
        var label = property?.GetCustomAttribute<SugarColumn>()?.ColumnDescription;
        if (string.IsNullOrWhiteSpace(label))
        {
            label = property?.GetCustomAttribute<DescriptionAttribute>()?.Description;
        }
        return string.IsNullOrWhiteSpace(label) ? fallback : label!;
    }

    private static bool IsSensitive(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }
        return _sensitiveKeywords.Any(u => name.Contains(u, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 截断超长字段值，保留前 MaxValueLength 个字符并标注已截断
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static string? TruncateValue(string? value)
    {
        if (value == null || value.Length <= MaxValueLength)
        {
            return value;
        }
        return string.Concat(value.AsSpan(0, MaxValueLength), "…(已截断)");
    }

    private static string? ToText(object? value)
    {
        return value switch
        {
            null or DBNull => null,
            DateTime dateTime => dateTime.ToString("yyyy-MM-dd HH:mm:ss"),
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss"),
            byte[] bytes => Convert.ToBase64String(bytes),
            bool boolean => boolean ? "true" : "false",
            string text => text,
            System.Collections.IEnumerable enumerable => string.Join(",", enumerate(enumerable)),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture)
        };

        //集合统一渲染成逗号分隔，避免把类型名写进日志
        static IEnumerable<string> enumerate(System.Collections.IEnumerable items)
        {
            foreach (var item in items)
            {
                var text = ToText(item);
                if (!string.IsNullOrEmpty(text))
                {
                    yield return text;
                }
            }
        }
    }
}