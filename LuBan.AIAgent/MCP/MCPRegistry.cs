/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.MCP
*文件名： MCPRegistry
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：MCP 注册表
*
*****************************************************************************/
namespace LuBan.AIAgent.MCP;

/// <summary>
/// MCP 注册表（内置客户端缓存 + 外部服务器实例池，按配置同步）
/// </summary>
/// <remarks>
/// 内置与外部名称冲突时内置优先；冲突在 /mcp add 时拦截（命令层）。
/// </remarks>
public class MCPRegistry
{
    private readonly Dictionary<string, IMCPClient> _builtinClients = new();
    private readonly Dictionary<string, StdioMCPClient> _externalPool = new(StringComparer.OrdinalIgnoreCase);
    private readonly Configuration.ConfigManager? _configManager;

    /// <summary>
    /// 创建 MCPRegistry 实例
    /// </summary>
    /// <param name="clients">DI 注册的内置客户端</param>
    /// <param name="configManager">配置管理器（可选）</param>
    public MCPRegistry(IEnumerable<IMCPClient> clients, Configuration.ConfigManager? configManager = null)
    {
        foreach (var client in clients)
        {
            _builtinClients[client.Name.ToLowerInvariant()] = client;
        }
        _configManager = configManager;
    }

    private void SyncExternalPool()
    {
        if (_configManager == null) return;

        var enabledNames = _configManager.McpServers
            .Where(s => s.Enabled)
            .Select(s => s.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // 移除已删除或禁用的实例（先断开）
        foreach (var name in _externalPool.Keys.ToList())
        {
            if (!enabledNames.Contains(name))
            {
                var client = _externalPool[name];
                _externalPool.Remove(name);
                if (client.IsConnected)
                {
                    try { client.DisconnectAsync().GetAwaiter().GetResult(); } catch { }
                }
            }
        }

        // 新增缺失的实例（跳过与内置同名的，内置优先）
        foreach (var cfg in _configManager.McpServers.Where(s => s.Enabled))
        {
            if (!_externalPool.ContainsKey(cfg.Name)
                && !_builtinClients.ContainsKey(cfg.Name.ToLowerInvariant()))
            {
                _externalPool[cfg.Name] = new StdioMCPClient(cfg);
            }
        }
    }

    /// <summary>
    /// 获取所有客户端（内置按 DisabledBuiltinMcpClients 过滤 + 启用的外部实例）
    /// </summary>
    public IReadOnlyList<IMCPClient> GetAll()
    {
        SyncExternalPool();

        var disabledBuiltin = _configManager?.DisabledBuiltinMcpClients;
        var result = _builtinClients
            .Where(kv => disabledBuiltin?.Contains(kv.Key) != true)
            .Select(kv => kv.Value)
            .ToList();

        result.AddRange(_externalPool.Values);
        return result;
    }

    /// <summary>
    /// 根据名称获取客户端
    /// </summary>
    public IMCPClient? Get(string name)
    {
        SyncExternalPool();

        var key = name.ToLowerInvariant();
        if (_configManager?.DisabledBuiltinMcpClients.Contains(key) != true
            && _builtinClients.TryGetValue(key, out var builtin))
        {
            return builtin;
        }

        return _externalPool.TryGetValue(key, out var external) ? external : null;
    }

    /// <summary>
    /// 判断指定名称是否为内置客户端
    /// </summary>
    public bool IsBuiltin(string name) => _builtinClients.ContainsKey(name.ToLowerInvariant());

    /// <summary>
    /// 连接所有 MCP 客户端
    /// </summary>
    public async Task<Dictionary<string, bool>> ConnectAllAsync(CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, bool>();
        foreach (var client in GetAll())
        {
            try
            {
                var connected = await client.ConnectAsync(cancellationToken);
                results[client.Name] = connected;
            }
            catch
            {
                results[client.Name] = false;
            }
        }
        return results;
    }

    /// <summary>
    /// 获取所有可用的工具
    /// </summary>
    public async Task<Dictionary<string, IEnumerable<MCPTool>>> GetAllToolsAsync(CancellationToken cancellationToken = default)
    {
        var tools = new Dictionary<string, IEnumerable<MCPTool>>();
        foreach (var client in GetAll().Where(c => c.IsConnected))
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
    /// 调用工具
    /// </summary>
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
}
