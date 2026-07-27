/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Rules
*文件名： RuleEngine
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：规则引擎
*
*****************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LuBan.AIAgent.Rules;

/// <summary>
/// 规则引擎 - 管理和执行规则
/// </summary>
public class RuleEngine
{
    private readonly List<IRule> _rules;

    /// <summary>
    /// 创建规则引擎实例
    /// </summary>
    /// <param name="rules">所有注册的规则</param>
    public RuleEngine(IEnumerable<IRule> rules)
    {
        _rules = rules.OrderByDescending(r => r.Priority).ToList();
    }

    /// <summary>
    /// 获取所有规则
    /// </summary>
    public IReadOnlyList<IRule> GetAllRules() => _rules;

    /// <summary>
    /// 获取启用的规则
    /// </summary>
    public IReadOnlyList<IRule> GetEnabledRules() => _rules.Where(r => r.IsEnabled).ToList();

    /// <summary>
    /// 根据规则 ID 获取规则
    /// </summary>
    public IRule? GetRule(string id) => _rules.FirstOrDefault(r => r.Id == id);

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