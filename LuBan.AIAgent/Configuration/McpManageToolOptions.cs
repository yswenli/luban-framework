/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Configuration
*文件名： McpManageToolOptions
*版本号： V1.0.0.0
*唯一标识：e9f4b1e8-2ae6-4298-bc13-55decc909936
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/14
*描述：MCP 管理工具组配置（绑定到 LuBanAgent:Tools:McpManage）
*
*****************************************************************************/

namespace LuBan.AIAgent.Configuration;

/// <summary>
/// MCP 管理工具组配置。绑定到 <c>LuBanAgent:Tools:McpManage</c>。
/// </summary>
public class McpManageToolOptions
{
    /// <summary>是否启用 MCP 管理工具组。</summary>
    public bool Enabled { get; set; } = true;
}