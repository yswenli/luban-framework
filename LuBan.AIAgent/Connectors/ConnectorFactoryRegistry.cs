/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Connectors
*文件名： ConnectorFactoryRegistry
*版本号： V1.0.0.0
*唯一标识：新建
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/19
*描述：连接器工厂表与宿主级实例宿主（单例）
*
*****************************************************************************/
namespace LuBan.AIAgent.Connectors;

/// <summary>
/// 连接器工厂注册项：把提供方键映射到「按上下文造连接器」的工厂。
/// </summary>
/// <param name="ProviderKey">提供方键（如 <c>aliyunDevOps</c>）。</param>
/// <param name="Factory">连接器工厂。</param>
/// <param name="DisplayName">提供方显示名（可选）。</param>
public sealed record ConnectorFactoryRegistration(
    string ProviderKey,
    Func<ConnectorBuildContext, IConnector> Factory,
    string? DisplayName = null);

/// <summary>
/// 连接器工厂表（<b>单例</b>）：由 DI 在构建期收集，运行期只读查询。
/// </summary>
/// <remarks>
/// 只登记「有哪些提供方可用」，不代表已启用；启用由
/// <see cref="ConnectorRegistry"/> 结合配置判定（PRD §3.10 连接器配置化）。
/// </remarks>
public sealed class ConnectorFactoryRegistry
{
    private readonly Dictionary<string, ConnectorFactoryRegistration> _registrations;

    /// <summary>
    /// 创建工厂表。
    /// </summary>
    /// <param name="registrations">DI 收集到的全部工厂注册项（同名时后者覆盖前者）。</param>
    public ConnectorFactoryRegistry(IEnumerable<ConnectorFactoryRegistration> registrations)
    {
        _registrations = new Dictionary<string, ConnectorFactoryRegistration>(StringComparer.OrdinalIgnoreCase);

        foreach (var registration in registrations ?? Enumerable.Empty<ConnectorFactoryRegistration>())
        {
            if (registration is null || string.IsNullOrWhiteSpace(registration.ProviderKey)) continue;
            _registrations[registration.ProviderKey.Trim()] = registration;
        }
    }

    /// <summary>已注册的提供方键集合。</summary>
    public IReadOnlyCollection<string> ProviderKeys => _registrations.Keys;

    /// <summary>
    /// 是否存在该提供方的工厂。
    /// </summary>
    /// <param name="providerKey">提供方键。</param>
    /// <returns>存在时返回 <c>true</c>。</returns>
    public bool Has(string providerKey)
        => !string.IsNullOrWhiteSpace(providerKey) && _registrations.ContainsKey(providerKey.Trim());

    /// <summary>
    /// 取提供方工厂注册项。
    /// </summary>
    /// <param name="providerKey">提供方键。</param>
    /// <param name="registration">命中的注册项。</param>
    /// <returns>命中时返回 <c>true</c>。</returns>
    public bool TryGet(string providerKey, out ConnectorFactoryRegistration registration)
    {
        if (string.IsNullOrWhiteSpace(providerKey))
        {
            registration = null!;
            return false;
        }

        return _registrations.TryGetValue(providerKey.Trim(), out registration!);
    }
}

/// <summary>
/// 连接器实例宿主（<b>单例</b>）：按实例名缓存连接器，保证「连接器 = 宿主级单例」。
/// </summary>
/// <remarks>
/// <para>
/// 实例用<b>根容器</b>解析依赖（连接器依赖的 provider / 凭据提供者均为单例），
/// 故不受会话 scope 影响；会话级 MCP 客户端只借用实例，不拥有其生命周期。
/// </para>
/// <para>
/// 配置声明的<b>非敏感参数</b>按「工作区 &gt; config.json &gt; appsettings」合并后经
/// <see cref="ConnectorBuildContext"/> 传入；同名的首建参数生效（后续声明变更不重建实例）。
/// </para>
/// </remarks>
public sealed class ConnectorInstanceHost : IAsyncDisposable
{
    private readonly IServiceProvider _root;
    private readonly ConnectorFactoryRegistry _factories;
    private readonly ConcurrentDictionary<string, IConnector> _instances = new(StringComparer.OrdinalIgnoreCase);
    private bool _disposed;

    /// <summary>
    /// 创建实例宿主。
    /// </summary>
    /// <param name="root">根容器。</param>
    /// <param name="factories">工厂表。</param>
    public ConnectorInstanceHost(IServiceProvider root, ConnectorFactoryRegistry factories)
    {
        _root = root ?? throw new ArgumentNullException(nameof(root));
        _factories = factories ?? throw new ArgumentNullException(nameof(factories));
    }

    /// <summary>
    /// 根容器（连接器暴露面工厂解析依赖用）。
    /// </summary>
    public IServiceProvider Root => _root;

    /// <summary>
    /// 取（或创建）指定实例名的连接器。
    /// </summary>
    /// <param name="name">实例名。</param>
    /// <param name="providerKey">提供方键。</param>
    /// <param name="settings">非敏感参数（可选）。</param>
    /// <param name="configuration">宿主配置（可选）。</param>
    /// <returns>连接器实例；无对应工厂时返回 <c>null</c>。</returns>
    public IConnector? GetOrCreate(
        string name,
        string providerKey,
        ConnectorSettings? settings = null,
        IConfiguration? configuration = null)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ConnectorInstanceHost));
        if (string.IsNullOrWhiteSpace(name)) return null;
        if (!_factories.TryGet(providerKey, out var registration)) return null;

        var key = name.Trim();

        return _instances.GetOrAdd(key, _ => registration.Factory(new ConnectorBuildContext
        {
            Name = key,
            ProviderKey = registration.ProviderKey,
            Services = _root,
            Settings = settings ?? ConnectorSettings.Empty,
            Configuration = configuration
        }));
    }

    /// <summary>
    /// 取已创建的连接器实例（不触发创建）。
    /// </summary>
    /// <param name="name">实例名。</param>
    /// <param name="connector">命中的实例。</param>
    /// <returns>命中时返回 <c>true</c>。</returns>
    public bool TryGet(string name, out IConnector connector)
    {
        if (!string.IsNullOrWhiteSpace(name) && _instances.TryGetValue(name.Trim(), out var found))
        {
            connector = found;
            return true;
        }

        connector = null!;
        return false;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var connector in _instances.Values)
        {
            try
            {
                await connector.DisposeAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Debug($"释放连接器实例失败: {connector.Name}", ex);
            }
        }

        _instances.Clear();
    }
}