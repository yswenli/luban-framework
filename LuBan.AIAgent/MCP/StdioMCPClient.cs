/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.MCP
*文件名： StdioMCPClient
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/28
*描述：基于 stdio JSON-RPC 的外部 MCP 客户端
*
*****************************************************************************/
namespace LuBan.AIAgent.MCP;

/// <summary>
/// 基于 stdio JSON-RPC 的外部 MCP 客户端（Task 13 补全实现）
/// </summary>
public class StdioMCPClient : IMCPClient
{
    private readonly Configuration.McpServerConfig _config;

    /// <summary>
    /// 创建 StdioMCPClient 实例
    /// </summary>
    /// <param name="config">外部 MCP 服务器配置</param>
    public StdioMCPClient(Configuration.McpServerConfig config)
    {
        _config = config;
    }

    /// <inheritdoc />
    public string Name => _config.Name;

    /// <inheritdoc />
    public string Description => _config.Description;

    /// <inheritdoc />
    public bool IsConnected => false;

    /// <inheritdoc />
    public Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(false);

    /// <inheritdoc />
    public Task DisconnectAsync() => Task.CompletedTask;

    /// <inheritdoc />
    public Task<IEnumerable<MCPTool>> ListToolsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Enumerable.Empty<MCPTool>());

    /// <inheritdoc />
    public Task<MCPToolResult> CallToolAsync(string toolName, Dictionary<string, object?> arguments, CancellationToken cancellationToken = default)
        => Task.FromResult(new MCPToolResult { Success = false, Error = "StdioMCPClient 尚未实现" });

    /// <inheritdoc />
    public Task<IEnumerable<MCPResource>> ListResourcesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Enumerable.Empty<MCPResource>());

    /// <inheritdoc />
    public Task<MCPResourceContent> ReadResourceAsync(string resourceUri, CancellationToken cancellationToken = default)
        => Task.FromResult(new MCPResourceContent { Uri = resourceUri, Content = null });
}
