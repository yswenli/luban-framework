/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Skills
*文件名： SkillRegistry
*版本号： V1.0.0.0
*唯一标识：8ce8e6ed-6316-4c28-a5b6-5f521b2d384b
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：Skill 注册表
*
*=================================================
*修改标记
*修改时间：2026/7/27
*修改人： yswenli
*版本号： V1.0.0.0
*描述：Skill 注册表
*
*****************************************************************************/

namespace LuBan.AIAgent.Skills;

/// <summary>
/// Skill 注册表，管理所有可用的 Skill（内置 + 自定义，惰性合并）
/// </summary>
/// <remarks>
/// 内置与自定义 Id 冲突时内置优先；冲突在 /skill add 时拦截（命令层）。
/// </remarks>
public class SkillRegistry
{
    private readonly Dictionary<string, ISkill> _builtinSkills = new();
    private readonly Configuration.ConfigManager? _configManager;

    /// <summary>
    /// 创建 SkillRegistry 实例
    /// </summary>
    /// <param name="skills">DI 注册的内置 Skill</param>
    /// <param name="configManager">配置管理器（可选，无则只有内置）</param>
    public SkillRegistry(IEnumerable<ISkill> skills, Configuration.ConfigManager? configManager = null)
    {
        foreach (var skill in skills)
        {
            _builtinSkills[skill.Id.ToLowerInvariant()] = skill;
        }
        _configManager = configManager;
    }

    private IEnumerable<ISkill> GetMerged()
    {
        var disabledBuiltin = _configManager?.DisabledBuiltinSkills;
        foreach (var (id, skill) in _builtinSkills)
        {
            if (disabledBuiltin?.Contains(id) == true)
                continue;
            yield return skill;
        }

        if (_configManager != null)
        {
            foreach (var cfg in _configManager.CustomSkills.Where(c => c.Enabled))
            {
                if (_builtinSkills.ContainsKey(cfg.Id.ToLowerInvariant()))
                    continue;
                yield return new CustomSkill(cfg);
            }
        }
    }

    /// <summary>
    /// 获取所有 Skill
    /// </summary>
    public IReadOnlyList<ISkill> GetAll() => GetMerged().ToList();

    /// <summary>
    /// 根据分类获取 Skill
    /// </summary>
    public IReadOnlyList<ISkill> GetByCategory(string category)
        => GetMerged().Where(s => s.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();

    /// <summary>
    /// 根据 ID 获取 Skill
    /// </summary>
    public ISkill? Get(string id)
    {
        id = id.ToLowerInvariant();
        if (_configManager?.DisabledBuiltinSkills.Contains(id) != true
            && _builtinSkills.TryGetValue(id, out var builtin))
        {
            return builtin;
        }

        var custom = _configManager?.CustomSkills
            .FirstOrDefault(c => c.Id == id && c.Enabled);
        return custom != null ? new CustomSkill(custom) : null;
    }

    /// <summary>
    /// 搜索 Skill
    /// </summary>
    public IReadOnlyList<ISkill> Search(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return GetAll();

        return GetMerged()
            .Where(s => s.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                       s.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// 获取所有分类
    /// </summary>
    public IReadOnlyList<string> GetCategories()
        => GetMerged().Select(s => s.Category).Distinct().ToList();
}
