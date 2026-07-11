namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 技能规划器接口，用于为代理请求规划技能执行
/// </summary>
public interface ISkillPlanner
{
    /// <summary>
    /// 异步为代理请求规划技能执行
    /// </summary>
    /// <param name="request">代理请求</param>
    /// <param name="skills">技能列表</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>技能执行计划</returns>
    Task<SkillPlan> PlanAsync(
        AgentRequest request,
        IReadOnlyList<ISkill> skills,
        CancellationToken cancellationToken = default);
}
