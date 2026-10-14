/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Configuration
*文件名： McpOptions
*描述：MCP 运行期配置（绑定到 LuBanAgent:Mcp）
*****************************************************************************/
namespace LuBan.AIAgent.Configuration;

/// <summary>
/// MCP 运行期配置。绑定到 <c>LuBanAgent:Mcp</c>。
/// </summary>
public class McpOptions
{
    /// <summary>单个 MCP 服务器连接超时（秒）。</summary>
    public int ConnectTimeoutSeconds { get; set; } = 10;
}