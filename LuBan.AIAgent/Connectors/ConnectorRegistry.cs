/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Connectors
*文件名： ConnectorRegistry
*版本号： V1.0.0.0
*唯一标识：新建
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/19
*描述：连接器注册表（会话级）—— 声明/启用解析、MCP 客户端派生与注销
*
*****************************************************************************/
namespace LuBan.AIAgent.Connectors;

/// <summary>
/// 连接器状态（供设置页 / 插件管理器只读展示）。
/// </summary>
public enum ConnectorState
{
    /// <summary>提供方已注册（有工厂），尚未在 config.json 声明。</summary>
    Available,

    /// <summary>宿主已声明，但当前工作区未启用。</summary>
    Declared,

    /// <summary>已声明且已启用。</summary>
    Enabled,

    /// <summary>声明了提供方但无对应工厂（未安装 / 拼写错误）。</summary>
    Unavailable,

    /// <summary>实例化或初始化失败。</summary>
    Error
}

/// <summary>
/// 连接器状态项。
/// </summary>
/// <param name="Name">连接器实例名（或提供方键，见 <see cref="ConnectorState.Available"/>）。</param>
/// <param name="ProviderKey">提供方键。</param>
/// <param name="State">状态。</param>
/// <param name="Message">说明（供 UI 直接展示）。</param>
public sealed record ConnectorStatus(string Name, string ProviderKey, ConnectorState State, string Message);

/// <summary>
/// 连接器注册表（<b>会话级</b>）：把「声明（config.json）+ 启用（工作区）+ 工厂（提供方）」
/// 解析成可用的连接器与 MCP 客户端。
/// </summary>
/// <remarks>
/// <para>
/// 只负责<b>会话内的派生与记账</b>：连接器实例由单例 <see cref="ConnectorInstanceHost"/> 持有，
/// 本类不拥有其生命周期（MCP 客户端对连接器是借用关系）。
/// </para>
/// <para>
/// 优先级（工作区 &gt; config.json &gt; appsettings）：
/// 工作区 <c>.luban-agent/connectors/&lt;name&gt;.json</c> 只表达<b>启用/停用</b>，
/// 工作区启用了宿主未声明的连接器按「未安装」处理（告警、不放行）。
/// </para>
/// </remarks>
public sealed class ConnectorRegistry
{
    private readonly ConnectorInstanceHost _host;
    private readonly ConnectorFactoryRegistry _factories;
    private readonly MCP.MCPRegistry? _mcpRegistry;
    private readonly IAppConfigReader? _configReader;
    private readonly IConfiguration? _configuration;

    private readonly Dictionary<string, bool> _workspaceEnabled = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _errors = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>已执行过初始化的连接器实例名（<c>InitializeAsync</c> 契约：每实例一次）。</summary>
    private readonly HashSet<string> _initialized = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>已告警过的「工作区启用但宿主未声明」连接器名（避免每次解析都重复刷日志）。</summary>
    private readonly HashSet<string> _warnedUndeclared = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>本会话已注册的 MCP 客户端：MCP 名 → 客户端（用于注销，必须先于 ALC 卸载）。</summary>
    private readonly Dictionary<string, MCP.IMCPClient> _registered = new(StringComparer.OrdinalIgnoreCase);

    private bool _workspaceLoaded;

    /// <summary>
    /// 创建连接器注册表。
    /// </summary>
    /// <param name="host">连接器实例宿主（单例）。</param>
    /// <param name="factories">连接器工厂表（单例）。</param>
    /// <param name="mcpRegistry">会话 MCP 注册表（可选；缺省时只派生不注册）。</param>
    /// <param name="configReader">config.json 读取器（可选）。</param>
    /// <param name="configuration">宿主配置（appsettings 层，可选）。</param>
    public ConnectorRegistry(
        ConnectorInstanceHost host,
        ConnectorFactoryRegistry factories,
        MCP.MCPRegistry? mcpRegistry = null,
        IAppConfigReader? configReader = null,
        IConfiguration? configuration = null)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _factories = factories ?? throw new ArgumentNullException(nameof(factories));
        _mcpRegistry = mcpRegistry;
        _configReader = configReader;
        _configuration = configuration;
    }

    /// <summary>
    /// 从工作区加载连接器启用状态（<c>.luban-agent/connectors/*.json</c>，文件名为实例名）。
    /// </summary>
    /// <param name="workspaceDir">工作区目录。</param>
    [UnconditionalSuppressMessage("Trimming", "IL2026",
        Justification = "JSON 反序列化仅用于简单配置类型，已通过 JsonSerializerOptions 处理")]
    public void LoadFromWorkspace(string workspaceDir)
    {
        _workspaceEnabled.Clear();
        _workspaceLoaded = true;

        if (string.IsNullOrWhiteSpace(workspaceDir))
            return;

        var connectorsDir = Path.Combine(workspaceDir, ".luban-agent", "connectors");
        if (!Directory.Exists(connectorsDir))
            return;

        foreach (var jsonFile in Directory.GetFiles(connectorsDir, "*.json"))
        {
            var name = Path.GetFileNameWithoutExtension(jsonFile);
            if (string.IsNullOrWhiteSpace(name)) continue;

            try
            {
                var json = File.ReadAllText(jsonFile);
                var entry = json.ToObject<WorkspaceConnectorEntry>();
                _workspaceEnabled[name.Trim()] = entry?.Enabled ?? true;
            }
            catch (Exception ex)
            {
                Logger.Error($"加载工作区连接器失败: {jsonFile}", ex);
            }
        }
    }

    /// <summary>
    /// 取全部连接器状态（供设置页 / 插件管理器只读展示）。
    /// </summary>
    /// <param name="workspaceRoot">工作区根路径；传入时按需加载启用状态。</param>
    /// <returns>状态列表。</returns>
    public IReadOnlyList<ConnectorStatus> GetStates(string? workspaceRoot = null)
    {
        EnsureWorkspaceLoaded(workspaceRoot);
        WarnUndeclaredWorkspaceConnectors();

        var declared = GetDeclared();
        var result = new List<ConnectorStatus>();
        var declaredNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var config in declared)
        {
            if (string.IsNullOrWhiteSpace(config.Name)) continue;
            declaredNames.Add(config.Name);

            if (!_factories.Has(config.Provider))
            {
                result.Add(new ConnectorStatus(config.Name, config.Provider, ConnectorState.Unavailable,
                    $"提供方 '{config.Provider}' 未注册，连接器不可用。"));
                continue;
            }

            if (_errors.TryGetValue(config.Name, out var error))
            {
                result.Add(new ConnectorStatus(config.Name, config.Provider, ConnectorState.Error, error));
                continue;
            }

            result.Add(IsEnabled(config)
                ? new ConnectorStatus(config.Name, config.Provider, ConnectorState.Enabled, "已启用。")
                : new ConnectorStatus(config.Name, config.Provider, ConnectorState.Declared, "宿主已声明，当前工作区未启用。"));
        }

        foreach (var name in _workspaceEnabled.Keys.Where(k => !declaredNames.Contains(k)))
        {
            result.Add(new ConnectorStatus(name, string.Empty, ConnectorState.Unavailable,
                "工作区启用了宿主未声明的连接器，按未安装处理。"));
        }

        foreach (var providerKey in _factories.ProviderKeys.Where(p =>
                     !declared.Any(c => string.Equals(c.Provider, p, StringComparison.OrdinalIgnoreCase))))
        {
            result.Add(new ConnectorStatus(providerKey, providerKey, ConnectorState.Available,
                "提供方已注册，等待 config.json 声明。"));
        }

        return result;
    }

    /// <summary>
    /// 取已启用的连接器实例（按需实例化，不注册 MCP，<b>不</b>执行初始化）。
    /// </summary>
    /// <param name="workspaceRoot">工作区根路径（可选）。</param>
    /// <returns>已启用的连接器实例列表。</returns>
    public IReadOnlyList<IConnector> GetEnabledConnectors(string? workspaceRoot = null)
    {
        EnsureWorkspaceLoaded(workspaceRoot);
        WarnUndeclaredWorkspaceConnectors();

        var result = new List<IConnector>();
        foreach (var config in GetDeclared())
        {
            if (string.IsNullOrWhiteSpace(config.Name) || !IsEnabled(config)) continue;
            if (!_factories.Has(config.Provider)) continue;

            var connector = _host.GetOrCreate(config.Name, config.Provider, BuildSettings(config), _configuration);
            if (connector is not null) result.Add(connector);
        }

        return result;
    }

    /// <summary>
    /// 初始化并注册全部已启用连接器的 MCP 暴露面到会话注册表。
    /// </summary>
    /// <param name="workspaceRoot">工作区根路径（可选）。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    public async Task ApplyEnabledAsync(string? workspaceRoot = null, CancellationToken cancellationToken = default)
    {
        EnsureWorkspaceLoaded(workspaceRoot);
        WarnUndeclaredWorkspaceConnectors();

        foreach (var config in GetDeclared())
        {
            if (string.IsNullOrWhiteSpace(config.Name) || !IsEnabled(config)) continue;
            if (!_factories.Has(config.Provider)) continue;

            var connector = await GetInitializedConnectorAsync(config, cancellationToken).ConfigureAwait(false);
            if (connector is null) continue;

            foreach (var exposure in connector.McpExposures)
                RegisterClient(exposure.Name, () => exposure.CreateClient(_host.Root, null));
        }
    }

    /// <summary>
    /// 按 MCP 名派生一个 MCP 客户端（供插件清单声明解析；Q8 保持 <c>mcpServers</c> 语义不变）。
    /// </summary>
    /// <param name="mcpName">MCP 名（如默认实例名 <c>myconnector</c>，不区分大小写）。</param>
    /// <param name="scope">显式权限环境；<c>null</c> 视为宿主自身调用。</param>
    /// <param name="workspaceRoot">工作区根路径（可选）。</param>
    /// <returns>MCP 客户端；无匹配（或提供方未注册）时返回 <c>null</c>。</returns>
    /// <remarks>同步包装，等价于 <see cref="CreateMcpClientAsync"/>；异步宿主应优先用后者。</remarks>
    public MCP.IMCPClient? CreateMcpClient(
        string mcpName,
        Plugins.Security.PluginPermissionScope? scope = null,
        string? workspaceRoot = null)
        => CreateMcpClientAsync(mcpName, scope, workspaceRoot).GetAwaiter().GetResult();

    /// <summary>
    /// 按 MCP 名派生一个 MCP 客户端（异步版本）。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 查找顺序：<b>优先</b>按「声明实例名 == MCP 名」匹配（实例名即暴露面名的命名约定），
    /// 命中即返回，避免为一次解析实例化全部已声明连接器；未命中才依次探查其余连接器的暴露面。
    /// </para>
    /// <para>
    /// 命中的连接器会被实例化，并执行一次 <see cref="IConnector.InitializeAsync"/>
    /// （与 <see cref="ApplyEnabledAsync"/> 同口径：每实例一次，失败不缓存）。连接器实例归宿主所有，
    /// 本方法只派发客户端、不登记到会话 MCP 注册表（是否登记由调用方决定）。
    /// </para>
    /// </remarks>
    /// <param name="mcpName">MCP 名（如 <c>myconnector</c>，不区分大小写）。</param>
    /// <param name="scope">显式权限环境；<c>null</c> 视为宿主自身调用。</param>
    /// <param name="workspaceRoot">工作区根路径（可选）。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>MCP 客户端；无匹配（或提供方未注册）时返回 <c>null</c>。</returns>
    public async Task<MCP.IMCPClient?> CreateMcpClientAsync(
        string mcpName,
        Plugins.Security.PluginPermissionScope? scope = null,
        string? workspaceRoot = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(mcpName)) return null;

        EnsureWorkspaceLoaded(workspaceRoot);
        WarnUndeclaredWorkspaceConnectors();

        var target = mcpName.Trim();

        var candidates = GetDeclared()
            .Where(config => !string.IsNullOrWhiteSpace(config.Name) && _factories.Has(config.Provider))
            .OrderByDescending(config => string.Equals(config.Name.Trim(), target, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var config in candidates)
        {
            var connector = await GetInitializedConnectorAsync(config, cancellationToken).ConfigureAwait(false);
            if (connector is null) continue;

            foreach (var exposure in connector.McpExposures)
            {
                if (string.Equals(exposure.Name, target, StringComparison.OrdinalIgnoreCase))
                    return exposure.CreateClient(_host.Root, scope);
            }
        }

        return null;
    }

    /// <summary>
    /// 注销指定 MCP 名的进程内客户端（必须先于 ALC 卸载，避免钉住插件程序集）。
    /// </summary>
    /// <param name="mcpName">MCP 名。</param>
    /// <returns>是否注销成功。</returns>
    public bool Unregister(string mcpName)
    {
        if (string.IsNullOrWhiteSpace(mcpName)) return false;

        if (!_registered.Remove(mcpName, out _)) return false;

        _mcpRegistry?.UnregisterPluginClient(mcpName);
        return true;
    }

    /// <summary>
    /// 注销本会话注册的全部连接器 MCP 客户端。
    /// </summary>
    public void UnregisterAll()
    {
        foreach (var name in _registered.Keys.ToList())
            Unregister(name);
    }

    #region 内部实现

    /// <summary>
    /// 取「已初始化」的连接器实例（<see cref="IConnector.InitializeAsync"/> 每实例只调一次；失败不缓存、可重试）。
    /// </summary>
    /// <remarks>实例化或初始化失败时记录到 <see cref="_errors"/>（供 <see cref="GetStates"/> 展示）并返回 <c>null</c>，不抛出。</remarks>
    private async Task<IConnector?> GetInitializedConnectorAsync(ConnectorConfig config, CancellationToken cancellationToken)
    {
        try
        {
            var connector = _host.GetOrCreate(config.Name, config.Provider, BuildSettings(config), _configuration);
            if (connector is null) return null;

            if (_initialized.Add(config.Name.Trim()))
            {
                try
                {
                    await connector.InitializeAsync(cancellationToken).ConfigureAwait(false);
                }
                catch
                {
                    _initialized.Remove(config.Name.Trim());
                    throw;
                }
            }

            return connector;
        }
        catch (Exception ex)
        {
            _errors[config.Name] = $"初始化失败: {ex.Message}";
            Logger.Error($"初始化连接器失败: {config.Name}", ex);
            return null;
        }
    }

    /// <summary>
    /// 工作区启用了宿主未声明的连接器：告警一次并按「未安装」处理（规范要求：告警 + 跳过，不中断）。
    /// </summary>
    private void WarnUndeclaredWorkspaceConnectors()
    {
        if (_workspaceEnabled.Count == 0) return;

        var declared = GetDeclared();
        foreach (var name in _workspaceEnabled.Keys)
        {
            if (!_warnedUndeclared.Add(name)) continue;
            if (declared.Any(config => string.Equals(config?.Name?.Trim(), name, StringComparison.OrdinalIgnoreCase))) continue;

            Logger.Warn($"工作区启用了宿主未声明的连接器 '{name}'，按未安装处理（请在 config.json:Connectors 内声明）。");
        }
    }

    private void RegisterClient(string name, Func<MCP.IMCPClient> factory)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        if (_mcpRegistry is null) return;
        if (_mcpRegistry.Contains(name)) return;

        try
        {
            var client = factory();
            _mcpRegistry.RegisterPluginClient(client);
            _registered[name] = client;
        }
        catch (Exception ex)
        {
            Logger.Warn($"注册连接器 MCP 客户端失败: {name}", ex);
        }
    }

    private void EnsureWorkspaceLoaded(string? workspaceRoot)
    {
        if (_workspaceLoaded || string.IsNullOrWhiteSpace(workspaceRoot)) return;
        LoadFromWorkspace(workspaceRoot!);
    }

    private List<ConnectorConfig> GetDeclared()
    {
        try
        {
            return _configReader?.Connectors ?? new List<ConnectorConfig>();
        }
        catch (Exception ex)
        {
            Logger.Error("读取 config.json 连接器声明失败", ex);
            return new List<ConnectorConfig>();
        }
    }

    private bool IsEnabled(ConnectorConfig config)
        => _workspaceEnabled.TryGetValue(config.Name.Trim(), out var enabled) ? enabled : config.Enabled;

    /// <summary>合并非敏感参数：config.json 为基，appsettings <c>LuBanAgent:Connectors:&lt;实例名&gt;</c> 覆盖。</summary>
    private ConnectorSettings BuildSettings(ConnectorConfig config)
    {
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in config.Settings ?? new Dictionary<string, string?>())
            values[kvp.Key] = kvp.Value;

        var section = _configuration?.GetSection($"LuBanAgent:Connectors:{config.Name}");
        if (section is not null)
        {
            foreach (var child in section.GetChildren())
            {
                // AccessToken 属凭据，绝不进入参数包
                if (string.Equals(child.Key, "AccessToken", StringComparison.OrdinalIgnoreCase)) continue;
                values[child.Key] = child.Value;
            }
        }

        return new ConnectorSettings(values);
    }

    /// <summary>工作区连接器启用文件（<c>.luban-agent/connectors/&lt;name&gt;.json</c>）。</summary>
    private sealed class WorkspaceConnectorEntry
    {
        public bool Enabled { get; set; } = true;
    }

    #endregion
}