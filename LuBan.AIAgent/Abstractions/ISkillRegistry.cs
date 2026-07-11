namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 技能注册表接口，用于注册和管理技能
/// </summary>
public interface ISkillRegistry
{
    /// <summary>
    /// 注册技能
    /// </summary>
    /// <param name="skill">技能实例</param>
    void Register(ISkill skill);
    
    /// <summary>
    /// 注册多个技能
    /// </summary>
    /// <param name="skills">技能实例集合</param>
    void Register(IEnumerable<ISkill> skills);
    
    /// <summary>
    /// 尝试根据名称获取技能
    /// </summary>
    /// <param name="name">技能名称</param>
    /// <param name="skill">技能实例，如果不存在则返回null</param>
    /// <returns>是否找到技能</returns>
    bool TryGetSkill(string name, out ISkill? skill);
    
    /// <summary>
    /// 获取所有技能
    /// </summary>
    /// <returns>技能列表</returns>
    IReadOnlyList<ISkill> GetAllSkills();
}
