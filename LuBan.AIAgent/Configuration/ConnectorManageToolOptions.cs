/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Configuration
*文件名： ConnectorManageToolOptions
*版本号： V1.0.0.0
*唯一标识：a1b91afe-5270-484a-94df-62d6da3b8fd2
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/20
*描述：连接器管理工具组配置（绑定到 LuBanAgent:Tools:ConnectorManage）
*
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