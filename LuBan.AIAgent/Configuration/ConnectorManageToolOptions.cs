/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Configuration
*文件名： ConnectorManageToolOptions
*描述：连接器管理工具组配置（绑定到 LuBanAgent:Tools:ConnectorManage）
*****************************************************************************/
namespace LuBan.AIAgent.Configuration;

/// <summary>
/// 连接器管理工具组配置。绑定到 <c>LuBanAgent:Tools:ConnectorManage</c>。
/// </summary>
/// <remarks>
/// 对话生成连接器（PRD §4.12 C 路线）的落点：模型经本工具组把「提供方 + 非敏感参数 + 启用」
/// 写进工作区 <c>.luban-agent/connectors/&lt;name&gt;.json</c>；属厂商知识的提供方键必须已由
/// 宿主 / 产品 / 插件注册，工具组只做「配置驱动」，不做厂商硬编码。
/// </remarks>
public class ConnectorManageToolOptions
{
    /// <summary>是否启用连接器管理工具组。</summary>
    public bool Enabled { get; set; } = true;
}