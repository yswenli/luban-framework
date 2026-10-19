/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Connectors
*文件名： IConnector
*版本号： V1.0.0.0
*唯一标识：新建
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/19
*描述：连接器抽象 —— 连接器是唯一的声明单元，其 MCP 暴露面由连接器派生
*
*****************************************************************************/
namespace LuBan.AIAgent.Connectors;

/// <summary>
/// 连接器：对某一外部服务的连接门面，是<b>唯一的声明单元</b>。
/// </summary>
/// <remarks>
/// <para>
/// 连接器<b>不</b>继承 <see cref="MCP.IMCPClient"/>：MCP 只是连接器对外暴露能力的一种形态，
/// 由 <see cref="McpExposures"/> 派生。这样同一连接器可同时服务代码直调（如插件直接依赖
/// <c>YunxiaoConnector</c>）与 MCP 工具通道，避免为两者各写一套声明。
/// </para>
/// <para>
/// 生命周期：连接器实例由宿主以<b>宿主级单例</b>持有（见 <c>ConnectorInstanceHost</c>），
/// 会话级 MCP 客户端对它是<b>借用</b>关系——MCP 客户端释放时不得释放连接器。
/// </para>
/// </remarks>
public interface IConnector : IAsyncDisposable
{
    /// <summary>
    /// 提供方键（实现归属），如 <c>aliyunDevOps</c>。多个实例名可指向同一提供方。
    /// </summary>
    string Key { get; }

    /// <summary>
    /// 连接器实例名（配置中的 <c>name</c>），如 <c>yunxiao</c>。凭据按实例名取用。
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 显示名（供设置页 / 插件管理器展示）。
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// 该连接器派生的 MCP 暴露面（名称 → 客户端工厂）。无 MCP 时返回空集合。
    /// </summary>
    IReadOnlyList<ConnectorMcpExposure> McpExposures { get; }

    /// <summary>
    /// 初始化连接器（如回填持久化参数）。宿主在实例创建后调用<b>一次</b>：启用注册
    /// （<c>ConnectorRegistry.ApplyEnabledAsync</c>）与按 MCP 名派生（<c>CreateMcpClient</c>）
    /// 共用同一门禁——每个实例名只初始化一次，初始化失败则不缓存、下次可重试。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    ValueTask InitializeAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// 连接器派生的 MCP 暴露面：一个可在会话内注册为 MCP 客户端的能力出口。
/// </summary>
/// <remarks>
/// 工具 schema 由连接器实现以强类型代码定义（与 <see cref="IConnector"/> 的强类型方法一一对应），
/// 本结构只负责「如何造客户端」，不支持用配置覆盖工具集。
/// </remarks>
/// <param name="Name">MCP 名（如 <c>yunxiao</c>），亦是工具名前缀来源（<c>mcp_&lt;name&gt;_*</c>）。
/// 约定：无特殊理由时应与所属连接器的 <see cref="IConnector.Name"/> 一致，宿主因此可「按声明实例名直接命中」，
/// 无需为一次解析实例化全部已声明连接器。</param>
/// <param name="Description">MCP 描述。</param>
/// <param name="CreateClient">客户端工厂；第二参数为显式权限环境（<c>null</c> 视为宿主自身调用）。</param>
public sealed record ConnectorMcpExposure(
    string Name,
    string Description,
    Func<IServiceProvider, Plugins.Security.PluginPermissionScope?, MCP.IMCPClient> CreateClient);

/// <summary>
/// 连接器构建上下文：工厂据此实例化连接器（含实例名与非敏感参数）。
/// </summary>
public sealed class ConnectorBuildContext
{
    /// <summary>连接器实例名（配置中的 <c>name</c>）。</summary>
    public required string Name { get; init; }

    /// <summary>提供方键。</summary>
    public required string ProviderKey { get; init; }

    /// <summary>解析连接器依赖的容器（宿主级单例取根容器）。</summary>
    public required IServiceProvider Services { get; init; }

    /// <summary>非敏感参数（配置来源合并结果）。</summary>
    public ConnectorSettings Settings { get; init; } = ConnectorSettings.Empty;

    /// <summary>宿主配置（appsettings 层，可选）。</summary>
    public IConfiguration? Configuration { get; init; }
}

/// <summary>
/// 连接器非敏感参数包（<c>endpoint</c> / <c>organizationId</c> 等）。
/// </summary>
/// <remarks>
/// <b>仅承载非敏感参数</b>：PAT 等凭据一律走 <see cref="Plugins.Security.ICredentialStore"/>，
/// 不得写入任何配置文件。
/// </remarks>
public sealed class ConnectorSettings
{
    private readonly Dictionary<string, string?> _values;

    /// <summary>空参数包。</summary>
    public static ConnectorSettings Empty { get; } = new();

    /// <summary>
    /// 创建参数包。
    /// </summary>
    /// <param name="values">键值集合；<c>null</c> 视为空。</param>
    public ConnectorSettings(IDictionary<string, string?>? values = null)
        => _values = values is null
            ? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string?>(values, StringComparer.OrdinalIgnoreCase);

    /// <summary>只读键值视图。</summary>
    public IReadOnlyDictionary<string, string?> Values => _values;

    /// <summary>取原始字符串值。</summary>
    /// <param name="key">键（不区分大小写）。</param>
    /// <returns>值；缺失或空白时返回 <c>null</c>。</returns>
    public string? Get(string key)
        => _values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : null;

    /// <summary>取字符串值（缺失时返回 <paramref name="defaultValue"/>）。</summary>
    /// <param name="key">键。</param>
    /// <param name="defaultValue">默认值。</param>
    /// <returns>值。</returns>
    public string Get(string key, string defaultValue) => Get(key) ?? defaultValue;

    /// <summary>取整型值（缺失或非法时返回 <paramref name="defaultValue"/>）。</summary>
    /// <param name="key">键。</param>
    /// <param name="defaultValue">默认值。</param>
    /// <returns>值。</returns>
    public int GetInt(string key, int defaultValue)
        => int.TryParse(Get(key), out var parsed) ? parsed : defaultValue;

    /// <summary>取布尔值（缺失或非法时返回 <paramref name="defaultValue"/>）。</summary>
    /// <param name="key">键。</param>
    /// <param name="defaultValue">默认值。</param>
    /// <returns>值。</returns>
    public bool GetBool(string key, bool defaultValue)
        => bool.TryParse(Get(key), out var parsed) ? parsed : defaultValue;
}