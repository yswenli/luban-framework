/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Rules
*文件名： RuleEngine
*版本号： V1.0.0.0
*唯一标识：e3aec384-cae4-4a25-95c0-af92a9b3a466
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：规则引擎
*
*=================================================
*修改标记
*修改时间：2026/7/27
*修改人： yswenli
*版本号： V1.0.0.0
*描述：规则引擎
*
*****************************************************************************/

namespace LuBan.AIAgent.Rules;

/// <summary>
/// 规则引擎 - 管理和执行规则（内置 + 自定义，惰性合并）
/// </summary>
/// <remarks>
/// 内置规则构造时缓存，自定义规则每次读取时从 ConfigManager 实时合并；
/// 内置与自定义 Id 冲突时内置优先（冲突项在合并时被跳过）。
/// </remarks>
public class RuleEngine
{
    private readonly List<IRule> _builtinRules;
    private readonly HashSet<string> _builtinIds;
    private readonly Configuration.ConfigManager? _configManager;

    /// <summary>
    /// 创建规则引擎实例
    /// </summary>
    /// <param name="rules">DI 注册的内置规则</param>
    /// <param name="configManager">配置管理器（可选，无则只有内置）</param>
    public RuleEngine(IEnumerable<IRule> rules, Configuration.ConfigManager? configManager = null)
    {
        _builtinRules = rules.ToList();
        _builtinIds = _builtinRules.Select(r => r.Id.ToLowerInvariant()).ToHashSet();
        _configManager = configManager;
    }

    private List<IRule> GetMerged()
    {
        var disabledBuiltin = _configManager?.DisabledBuiltinRules;
        var merged = _builtinRules
            .Where(r => disabledBuiltin?.Contains(r.Id.ToLowerInvariant()) != true);

        if (_configManager != null)
        {
            merged = merged.Concat(
                _configManager.CustomRules
                    .Where(c => !_builtinIds.Contains(c.Id.ToLowerInvariant()))
                    .Select(c => new CustomRule(c)));
        }

        return merged.OrderByDescending(r => r.Priority).ToList();
    }

    /// <summary>
    /// 获取所有规则
    /// </summary>
    public IReadOnlyList<IRule> GetAllRules() => GetMerged();

    /// <summary>
    /// 获取启用的规则
    /// </summary>
    public IReadOnlyList<IRule> GetEnabledRules() => GetMerged().Where(r => r.IsEnabled).ToList();

    /// <summary>
    /// 根据规则 ID 获取规则
    /// </summary>
    public IRule? GetRule(string id) => GetMerged().FirstOrDefault(r => r.Id == id.ToLowerInvariant());

    /// <summary>
    /// 评估规则
    /// </summary>
    /// <param name="context">规则上下文</param>
    /// <returns>评估结果</returns>
    public async Task<RuleEvaluationResult> EvaluateAsync(RuleContext context)
    {
        var applicableRules = GetEnabledRules().Where(r => r.IsApplicable(context)).ToList();
        
        var results = new List<RuleExecutionResult>();
        bool? finalAllow = null;
        Dictionary<string, object?>? finalArguments = null;

        foreach (var rule in applicableRules)
        {
            var result = await rule.ExecuteAsync(context);
            results.Add(new RuleExecutionResult
            {
                RuleId = rule.Id,
                RuleName = rule.Name,
                Result = result
            });

            // 如果规则拒绝，立即返回
            if (!result.Allow)
            {
                return new RuleEvaluationResult
                {
                    Allow = false,
                    Message = result.Message,
                    Results = results
                };
            }

            // 如果规则修改了参数
            if (result.Modified && result.ModifiedArguments != null)
            {
                finalArguments = result.ModifiedArguments;
            }

            // 记录是否允许
            if (finalAllow == null)
                finalAllow = result.Allow;
        }

        // 没有适用规则时默认允许
        return new RuleEvaluationResult
        {
            Allow = finalAllow ?? true,
            ModifiedArguments = finalArguments,
            Results = results
        };
    }
}

/// <summary>
/// 规则评估结果
/// </summary>
public class RuleEvaluationResult
{
    /// <summary>
    /// 是否允许执行
    /// </summary>
    public bool Allow { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 修改后的参数
    /// </summary>
    public Dictionary<string, object?>? ModifiedArguments { get; set; }

    /// <summary>
    /// 各规则执行结果
    /// </summary>
    public List<RuleExecutionResult> Results { get; set; } = new();
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
    /// 执行结果
    /// </summary>
    public RuleResult Result { get; set; } = new();
}
