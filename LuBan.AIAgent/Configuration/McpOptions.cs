/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Configuration
*文件名： McpOptions
*版本号： V1.0.0.0
*唯一标识：d173531b-4280-427f-bbc1-ea3b7073babd
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/14
*描述：MCP 运行期配置（绑定到 LuBanAgent:Mcp）
*
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