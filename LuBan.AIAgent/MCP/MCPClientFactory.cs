/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.MCP
*文件名： MCPClientFactory
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/19
*描述：外部 MCP 客户端工厂 —— 按传输方式收敛创建逻辑（stdio / http / sse）
*
*****************************************************************************/
namespace LuBan.AIAgent.MCP;

/// <summary>
/// 外部 MCP 客户端工厂：按 <see cref="Configuration.McpServerConfig.Transport"/> 创建对应客户端。
/// </summary>
/// <remarks>
/// 收敛原先散落三处的传输分派（<c>MCPRegistry.LoadFromWorkspace</c> ×2 与
/// <c>McpManageToolGroup.TestMcpServerAsync</c>），避免各写一套口径导致行为分叉。
/// <b>不</b>负责进程内连接器派生的 MCP 客户端（见 <c>ConnectorMcpExposure</c>）。
/// </remarks>
public static class MCPClientFactory
{
    /// <summary>
    /// 按配置的传输方式创建 MCP 客户端。
    /// </summary>
    /// <param name="config">外部 MCP 服务器配置。</param>
    /// <returns>对应传输方式的 MCP 客户端实例。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="config"/> 为 <c>null</c>。</exception>
    public static IMCPClient Create(Configuration.McpServerConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        return config.Transport?.Trim().ToLowerInvariant() switch
        {
            "http" or "sse" => new HttpMCPClient(config),
            _ => new StdioMCPClient(config)
        };
    }
}