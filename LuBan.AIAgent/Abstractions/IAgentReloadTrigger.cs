/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Abstractions
*文件名： IAgentReloadTrigger
*描述：Agent 上下文重载触发器接口，用于延迟重建 Agent
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