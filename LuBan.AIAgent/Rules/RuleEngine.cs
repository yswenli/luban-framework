/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Rules
*文件名： RuleEngine
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：规则引擎，管理和执行规则
*
*****************************************************************************/
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

    /// <summary>
    /// 创建规则引擎
    /// </summary>
    /// <param name="rules">内置规则集合，注册到最高优先级的硬编码规则表</param>
    /// <param name="configReader">配置文件读取器，用于加载 config.json 中的自定义规则（可为 null）</param>
    public RuleEngine(IEnumerable<IRule> rules, Configuration.IAppConfigReader? configReader = null)
    {
        _configReader = configReader;
        foreach (var rule in rules)
            _hardcoded[rule.Id] = rule;
        LoadFromConfig();
    }

    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026", 
        Justification = "JSON 序列化仅用于简单配置类型，已通过 JsonSerializerOptions 处理")]
    /// <summary>
    /// 从工作区目录加载规则（读取 .luban-agent/rules 下的 JSON 规则文件）
    /// </summary>
    /// <param name="workspaceDir">工作区根目录</param>
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

    /// <summary>
    /// 从 config.json 加载自定义规则
    /// </summary>
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

    /// <summary>
    /// 重新加载所有规则（config.json + 可选的工作区规则）
    /// </summary>
    /// <param name="workspaceDir">工作区根目录；为 null 时仅重载 config.json</param>
    public void Reload(string? workspaceDir = null)
    {
        LoadFromConfig();
        if (workspaceDir != null)
            LoadFromWorkspace(workspaceDir);
    }

    /// <summary>
    /// 获取全部规则，按优先级降序排列
    /// </summary>
    /// <returns>规则列表</returns>
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

    /// <summary>
    /// 获取当前启用的规则（IsEnabled 为 true），按优先级降序排列
    /// </summary>
    /// <returns>启用的规则列表</returns>
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

    /// <summary>
    /// 按 ID 查找规则（忽略大小写）
    /// </summary>
    /// <param name="id">规则 ID</param>
    /// <returns>匹配的规则，未找到时返回 null</returns>
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

    /// <summary>
    /// 判断是否包含指定 ID 的规则（忽略大小写）
    /// </summary>
    /// <param name="id">规则 ID</param>
    /// <returns>存在返回 true，否则返回 false</returns>
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

    /// <summary>
    /// 对规则上下文执行评估，依次运行所有适用规则并汇总结果
    /// </summary>
    /// <param name="context">规则评估上下文</param>
    /// <returns>规则评估结果（含是否放行、注入文本、修改后的参数等）</returns>
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
    /// <summary>
    /// 是否允许继续执行（任一规则拒绝则为 false）
    /// </summary>
    public bool Allow { get; set; }

    /// <summary>
    /// 拒绝原因消息
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 规则修改后的参数（存在规则修改参数时非 null）
    /// </summary>
    public Dictionary<string, object?>? ModifiedArguments { get; set; }

    /// <summary>
    /// 各规则的执行结果列表
    /// </summary>
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
    /// <summary>
    /// 规则 ID
    /// </summary>
    public string RuleId { get; set; } = "";

    /// <summary>
    /// 规则名称
    /// </summary>
    public string RuleName { get; set; } = "";

    /// <summary>
    /// 规则执行结果
    /// </summary>
    public RuleResult Result { get; set; } = new();
}
