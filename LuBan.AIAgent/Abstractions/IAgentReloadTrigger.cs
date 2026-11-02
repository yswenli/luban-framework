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
*命名空间：LuBan.AIAgent.Abstractions
*文件名： IAgentReloadTrigger
*版本号： V1.0.0.0
*唯一标识：9491432f-2aaf-4d6d-a55c-aa4ccdf7ae27
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/14
*描述：Agent 上下文重载触发器接口，用于延迟重建 Agent
*
*****************************************************************************/

namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// Agent 上下文重载触发器接口。
/// 工具修改工作区级配置（Skill/MCP）后调用，使新配置在回合结束后生效。
/// 实现应标记 pending 状态，由宿主在回合结束时检查并执行重建。
/// </summary>
public interface IAgentReloadTrigger
{
    /// <summary>
    /// 请求重载 Agent 上下文（重新加载工作区级 Skill/MCP，重建 Agent）。
    /// </summary>
    void RequestReload();
}