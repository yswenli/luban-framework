/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Rules
*文件名： CustomRule
*版本号： V1.0.0.0
*唯一标识：7d6457fc-bca9-4e94-a80d-f063042c29d5
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/28
*描述：自定义规则适配器，将 CustomRuleConfig 包装为 IRule
*
*=================================================
*修改标记
*修改时间：2026/7/28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：自定义规则适配器，将 CustomRuleConfig 包装为 IRule
*
*****************************************************************************/

using LuBan.AIAgent.Configuration;

namespace LuBan.AIAgent.Rules;

/// <summary>
/// 自定义规则适配器，将 CustomRuleConfig 包装为 IRule
/// </summary>
public class CustomRule : IRule
{
    private readonly CustomRuleConfig _config;

    /// <summary>
    /// 创建自定义规则实例
    /// </summary>
    /// <param name="config">自定义规则配置</param>
    public CustomRule(CustomRuleConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// 规则 ID
    /// </summary>
    public string Id => _config.Id;

    /// <summary>
    /// 规则名称
    /// </summary>
    public string Name => _config.Name;

    /// <summary>
    /// 规则描述
    /// </summary>
    public string Description => _config.Description;

    /// <summary>
    /// 规则优先级
    /// </summary>
    public int Priority => _config.Priority;

    /// <summary>
    /// 规则是否启用
    /// </summary>
    public bool IsEnabled
    {
        get => _config.Enabled;
        set => _config.Enabled = value;
    }

    /// <summary>
    /// 检查规则是否适用（ActionType 与 Target 均需匹配）
    /// </summary>
    /// <param name="context">规则上下文</param>
    /// <returns>是否适用</returns>
    public bool IsApplicable(RuleContext context)
    {
        return WildcardMatch(_config.ActionTypePattern, context.ActionType)
            && WildcardMatch(_config.TargetPattern, context.Target);
    }

    /// <summary>
    /// 执行规则，根据配置动作返回允许或拒绝
    /// </summary>
    /// <param name="context">规则上下文</param>
    /// <returns>规则执行结果</returns>
    public Task<RuleResult> ExecuteAsync(RuleContext context)
    {
        if (string.Equals(_config.Action, "allow", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(RuleResult.AllowResult($"规则 '{Name}' 允许"));

        return Task.FromResult(RuleResult.DenyResult(
            $"操作被规则 '{Name}' 拒绝（目标: {context.Target}）"));
    }

    internal static bool WildcardMatch(string pattern, string value)
    {
        if (string.IsNullOrEmpty(pattern) || pattern == "*")
            return true;
        if (string.IsNullOrEmpty(value))
            return false;

        pattern = pattern.ToLowerInvariant();
        value = value.ToLowerInvariant();

        var parts = pattern.Split('*');
        if (parts.Length == 1)
            return value == pattern;

        var pos = 0;
        for (var i = 0; i < parts.Length; i++)
        {
            var part = parts[i];
            if (part.Length == 0) continue;

            var idx = value.IndexOf(part, pos, StringComparison.Ordinal);
            if (idx < 0) return false;
            if (i == 0 && idx != 0) return false;
            pos = idx + part.Length;
        }

        var lastPart = parts[^1];
        if (lastPart.Length > 0 && !value.EndsWith(lastPart, StringComparison.Ordinal))
            return false;

        return true;
    }
}
