namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 技能执行计划记录，用于决定是否使用技能以及使用哪个技能
/// </summary>
public record SkillPlan
{
    /// <summary>
    /// 是否应该使用技能
    /// </summary>
    public bool ShouldUseSkill { get; init; }
    
    /// <summary>
    /// 技能名称
    /// </summary>
    public string? SkillName { get; init; }
    
    /// <summary>
    /// 计划原因
    /// </summary>
    public string? Reason { get; init; }
}
