/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Plugins
*文件名： PluginPermissions
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/15
*描述：插件权限声明（PRD §3.2 / §4.7）
*
*****************************************************************************/
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LuBan.AIAgent.Plugins;

/// <summary>
/// 插件权限声明（PRD §3.2 <c>permissions</c> 字段、§4.7 权限模型）。
/// </summary>
/// <remarks>
/// 六个维度：<c>filesystem</c> / <c>network</c> / <c>git</c> / <c>database</c> / <c>process</c> / <c>credential</c>。
/// 每个维度携带「范围」列表（scope），例如 <c>filesystem: ["workspace"]</c>、<c>network: ["codeup.aliyun.com"]</c>。
/// <para>
/// 为兼容 <c>plugin.json</c> 的两种写法，本类型挂了
/// <see cref="PluginPermissionsConverter"/>：
/// ① 数组简写 <c>["filesystem", "network"]</c>（裸维度等价于该维度通配 <c>"*"</c>）；
/// ② 对象详写 <c>{"filesystem": ["workspace"], "network": ["*"]}</c>。
/// </para>
/// <para>
/// 通配 <c>"*"</c>（含数组简写的裸维度）表示「声明该维度、具体范围由宿主白名单决定」：
/// 声明期放行（仅记警告），运行期以宿主白名单为上限判定访问是否越界（PRD §3.7）。
/// 要精确限定范围，请写 <c>维度:范围</c>（如 <c>"network:codeup.aliyun.com"</c>）。
/// </para>
/// </remarks>
[JsonConverter(typeof(PluginPermissionsConverter))]
public sealed class PluginPermissions
{
    /// <summary>文件系统维度范围。</summary>
    public List<string> Filesystem { get; set; } = new();

    /// <summary>网络维度范围（域名 / 通配符）。</summary>
    public List<string> Network { get; set; } = new();

    /// <summary>Git 维度范围。</summary>
    public List<string> Git { get; set; } = new();

    /// <summary>数据库维度范围。</summary>
    public List<string> Database { get; set; } = new();

    /// <summary>进程维度范围。</summary>
    public List<string> Process { get; set; } = new();

    /// <summary>凭据维度范围。</summary>
    public List<string> Credential { get; set; } = new();

    /// <summary>
    /// 全部已知维度名（顺序与文档一致）。
    /// </summary>
    public static readonly IReadOnlyList<string> KnownDimensions = new[]
    {
        "filesystem", "network", "git", "database", "process", "credential"
    };

    /// <summary>
    /// 是否未声明任何权限。
    /// </summary>
    public bool IsEmpty =>
        Filesystem.Count == 0 && Network.Count == 0 && Git.Count == 0 &&
        Database.Count == 0 && Process.Count == 0 && Credential.Count == 0;

    /// <summary>
    /// 已声明的维度名集合。
    /// </summary>
    public IEnumerable<string> DeclaredDimensions
    {
        get
        {
            if (Filesystem.Count > 0) yield return "filesystem";
            if (Network.Count > 0) yield return "network";
            if (Git.Count > 0) yield return "git";
            if (Database.Count > 0) yield return "database";
            if (Process.Count > 0) yield return "process";
            if (Credential.Count > 0) yield return "credential";
        }
    }

    /// <summary>
    /// 是否声明了指定维度。
    /// </summary>
    /// <param name="dimension">维度名（忽略大小写）。</param>
    /// <returns>声明了返回 <c>true</c>。</returns>
    public bool Declares(string dimension) => GetScopes(dimension).Count > 0;

    /// <summary>
    /// 取指定维度的范围列表。
    /// </summary>
    /// <param name="dimension">维度名（忽略大小写）。</param>
    /// <returns>范围列表；未知维度返回空列表。</returns>
    public IReadOnlyList<string> GetScopes(string dimension) => dimension?.Trim().ToLowerInvariant() switch
    {
        "filesystem" => Filesystem,
        "network" => Network,
        "git" => Git,
        "database" => Database,
        "process" => Process,
        "credential" => Credential,
        _ => Array.Empty<string>()
    };

    /// <summary>
    /// 取指定维度范围的列表（可变引用，供解析期填充）。
    /// </summary>
    /// <param name="dimension">维度名（忽略大小写）。</param>
    /// <returns>范围列表；未知维度返回 <c>null</c>。</returns>
    internal List<string>? ScopesOf(string dimension) => dimension?.Trim().ToLowerInvariant() switch
    {
        "filesystem" => Filesystem,
        "network" => Network,
        "git" => Git,
        "database" => Database,
        "process" => Process,
        "credential" => Credential,
        _ => null
    };

    /// <summary>
    /// 去重并剔除空白项。
    /// </summary>
    public void Normalize()
    {
        foreach (var dimension in KnownDimensions)
        {
            var list = ScopesOf(dimension);
            if (list is null) continue;

            var normalized = list
                .Where(scope => !string.IsNullOrWhiteSpace(scope))
                .Select(scope => scope.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            list.Clear();
            list.AddRange(normalized);
        }
    }
}

/// <summary>
/// <see cref="PluginPermissions"/> 的 JSON 转换器：兼容数组简写与对象详写（PRD §3.2）。
/// </summary>
public sealed class PluginPermissionsConverter : JsonConverter<PluginPermissions>
{
    /// <summary>
    /// <c>null</c> 令牌也交给本转换器处理。
    /// </summary>
    /// <remarks>
    /// 默认值为 <c>false</c> 时，引用类型遇到 JSON <c>null</c> 会绕过转换器、把属性直接赋成 <c>null</c>，
    /// 于是清单里写 <c>"permissions": null</c> 就会产生空引用，后续任何权限判断都会 NRE。
    /// 置为 <c>true</c> 后统一归一化为「空权限」。
    /// </remarks>
    public override bool HandleNull => true;

    /// <inheritdoc />
    public override PluginPermissions Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var permissions = new PluginPermissions();

        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return permissions;

            case JsonTokenType.StartArray:
                ReadArrayForm(ref reader, permissions);
                break;

            case JsonTokenType.StartObject:
                ReadObjectForm(ref reader, permissions);
                break;

            default:
                throw new JsonException($"permissions 字段格式非法：{reader.TokenType}");
        }

        permissions.Normalize();
        return permissions;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, PluginPermissions value, JsonSerializerOptions options)
    {
        // HandleNull = true 时 null 也会进到这里，必须自行处理（序列化为空对象，与「空权限」等价）。
        if (value is null)
        {
            writer.WriteStartObject();
            writer.WriteEndObject();
            return;
        }

        writer.WriteStartObject();
        foreach (var dimension in PluginPermissions.KnownDimensions)
        {
            var scopes = value.GetScopes(dimension);
            if (scopes.Count == 0) continue;

            writer.WritePropertyName(dimension);
            writer.WriteStartArray();
            foreach (var scope in scopes)
                writer.WriteStringValue(scope);
            writer.WriteEndArray();
        }

        writer.WriteEndObject();
    }

    /// <summary>
    /// 数组简写：<c>["filesystem", "network"]</c> 或 <c>["filesystem:workspace"]</c>。
    /// </summary>
    private static void ReadArrayForm(ref Utf8JsonReader reader, PluginPermissions permissions)
    {
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                return;

            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException("permissions 数组元素必须是字符串");

            var entry = reader.GetString();
            if (string.IsNullOrWhiteSpace(entry)) continue;

            var separator = entry.IndexOf(':');
            var dimension = separator < 0 ? entry : entry[..separator];
            var scope = separator < 0 ? "*" : entry[(separator + 1)..];

            var list = permissions.ScopesOf(dimension);
            if (list is null)
                throw new JsonException($"permissions 中出现未知权限维度：{dimension}");

            list.Add(string.IsNullOrWhiteSpace(scope) ? "*" : scope);
        }
    }

    /// <summary>
    /// 对象详写：<c>{"filesystem": ["workspace"], "network": ["*"]}</c>。
    /// </summary>
    private static void ReadObjectForm(ref Utf8JsonReader reader, PluginPermissions permissions)
    {
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("permissions 对象结构非法");

            var dimension = reader.GetString() ?? string.Empty;
            var list = permissions.ScopesOf(dimension)
                ?? throw new JsonException($"permissions 中出现未知权限维度：{dimension}");

            reader.Read();
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    var single = reader.GetString();
                    if (!string.IsNullOrWhiteSpace(single)) list.Add(single.Trim());
                    break;

                case JsonTokenType.StartArray:
                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndArray) break;
                        if (reader.TokenType != JsonTokenType.String)
                            throw new JsonException($"permissions.{dimension} 数组元素必须是字符串");

                        var scope = reader.GetString();
                        if (!string.IsNullOrWhiteSpace(scope)) list.Add(scope.Trim());
                    }
                    break;

                case JsonTokenType.Null:
                    break;

                default:
                    throw new JsonException($"permissions.{dimension} 取值类型非法");
            }
        }
    }
}