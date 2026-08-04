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
/// Skill 注册表，管理所有可用的 Skill（内置 + 文件级 + 自定义，惰性合并）
/// </summary>
/// <remarks>
/// 优先级：文件级（项目/用户）> 内置 > 自定义（config.json）。
/// 同名 Id 高优先级覆盖低优先级。
/// </remarks>
public class SkillRegistry
{
    private readonly Dictionary<string, ISkill> _builtinSkills = new();
    private readonly Configuration.ConfigManager? _configManager;
    private List<FileSkill> _fileSkills = new();

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

    /// <summary>
    /// 加载工作区级和用户级的文件 Skill。每次切换工作区时调用。
    /// </summary>
    /// <param name="workspaceSkillsDir">工作区级 skills 目录，可为 null</param>
    public void LoadFileSkills(string? workspaceSkillsDir)
    {
        var configs = SkillLoader.LoadAll(workspaceSkillsDir);
        _fileSkills = configs.Select(c => new FileSkill(c)).ToList();
    }

    private IEnumerable<ISkill> GetMerged()
    {
        var consumedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 1. 文件级 Skill（最高优先级）
        foreach (var fs in _fileSkills)
        {
            consumedIds.Add(fs.Id.ToLowerInvariant());
            yield return fs;
        }

        // 2. 内置 Skill（过滤已禁用和被文件级覆盖的）
        var disabledBuiltin = _configManager?.DisabledBuiltinSkills;
        foreach (var (id, skill) in _builtinSkills)
        {
            if (consumedIds.Contains(id)) continue;
            if (disabledBuiltin?.Contains(id) == true) continue;
            consumedIds.Add(id);
            yield return skill;
        }

        // 3. 自定义 Skill（config.json，过滤已被覆盖的）
        if (_configManager != null)
        {
            foreach (var cfg in _configManager.CustomSkills.Where(c => c.Enabled))
            {
                if (consumedIds.Contains(cfg.Id.ToLowerInvariant())) continue;
                consumedIds.Add(cfg.Id.ToLowerInvariant());
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
        var lowerId = id.ToLowerInvariant();

        // 1. 文件级优先
        var fileSkill = _fileSkills.FirstOrDefault(f => f.Id.Equals(lowerId, StringComparison.OrdinalIgnoreCase));
        if (fileSkill != null) return fileSkill;

        // 2. 内置
        if (_configManager?.DisabledBuiltinSkills.Contains(lowerId) != true
            && _builtinSkills.TryGetValue(lowerId, out var builtin))
        {
            return builtin;
        }

        // 3. 自定义（config.json）
        var custom = _configManager?.CustomSkills
            .FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase) && c.Enabled);
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
