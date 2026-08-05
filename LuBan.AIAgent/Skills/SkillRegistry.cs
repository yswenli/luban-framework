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
    private readonly Configuration.ConfigManager? _configManager;
    private List<ISkill> _merged = new();

    public SkillRegistry(IEnumerable<ISkill> skills, Configuration.ConfigManager? configManager = null)
    {
        _configManager = configManager;
        foreach (var skill in skills)
            _hardcoded[skill.Id] = skill;
        LoadFromConfig();
    }

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

    public void LoadFromConfig()
    {
        var temp = new Dictionary<string, ISkill>(StringComparer.OrdinalIgnoreCase);
        
        try
        {
            if (_configManager != null)
            {
                foreach (var cfg in _configManager.CustomSkills.Where(c => c.Enabled))
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

    public void Reload(string? workspaceDir = null)
    {
        LoadFromConfig();
        if (workspaceDir != null)
            LoadFromWorkspace(workspaceDir);
    }

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
        var disabledBuiltin = _configManager?.DisabledBuiltinSkills;
        foreach (var kvp in _hardcoded)
        {
            if (disabledBuiltin?.Contains(kvp.Key) == true) continue;
            merged[kvp.Key] = kvp.Value;
        }

        _merged = merged.Values.ToList();
    }
}
