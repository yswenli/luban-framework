/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.MCP
*文件名： IMCPClient
*版本号： V1.0.0.0
*唯一标识：69dbe7e3-5f2b-42c5-9b4b-2d1b432b6102
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：MCP (Model Context Protocol) 客户端接口
*
*=================================================
*修改标记
*修改时间：2026/7/27
*修改人： yswenli
*版本号： V1.0.0.0
*描述：MCP (Model Context Protocol) 客户端接口
*
*****************************************************************************/
namespace LuBan.AIAgent.MCP;

/// <summary>
/// MCP 客户端接口 - 与 MCP 服务器交互
/// </summary>
public interface IMCPClient
{
    /// <summary>
    /// MCP 服务器名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// MCP 服务器描述
    /// </summary>
    string Description { get; }

    /// <summary>
    /// 是否已连接
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// 连接到 MCP 服务器
    /// </summary>
    Task<bool> ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 断开连接
    /// </summary>
    Task DisconnectAsync();

    /// <summary>
    /// 获取可用的工具列表
    /// </summary>
    Task<IEnumerable<MCPTool>> ListToolsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 调用工具
    /// </summary>
    /// <param name="toolName">工具名称</param>
    /// <param name="arguments">参数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>执行结果</returns>
    Task<MCPToolResult> CallToolAsync(
        string toolName, 
        Dictionary<string, object?> arguments, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取可用的资源列表
    /// </summary>
    Task<IEnumerable<MCPResource>> ListResourcesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取资源
    /// </summary>
    Task<MCPResourceContent> ReadResourceAsync(
        string resourceUri, 
        CancellationToken cancellationToken = default);
}

/// <summary>
/// MCP 工具定义
/// </summary>
public class MCPTool
{
    /// <summary>
    /// 工具名称
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// 工具描述
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// 输入参数 schema (JSON Schema)
    /// </summary>
    public string? InputSchema { get; set; }
}

/// <summary>
/// MCP 工具执行结果
/// </summary>
public class MCPToolResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 结果内容
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// 是否需要用户确认
    /// </summary>
    public bool RequiresConfirmation { get; set; }
}

/// <summary>
/// MCP 资源定义
/// </summary>
public class MCPResource
{
    /// <summary>
    /// 资源 URI
    /// </summary>
    public string Uri { get; set; } = "";

    /// <summary>
    /// 资源名称
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// 资源描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// MIME 类型
    /// </summary>
    public string? MimeType { get; set; }
}

/// <summary>
/// MCP 资源内容
/// </summary>
public class MCPResourceContent
{
    /// <summary>
    /// 资源 URI
    /// </summary>
    public string Uri { get; set; } = "";

    /// <summary>
    /// 内容
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// MIME 类型
    /// </summary>
    public string? MimeType { get; set; }
}