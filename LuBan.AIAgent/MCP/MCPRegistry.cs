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
/// MCP 注册表 - 管理所有 MCP 客户端
/// </summary>
public class MCPRegistry
{
    private readonly Dictionary<string, IMCPClient> _clients = new();

    /// <summary>
    /// 创建 MCP 注册表实例
    /// </summary>
    /// <param name="clients">所有注册的 MCP 客户端</param>
    public MCPRegistry(IEnumerable<IMCPClient> clients)
    {
        foreach (var client in clients)
        {
            _clients[client.Name] = client;
        }
    }

    /// <summary>
    /// 获取所有 MCP 客户端
    /// </summary>
    public IReadOnlyList<IMCPClient> GetAll() => _clients.Values.ToList();

    /// <summary>
    /// 根据名称获取 MCP 客户端
    /// </summary>
    public IMCPClient? Get(string name) => _clients.TryGetValue(name, out var client) ? client : null;

    /// <summary>
    /// 连接所有 MCP 客户端
    /// </summary>
    public async Task<Dictionary<string, bool>> ConnectAllAsync(CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, bool>();
        foreach (var client in _clients.Values)
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
        foreach (var client in _clients.Values.Where(c => c.IsConnected))
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