/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Skills
*文件名： SkillRegistry
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：Skill 注册表，管理所有可用的 Skill（硬编码 + 工作区 + config.json，三级优先级）
*
*****************************************************************************/
namespace LuBan.AIAgent.Skills;

/// <summary>
/// Skill 注册表，管理所有可用的 Skill（硬编码 + 工作区 + config.json，三级优先级）
/// </summary>
/// <remarks>
/// 优先级：硬编码（DI）> 工作区文件 > config.json。
/// 同名 Id 高优先级优先，低优先级被忽略。
/// </remarks>
public class SkillRegistry
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly Dictionary<string, ISkill> _hardcoded = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ISkill> _workspace = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ISkill> _config = new(StringComparer.OrdinalIgnoreCase);
    private readonly Configuration.IAppConfigReader? _configReader;
    private List<ISkill> _merged = new();

    /// <summary>
    /// 创建 SkillRegistry 实例，注册硬编码 Skill 并从 config.json 加载自定义 Skill
    /// </summary>
    /// <param name="skills">硬编码的内置 Skill 列表</param>
    /// <param name="configReader">应用配置读取器，可为 null</param>
    public SkillRegistry(IEnumerable<ISkill> skills, Configuration.IAppConfigReader? configReader = null)
    {
        _configReader = configReader;
        foreach (var skill in skills)
            _hardcoded[skill.Id] = skill;
        LoadFromConfig();
    }

    /// <summary>
    /// 从工作区目录加载 SKILL.md 文件，作为工作区级 Skill
    /// </summary>
    /// <param name="workspaceDir">工作区根目录，其下 .luban-agent/skills 目录会被扫描</param>
    public void LoadFromWorkspace(string workspaceDir)
    {
        var temp = new Dictionary<string, ISkill>(StringComparer.OrdinalIgnoreCase);
        var skillsDir = Path.Combine(workspaceDir, ".luban-agent", "skills");
        
        if (Directory.Exists(skillsDir))
        {
            try
            {
                var configs = SkillLoader.LoadAll(skillsDir);
                foreach (var config in configs)
                {
                    var skill = new FileSkill(config);
                    temp[skill.Id] = skill;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"加载工作区 Skill 失败: {skillsDir}", ex);
            }
        }

        _lock.EnterWriteLock();
        try
        {
            _workspace.Clear();
            foreach (var kvp in temp)
                _workspace[kvp.Key] = kvp.Value;
            RebuildMerged();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// 从 config.json 加载启用的自定义 Skill 配置
    /// </summary>
    public void LoadFromConfig()
    {
        var temp = new Dictionary<string, ISkill>(StringComparer.OrdinalIgnoreCase);
        
        try
        {
            if (_configReader != null)
            {
                foreach (var cfg in _configReader.CustomSkills.Where(c => c.Enabled))
                    temp[cfg.Id] = new CustomSkill(cfg);
            }
        }
        catch (Exception ex)
        {
            Logger.Error("加载 config.json Skills 失败", ex);
        }

        _lock.EnterWriteLock();
        try
        {
            _config.Clear();
            foreach (var kvp in temp)
                _config[kvp.Key] = kvp.Value;
            RebuildMerged();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// 重新加载 config.json 与工作区 Skill
    /// </summary>
    /// <param name="workspaceDir">工作区根目录，为 null 时仅重新加载 config.json</param>
    public void Reload(string? workspaceDir = null)
    {
        LoadFromConfig();
        if (workspaceDir != null)
            LoadFromWorkspace(workspaceDir);
    }

    /// <summary>
    /// 获取合并后的全部 Skill 列表
    /// </summary>
    /// <returns>按优先级合并后的 Skill 列表</returns>
    public IReadOnlyList<ISkill> GetAll()
    {
        _lock.EnterReadLock();
        try
        {
            return _merged.ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// 根据 Skill Id 获取 Skill
    /// </summary>
    /// <param name="id">Skill 唯一标识，忽略大小写</param>
    /// <returns>匹配的 Skill，未找到时返回 null</returns>
    public ISkill? Get(string id)
    {
        _lock.EnterReadLock();
        try
        {
            return _merged.FirstOrDefault(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// 判断是否存在指定 Id 的 Skill
    /// </summary>
    /// <param name="id">Skill 唯一标识，忽略大小写</param>
    /// <returns>存在返回 true，否则返回 false</returns>
    public bool Contains(string id)
    {
        _lock.EnterReadLock();
        try
        {
            return _merged.Any(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// 根据分类获取 Skill 列表
    /// </summary>
    /// <param name="category">Skill 分类，忽略大小写</param>
    /// <returns>指定分类下的 Skill 列表</returns>
    public IReadOnlyList<ISkill> GetByCategory(string category)
    {
        _lock.EnterReadLock();
        try
        {
            return _merged.Where(s => s.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// 根据关键词在 Skill 的名称和描述中搜索
    /// </summary>
    /// <param name="keyword">搜索关键词，为空时返回全部 Skill</param>
    /// <returns>名称或描述中包含关键词的 Skill 列表</returns>
    public IReadOnlyList<ISkill> Search(string keyword)
    {
        _lock.EnterReadLock();
        try
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return _merged.ToList();

            return _merged
                .Where(s => s.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                           s.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// 根据输入内容自动检测最匹配的 Skill（基于触发关键词、名称和描述打分）
    /// </summary>
    /// <param name="input">用户输入内容</param>
    /// <param name="maxResults">返回的最大结果数，默认 3</param>
    /// <returns>按匹配度排序的 Skill 列表，最多 maxResults 个</returns>
    public IReadOnlyList<ISkill> DetectSkills(string input, int maxResults = 3)
    {
        if (string.IsNullOrWhiteSpace(input) || maxResults <= 0)
            return Array.Empty<ISkill>();

        var allSkills = GetAll();
        var lowerInput = input.ToLowerInvariant();
        var matches = new List<(ISkill Skill, int Score)>();

        foreach (var skill in allSkills)
        {
            int score = 0;
            foreach (var keyword in skill.TriggerKeywords)
            {
                if (!string.IsNullOrWhiteSpace(keyword) && lowerInput.Contains(keyword.ToLowerInvariant()))
                    score += 10;
            }

            if (lowerInput.Contains(skill.Name.ToLowerInvariant()))
                score += 2;
            if (lowerInput.Contains(skill.Description.ToLowerInvariant()))
                score += 1;

            if (score > 0)
                matches.Add((skill, score));
        }

        return matches
            .OrderByDescending(m => m.Score)
            .ThenBy(m => m.Skill.Name)
            .Take(maxResults)
            .Select(m => m.Skill)
            .ToList();
    }

    private void RebuildMerged()
    {
        var merged = new Dictionary<string, ISkill>(StringComparer.OrdinalIgnoreCase);
        
        // 1. 最低优先级：config.json
        foreach (var kvp in _config)
            merged[kvp.Key] = kvp.Value;
        
        // 2. 中优先级：工作区文件
        foreach (var kvp in _workspace)
            merged[kvp.Key] = kvp.Value;
        
        // 3. 最高优先级：硬编码（排除被禁用的）
        var disabledBuiltin = _configReader?.DisabledBuiltinSkills;
        foreach (var kvp in _hardcoded)
        {
            if (disabledBuiltin?.Contains(kvp.Key) == true) continue;
            merged[kvp.Key] = kvp.Value;
        }

        _merged = merged.Values.ToList();
    }
}
