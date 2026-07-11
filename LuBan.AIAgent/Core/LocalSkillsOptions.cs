namespace LuBan.AIAgent.Core;

/// <summary>
/// 本地技能选项
/// </summary>
public class LocalSkillsOptions
{
    /// <summary>
    /// 技能根目录，默认为"skills"
    /// </summary>
    public string RootDirectory { get; set; } = "skills";

    /// <summary>
    /// 当遇到重复技能时是否抛出异常，默认为true
    /// </summary>
    public bool ThrowOnDuplicateSkill { get; set; } = true;

    /// <summary>
    /// 是否忽略无效技能，默认为false
    /// </summary>
    public bool IgnoreInvalidSkills { get; set; } = false;
}
