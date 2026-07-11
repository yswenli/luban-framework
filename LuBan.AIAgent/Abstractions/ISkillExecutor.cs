namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 技能执行器接口，用于执行技能清单
/// </summary>
public interface ISkillExecutor
{
    /// <summary>
    /// 异步执行技能清单
    /// </summary>
    /// <param name="manifest">技能清单</param>
    /// <param name="context">技能执行上下文</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>代理执行结果</returns>
    Task<AgentResult> ExecuteAsync(
        SkillManifest manifest,
        SkillExecutionContext context,
        CancellationToken cancellationToken = default);
}
