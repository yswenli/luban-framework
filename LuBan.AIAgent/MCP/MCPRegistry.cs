/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.MCP
*文件名： MCPRegistry
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：MCP 注册表，管理硬编码、工作区与 config.json 三级优先级的 MCP 客户端
*
*****************************************************************************/
namespace LuBan.AIAgent.MCP;

/// <summary>
/// MCP 注册表（硬编码 + 工作区 + config.json，三级优先级）
/// </summary>
public class MCPRegistry
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly Dictionary<string, IMCPClient> _hardcoded = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, (IMCPClient Client, string Fingerprint)> _workspace = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, (IMCPClient Client, string Fingerprint)> _config = new(StringComparer.OrdinalIgnoreCase);
    private readonly Configuration.IAppConfigReader? _configReader;
    private List<IMCPClient> _merged = new();

    /// <summary>
    /// 创建 MCPRegistry 实例
    /// </summary>
    /// <param name="clients">硬编码的内置 MCP 客户端集合</param>
    /// <param name="configReader">config.json 配置读取器，可选</param>
    public MCPRegistry(IEnumerable<IMCPClient> clients, Configuration.IAppConfigReader? configReader = null)
    {
        _configReader = configReader;
        foreach (var client in clients)
            _hardcoded[client.Name] = client;
        LoadFromConfig();
    }

    private static string FingerprintOf(Configuration.McpServerConfig cfg)
        => cfg.Command + "\0" + string.Join("\0", cfg.Args);

    /// <summary>
    /// 从工作区目录加载 MCP 配置（.luban-agent/mcps 下的 *.json 文件）
    /// </summary>
    /// <param name="workspaceDir">工作区目录</param>
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026", 
        Justification = "JSON 序列化仅用于简单配置类型，已通过 JsonSerializerOptions 处理")]
    public void LoadFromWorkspace(string workspaceDir)
    {
        var temp = new Dictionary<string, (IMCPClient Client, string Fingerprint)>(StringComparer.OrdinalIgnoreCase);
        var mcpsDir = Path.Combine(workspaceDir, ".luban-agent", "mcps");

        // 用户级（低优先级，先加载）：~/.luban-agent/mcps
        if (Directory.Exists(GlobalLubanAgentPath.McpsDir))
        {
            foreach (var jsonFile in Directory.GetFiles(GlobalLubanAgentPath.McpsDir, "*.json"))
            {
                try
                {
                    var json = File.ReadAllText(jsonFile);
                    var config = json.ToObject<Configuration.McpServerConfig>();
                    if (config != null && config.Enabled)
                    {
                        var client = MCPClientFactory.Create(config);
                        temp[config.Name] = (client, FingerprintOf(config));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"加载用户级 MCP 失败: {jsonFile}", ex);
                }
            }
        }

        if (Directory.Exists(mcpsDir))
        {
            foreach (var jsonFile in Directory.GetFiles(mcpsDir, "*.json"))
            {
                try
                {
                    var json = File.ReadAllText(jsonFile);
                    var config = json.ToObject<Configuration.McpServerConfig>();
                    if (config != null && config.Enabled)
                    {
                        var client = MCPClientFactory.Create(config);
                        temp[config.Name] = (client, FingerprintOf(config));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"加载工作区 MCP 失败: {jsonFile}", ex);
                }
            }
        }

        _lock.EnterWriteLock();
        try
        {
            // 只断开/释放配置变更的客户端；指纹未变的复用
            foreach (var kvp in _workspace)
            {
                if (temp.TryGetValue(kvp.Key, out var updated) && updated.Fingerprint == kvp.Value.Fingerprint)
                {
                    temp[kvp.Key] = kvp.Value; // 复用
                    continue;
                }

                if (kvp.Value.Client.IsConnected)
                {
                    try { kvp.Value.Client.DisconnectAsync().GetAwaiter().GetResult(); }
                    catch (Exception ex) { Logger.Debug("断开 MCP 客户端失败", ex); }
                }
                (kvp.Value.Client as IDisposable)?.Dispose();
            }
            
            _workspace.Clear();
            foreach (var kvp in temp)
                _workspace[kvp.Key] = kvp.Value;
            RebuildMerged();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// 从 config.json 加载 MCP 配置
    /// </summary>
    public void LoadFromConfig()
    {
        var temp = new Dictionary<string, (IMCPClient Client, string Fingerprint)>(StringComparer.OrdinalIgnoreCase);
        
        try
        {
            if (_configReader != null)
            {
                foreach (var cfg in _configReader.McpServers.Where(s => s.Enabled))
                {
                    var client = MCPClientFactory.Create(cfg);
                    temp[cfg.Name] = (client, FingerprintOf(cfg));
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error("加载 config.json MCPs 失败", ex);
        }

        _lock.EnterWriteLock();
        try
        {
            // 断开并释放已移除或配置变更的旧客户端；配置未变的实例保留复用
            foreach (var kvp in _config)
            {
                if (temp.TryGetValue(kvp.Key, out var updated) && updated.Fingerprint == kvp.Value.Fingerprint)
                {
                    temp[kvp.Key] = kvp.Value;
                    continue;
                }

                if (kvp.Value.Client.IsConnected)
                {
                    try { kvp.Value.Client.DisconnectAsync().GetAwaiter().GetResult(); }
                    catch (Exception ex) { Logger.Debug("断开 MCP 客户端失败", ex); }
                }
                (kvp.Value.Client as IDisposable)?.Dispose();
            }

            _config.Clear();
            foreach (var kvp in temp)
                _config[kvp.Key] = kvp.Value;
            RebuildMerged();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// 重新加载 MCP 配置（config.json，可选包含工作区配置）
    /// </summary>
    /// <param name="workspaceDir">工作区目录，可选</param>
    public void Reload(string? workspaceDir = null)
    {
        LoadFromConfig();
        if (workspaceDir != null)
            LoadFromWorkspace(workspaceDir);
    }

    /// <summary>
    /// 获取合并后的全部 MCP 客户端列表
    /// </summary>
    /// <returns>合并后的 MCP 客户端列表</returns>
    public IReadOnlyList<IMCPClient> GetAll()
    {
        _lock.EnterReadLock();
        try
        {
            return _merged.ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// 按名称获取 MCP 客户端
    /// </summary>
    /// <param name="name">客户端名称</param>
    /// <returns>匹配的 MCP 客户端，未找到时返回 null</returns>
    public IMCPClient? Get(string name)
    {
        _lock.EnterReadLock();
        try
        {
            return _merged.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// 判断是否包含指定名称的 MCP 客户端
    /// </summary>
    /// <param name="name">客户端名称</param>
    /// <returns>包含时返回 true，否则返回 false</returns>
    public bool Contains(string name)
    {
        _lock.EnterReadLock();
        try
        {
            return _merged.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// 判断是否为硬编码的内置 MCP 客户端
    /// </summary>
    /// <param name="name">客户端名称</param>
    /// <returns>内置时返回 true，否则返回 false</returns>
    public bool IsBuiltin(string name) => _hardcoded.ContainsKey(name);

    /// <summary>
    /// 注册一个进程内插件 MCP 客户端（不走 stdio/http 子进程通道）。
    /// </summary>
    /// <remarks>
    /// 插件贡献的 MCP 客户端（如进程内 <c>YunxiaoMCPClient</c>）无法用文件式 <c>mcps/*.json</c>
    /// 描述（<c>McpServerConfig.Command</c> 为必填，会强制外部进程，且 PAT 需明文落盘）。
    /// 故提供此实例注册口，随插件启用注入、禁用/卸载时经
    /// <see cref="UnregisterPluginClient"/> 显式注销——<b>注销必须先于 ALC 卸载</b>，
    /// 否则注册表持有的强引用会导致插件程序集无法回收。
    /// </remarks>
    /// <param name="client">进程内 MCP 客户端实例</param>
    /// <returns>是否注册成功（同名已存在时覆盖并返回 false）</returns>
    public bool RegisterPluginClient(IMCPClient client)
    {
        ArgumentNullException.ThrowIfNull(client);

        _lock.EnterWriteLock();
        try
        {
            var overwritten = _hardcoded.ContainsKey(client.Name);
            _hardcoded[client.Name] = client;
            RebuildMerged();
            return !overwritten;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// 注销一个进程内插件 MCP 客户端。
    /// </summary>
    /// <remarks>
    /// 必须先断开连接并释放实例，再从注册表移除，确保插件程序集可被 ALC 回收。
    /// 内置客户端（随框架 DI 注入）不应经此方法移除。
    /// </remarks>
    /// <param name="name">客户端名称</param>
    /// <returns>是否成功注销（未找到时返回 false）</returns>
    public bool UnregisterPluginClient(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        IMCPClient? removed = null;
        _lock.EnterWriteLock();
        try
        {
            if (_hardcoded.Remove(name, out var client))
            {
                removed = client;
                RebuildMerged();
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        if (removed == null)
            return false;

        try
        {
            if (removed.IsConnected)
                removed.DisconnectAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Logger.Debug($"注销插件 MCP 客户端时断开连接失败: {name}", ex);
        }

        (removed as IDisposable)?.Dispose();
        return true;
    }

    /// <summary>
    /// 连接全部非内置的 MCP 客户端。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>客户端名称到连接结果的字典（不含内置客户端）。</returns>
    public async Task<Dictionary<string, bool>> ConnectAllAsync(CancellationToken cancellationToken = default)
    {
        var results = new System.Collections.Concurrent.ConcurrentDictionary<string, bool>();
        // 跳过已连接的客户端（内置 MCP 恒 IsConnected=true）
        var clients = GetAll().Where(c => !c.IsConnected).ToList();
        
        if (clients.Count == 0)
            return new Dictionary<string, bool>();

        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedCts.CancelAfter(TimeSpan.FromSeconds(30)); // 总体超时兜底

        var tasks = clients.Select(async client =>
        {
            try
            {
                var connected = await client.ConnectAsync(linkedCts.Token).ConfigureAwait(false);
                results[client.Name] = connected;
            }
            catch (OperationCanceledException)
            {
                results[client.Name] = false;
            }
            catch (Exception ex)
            {
                Logger.Debug($"MCP '{client.Name}' 连接失败", ex);
                results[client.Name] = false;
            }
        });

        await Task.WhenAll(tasks).ConfigureAwait(false);

        return new Dictionary<string, bool>(results);
    }

    /// <summary>
    /// 获取所有已连接 MCP 客户端的工具列表
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>客户端名称到工具列表的字典</returns>
    public async Task<Dictionary<string, IEnumerable<MCPTool>>> GetAllToolsAsync(CancellationToken cancellationToken = default)
    {
        var tools = new Dictionary<string, IEnumerable<MCPTool>>();
        var clients = GetAll().Where(c => c.IsConnected);
        
        foreach (var client in clients)
        {
            try
            {
                var clientTools = await client.ListToolsAsync(cancellationToken);
                tools[client.Name] = clientTools;
            }
            catch
            {
                tools[client.Name] = Enumerable.Empty<MCPTool>();
            }
        }
        return tools;
    }

    /// <summary>
    /// 调用指定 MCP 客户端的工具
    /// </summary>
    /// <param name="clientName">客户端名称</param>
    /// <param name="toolName">工具名称</param>
    /// <param name="arguments">工具参数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>工具调用结果</returns>
    public async Task<MCPToolResult> CallToolAsync(
        string clientName,
        string toolName,
        Dictionary<string, object?> arguments,
        CancellationToken cancellationToken = default)
    {
        var client = Get(clientName);
        if (client == null)
            return new MCPToolResult { Success = false, Error = $"未找到 MCP 客户端: {clientName}" };

        if (!client.IsConnected)
            return new MCPToolResult { Success = false, Error = $"MCP 客户端 {clientName} 未连接" };

        return await client.CallToolAsync(toolName, arguments, cancellationToken);
    }

    /// <summary>
    /// 按优先级合并三层 MCP 配置，构建最终客户端列表
    /// </summary>
    private void RebuildMerged()
    {
        var merged = new Dictionary<string, IMCPClient>(StringComparer.OrdinalIgnoreCase);
        
        // 1. 最低优先级：config.json
        foreach (var kvp in _config)
            merged[kvp.Key] = kvp.Value.Client;
        
        // 2. 中优先级：工作区文件
        foreach (var kvp in _workspace)
            merged[kvp.Key] = kvp.Value.Client;
        
        // 3. 最高优先级：硬编码（排除被禁用的）
        var disabledBuiltin = _configReader?.DisabledBuiltinMcpClients;
        foreach (var kvp in _hardcoded)
        {
            if (disabledBuiltin?.Contains(kvp.Key) == true) continue;
            merged[kvp.Key] = kvp.Value;
        }

        _merged = merged.Values.ToList();
    }
}
