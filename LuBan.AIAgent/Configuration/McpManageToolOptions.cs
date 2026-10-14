/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Configuration
*文件名： McpManageToolOptions
*描述：MCP 管理工具组配置（绑定到 LuBanAgent:Tools:McpManage）
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