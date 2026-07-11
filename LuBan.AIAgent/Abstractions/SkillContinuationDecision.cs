namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 技能继续决策记录，用于决定是否继续执行技能
/// </summary>
public record SkillContinuationDecision
{
    /// <summary>
    /// 是否继续执行活跃技能
    /// </summary>
    public bool ContinueActiveSkill { get; init; }
    
    /// <summary>
    /// 技能名称
    /// </summary>
    public string? SkillName { get; init; }
    
    /// <summary>
    /// 决策原因
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// 创建继续执行技能的决策
    /// </summary>
    /// <param name="skillName">技能名称</param>
    /// <param name="reason">决策原因</param>
    /// <returns>技能继续决策</returns>
    public static SkillContinuationDecision Continue(string skillName, string? reason = null) => new()
    {
        ContinueActiveSkill = true,
        SkillName = skillName,
        Reason = reason
    };

    /// <summary>
    /// 创建不继续执行技能的决策
    /// </summary>
    /// <param name="reason">决策原因</param>
    /// <returns>技能继续决策</returns>
    public static SkillContinuationDecision NoContinuation(string? reason = null) => new()
    {
        ContinueActiveSkill = false,
        Reason = reason
    };
}
