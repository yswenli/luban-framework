namespace LuBan.AIAgent.Rules;

/// <summary>
/// 规则引擎 - 管理和执行规则（硬编码 + 工作区 + config.json，三级优先级）
/// </summary>
public class RuleEngine
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly Dictionary<string, IRule> _hardcoded = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, IRule> _workspace = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, IRule> _config = new(StringComparer.OrdinalIgnoreCase);
    private readonly Configuration.IAppConfigReader? _configReader;
    private List<IRule> _merged = new();

    public RuleEngine(IEnumerable<IRule> rules, Configuration.IAppConfigReader? configReader = null)
    {
        _configReader = configReader;
        foreach (var rule in rules)
            _hardcoded[rule.Id] = rule;
        LoadFromConfig();
    }

    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026", 
        Justification = "JSON 序列化仅用于简单配置类型，已通过 JsonSerializerOptions 处理")]
    public void LoadFromWorkspace(string workspaceDir)
    {
        var temp = new Dictionary<string, IRule>(StringComparer.OrdinalIgnoreCase);
        var rulesDir = Path.Combine(workspaceDir, ".luban-agent", "rules");
        
        if (Directory.Exists(rulesDir))
        {
            foreach (var jsonFile in Directory.GetFiles(rulesDir, "*.json"))
            {
                try
                {
                    var json = File.ReadAllText(jsonFile);
                    var config = json.ToObject<Configuration.CustomRuleConfig>();
                    if (config != null && config.Enabled)
                    {
                        var rule = new CustomRule(config);
                        temp[config.Id] = rule;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"加载工作区 Rule 失败: {jsonFile}", ex);
                }
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
        var temp = new Dictionary<string, IRule>(StringComparer.OrdinalIgnoreCase);
        
        try
        {
            if (_configReader != null)
            {
                foreach (var cfg in _configReader.CustomRules)
                    temp[cfg.Id] = new CustomRule(cfg);
            }
        }
        catch (Exception ex)
        {
            Logger.Error("加载 config.json Rules 失败", ex);
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

    public IReadOnlyList<IRule> GetAllRules()
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

    public IReadOnlyList<IRule> GetEnabledRules()
    {
        _lock.EnterReadLock();
        try
        {
            return _merged.Where(r => r.IsEnabled).ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public IRule? GetRule(string id)
    {
        _lock.EnterReadLock();
        try
        {
            return _merged.FirstOrDefault(r => r.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
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
            return _merged.Any(r => r.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task<RuleEvaluationResult> EvaluateAsync(RuleContext context)
    {
        var applicableRules = GetEnabledRules().Where(r => r.IsApplicable(context)).ToList();
        
        var results = new List<RuleExecutionResult>();
        Dictionary<string, object?>? finalArguments = null;
        var evalInject = new List<string>();

        foreach (var rule in applicableRules)
        {
            var result = await rule.ExecuteAsync(context);
            results.Add(new RuleExecutionResult
            {
                RuleId = rule.Id,
                RuleName = rule.Name,
                Result = result
            });

            if (result.Inject.Count > 0)
                evalInject.AddRange(result.Inject);

            if (!result.Allow)
            {
                return new RuleEvaluationResult
                {
                    Allow = false,
                    Message = result.Message,
                    Results = results,
                    Inject = evalInject
                };
            }

            if (result.Modified && result.ModifiedArguments != null)
                finalArguments = result.ModifiedArguments;
        }

        return new RuleEvaluationResult
        {
            Allow = true,
            ModifiedArguments = finalArguments,
            Results = results,
            Inject = evalInject
        };
    }

    private void RebuildMerged()
    {
        var merged = new Dictionary<string, IRule>(StringComparer.OrdinalIgnoreCase);
        
        // 1. 最低优先级：config.json
        foreach (var kvp in _config)
            merged[kvp.Key] = kvp.Value;
        
        // 2. 中优先级：工作区文件
        foreach (var kvp in _workspace)
            merged[kvp.Key] = kvp.Value;
        
        // 3. 最高优先级：硬编码（排除被禁用的）
        var disabledBuiltin = _configReader?.DisabledBuiltinRules;
        foreach (var kvp in _hardcoded)
        {
            if (disabledBuiltin?.Contains(kvp.Key) == true) continue;
            merged[kvp.Key] = kvp.Value;
        }

        // 按 Priority 降序排序
        _merged = merged.Values.OrderByDescending(r => r.Priority).ToList();
    }
}

/// <summary>
/// 规则评估结果
/// </summary>
public class RuleEvaluationResult
{
    public bool Allow { get; set; }
    public string? Message { get; set; }
    public Dictionary<string, object?>? ModifiedArguments { get; set; }
    public List<RuleExecutionResult> Results { get; set; } = new();

    /// <summary>
    /// 所有规则注入的上下文文本（context-build 使用）
    /// </summary>
    public List<string> Inject { get; set; } = new();
}

/// <summary>
/// 单个规则执行结果
/// </summary>
public class RuleExecutionResult
{
    public string RuleId { get; set; } = "";
    public string RuleName { get; set; } = "";
    public RuleResult Result { get; set; } = new();
}
