using LuBan.AIAgent.Abstractions;

namespace LuBan.AIAgent.Core;

/// <summary>
/// 内存技能注册表，用于在内存中注册和管理技能
/// </summary>
public class InMemorySkillRegistry : ISkillRegistry
{
    private readonly Dictionary<string, ISkill> _skills = new();

    /// <summary>
    /// 注册单个技能
    /// </summary>
    /// <param name="skill">技能</param>
    public void Register(ISkill skill)
    {
        _skills[skill.Name] = skill;
    }

    /// <summary>
    /// 注册多个技能
    /// </summary>
    /// <param name="skills">技能集合</param>
    public void Register(IEnumerable<ISkill> skills)
    {
        foreach (var skill in skills)
        {
            Register(skill);
        }
    }

    /// <summary>
    /// 尝试获取技能
    /// </summary>
    /// <param name="name">技能名称</param>
    /// <param name="skill">输出参数，获取到的技能</param>
    /// <returns>是否成功获取技能</returns>
    public bool TryGetSkill(string name, out ISkill? skill)
    {
        return _skills.TryGetValue(name, out skill);
    }

    /// <summary>
    /// 获取所有技能
    /// </summary>
    /// <returns>所有技能的只读列表</returns>
    public IReadOnlyList<ISkill> GetAllSkills()
    {
        return _skills.Values.ToList();
    }
}
