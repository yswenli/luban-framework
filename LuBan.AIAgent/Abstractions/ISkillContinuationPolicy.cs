namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 技能继续策略接口，用于决定是否继续执行技能
/// </summary>
public interface ISkillContinuationPolicy
{
    /// <summary>
    /// 异步决定是否继续执行技能
    /// </summary>
    /// <param name="request">代理请求</param>
    /// <param name="state">对话状态</param>
    /// <param name="skills">技能列表</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>技能继续决策</returns>
    Task<SkillContinuationDecision> DecideAsync(
        AgentRequest request,
        ConversationState? state,
        IReadOnlyList<ISkill> skills,
        CancellationToken cancellationToken = default);
}
