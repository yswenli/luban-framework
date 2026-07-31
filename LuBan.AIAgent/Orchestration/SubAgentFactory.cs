/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Orchestration
*文件名： SubAgentFactory
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：SubAgent 工厂，封装 LuBanAgentFactory 的子 Agent 创建逻辑
*
*****************************************************************************/
using LuBan.AIAgent.Orchestration.Models;

namespace LuBan.AIAgent.Orchestration;

/// <summary>
/// SubAgent 工厂，封装 <see cref="LuBanAgentFactory"/> 的子 Agent 创建逻辑。
/// 不依赖 ISessionManager：LuBanAgent 内部已有 AgentSession 管理，
/// ISessionManager 仅用于 SessionChatHistoryProvider 的持久化，
/// SubAgent 不启用 SessionHistory，因此无需 ISessionManager。
/// </summary>
public class SubAgentFactory
{
    private readonly LuBanAgentFactory _innerFactory;

    /// <summary>
    /// 创建 SubAgentFactory 实例。
    /// </summary>
    /// <param name="innerFactory">内部 LuBanAgent 工厂。</param>
    public SubAgentFactory(LuBanAgentFactory innerFactory)
    {
        _innerFactory = innerFactory;
    }

    /// <summary>
    /// 根据 SubAgent 规格创建子 Agent。
    /// </summary>
    /// <param name="spec">SubAgent 创建规格。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>LuBanAgent 实例。</returns>
    public async Task<LuBanAgent> CreateAsync(SubAgentSpec spec, CancellationToken ct = default)
    {
        var agent = await _innerFactory.CreateSubAgentAsync(
            modelName: spec.ModelName,
            toolGroups: spec.ToolGroups,
            systemPrompt: BuildSubAgentSystemPrompt(spec),
            cancellationToken: ct);

        spec.SessionId = agent.Id;
        return agent;
    }

    /// <summary>
    /// 构建 SubAgent 系统提示词。
    /// </summary>
    /// <param name="spec">SubAgent 规格。</param>
    /// <returns>系统提示词字符串。</returns>
    private static string BuildSubAgentSystemPrompt(SubAgentSpec spec)
        => $"你是任务图谱中的子执行单元，负责完成「{spec.NodeId}」节点的任务。" +
           "请专注于当前任务，使用可用工具完成任务后给出简洁结果。";
}
