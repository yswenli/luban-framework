/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.MCP
*文件名： MCPClientBase
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：MCP 客户端基类
*
*****************************************************************************/
namespace LuBan.AIAgent.MCP;

/// <summary>
/// MCP 客户端基类
/// </summary>
public abstract class MCPClientBase : IMCPClient
{
    /// <summary>
    /// MCP 服务器名称
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// MCP 服务器描述
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// 是否已连接
    /// </summary>
    public bool IsConnected { get; protected set; }

    /// <summary>
    /// 连接到 MCP 服务器
    /// </summary>
    public abstract Task<bool> ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 断开连接
    /// </summary>
    public virtual Task DisconnectAsync()
    {
        IsConnected = false;
        return Task.CompletedTask;
    }

    /// <summary>
    /// 获取可用的工具列表
    /// </summary>
    public abstract Task<IEnumerable<MCPTool>> ListToolsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 调用工具
    /// </summary>
    public abstract Task<MCPToolResult> CallToolAsync(
        string toolName,
        Dictionary<string, object?> arguments,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取可用的资源列表
    /// </summary>
    public virtual Task<IEnumerable<MCPResource>> ListResourcesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IEnumerable<MCPResource>>(new List<MCPResource>());

    /// <summary>
    /// 读取资源
    /// </summary>
    public virtual Task<MCPResourceContent> ReadResourceAsync(
        string resourceUri,
        CancellationToken cancellationToken = default)
        => Task.FromResult(new MCPResourceContent { Uri = resourceUri });

    /// <summary>
    /// 创建成功结果
    /// </summary>
    protected static MCPToolResult Ok(string content) => new() { Success = true, Content = content };

    /// <summary>
    /// 创建失败结果
    /// </summary>
    protected static MCPToolResult Fail(string error) => new() { Success = false, Error = error };
}