using LuBan.AIAgent.Abstractions;
using Microsoft.Extensions.Logging;

namespace LuBan.AIAgent.Core;

/// <summary>
/// 默认技能继续策略，用于决定是否继续使用当前激活的技能
/// </summary>
public class DefaultSkillContinuationPolicy : ISkillContinuationPolicy
{
    private static readonly string[] ExitPhrases =
    [
        "stop",
        "exit",
        "quit",
        "cancel",
        "never mind",
        "nevermind",
        "forget it",
        "back to normal chat",
        "just chat",
        "plain chat",
        "no skill"
    ];

    private readonly ILogger<DefaultSkillContinuationPolicy>? _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public DefaultSkillContinuationPolicy(ILogger<DefaultSkillContinuationPolicy>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// 异步决定是否继续使用当前激活的技能
    /// </summary>
    /// <param name="request">代理请求</param>
    /// <param name="state">对话状态</param>
    /// <param name="skills">技能列表</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>技能继续决策</returns>
    public Task<SkillContinuationDecision> DecideAsync(
        AgentRequest request,
        ConversationState? state,
        IReadOnlyList<ISkill> skills,
        CancellationToken cancellationToken = default)
    {
        if (state == null || string.IsNullOrWhiteSpace(state.ActiveSkill))
        {
            var decision = SkillContinuationDecision.NoContinuation("No active skill in session state.");
            _logger?.LogDebug("Continuation decision: {Reason}", decision.Reason);
            return Task.FromResult(decision);
        }

        if (!request.EnableSkills)
        {
            var decision = SkillContinuationDecision.NoContinuation("Skills disabled for request.");
            _logger?.LogDebug("Continuation decision: {Reason}", decision.Reason);
            return Task.FromResult(decision);
        }

        if (ShouldExitActiveSkill(request.Input))
        {
            var decision = SkillContinuationDecision.NoContinuation("User explicitly asked to exit the active skill.");
            _logger?.LogInformation("Continuation skipped. ActiveSkill={ActiveSkill}, Reason={Reason}", state.ActiveSkill, decision.Reason);
            return Task.FromResult(decision);
        }

        var activeSkill = skills.FirstOrDefault(s => string.Equals(s.Name, state.ActiveSkill, StringComparison.Ordinal));
        if (activeSkill != null && ShouldExitActiveSkillFromManifest(request.Input, activeSkill))
        {
            var decision = SkillContinuationDecision.NoContinuation("User matched a skill-specific exit rule.");
            _logger?.LogInformation("Continuation skipped. ActiveSkill={ActiveSkill}, Reason={Reason}", state.ActiveSkill, decision.Reason);
            return Task.FromResult(decision);
        }

        if (!string.IsNullOrWhiteSpace(request.PreferredSkill) &&
            !string.Equals(request.PreferredSkill, state.ActiveSkill, StringComparison.Ordinal))
        {
            var decision = SkillContinuationDecision.NoContinuation("Preferred skill overrides active skill.");
            _logger?.LogInformation(
                "Continuation skipped. PreferredSkill={PreferredSkill}, ActiveSkill={ActiveSkill}, Reason={Reason}",
                request.PreferredSkill,
                state.ActiveSkill,
                decision.Reason);
            return Task.FromResult(decision);
        }

        if (activeSkill == null)
        {
            var decision = SkillContinuationDecision.NoContinuation("Active skill is not registered.");
            _logger?.LogWarning("Continuation skipped. ActiveSkill={ActiveSkill}, Reason={Reason}", state.ActiveSkill, decision.Reason);
            return Task.FromResult(decision);
        }

        if (ShouldContinueActiveSkillFromManifest(request.Input, activeSkill))
        {
            var manifestContinueDecision = SkillContinuationDecision.Continue(state.ActiveSkill, "Continuing active skill from skill-specific continuation rule.");
            _logger?.LogInformation("Continuation enabled by manifest rule. ActiveSkill={ActiveSkill}", state.ActiveSkill);
            return Task.FromResult(manifestContinueDecision);
        }

        var competingSkill = FindStrongCompetingSkill(request, state, skills);
        if (competingSkill != null)
        {
            var decision = SkillContinuationDecision.NoContinuation($"Input appears to target skill '{competingSkill.Name}' more strongly than the active skill.");
            _logger?.LogInformation(
                "Continuation skipped. ActiveSkill={ActiveSkill}, CompetingSkill={CompetingSkill}, Reason={Reason}",
                state.ActiveSkill,
                competingSkill.Name,
                decision.Reason);
            return Task.FromResult(decision);
        }

        var continueDecision = SkillContinuationDecision.Continue(state.ActiveSkill, "Continuing active skill from session state.");
        _logger?.LogInformation("Continuation enabled. ActiveSkill={ActiveSkill}", state.ActiveSkill);
        return Task.FromResult(continueDecision);
    }

    /// <summary>
    /// 检查输入是否包含退出活动技能的短语
    /// </summary>
    /// <param name="input">输入文本</param>
    /// <returns>是否应该退出活动技能</returns>
    private static bool ShouldExitActiveSkill(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        return ExitPhrases.Any(phrase => input.Contains(phrase, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 从技能清单中检查是否应该退出活动技能
    /// </summary>
    /// <param name="input">输入文本</param>
    /// <param name="skill">技能</param>
    /// <returns>是否应该退出活动技能</returns>
    private static bool ShouldExitActiveSkillFromManifest(string? input, ISkill skill)
        => ContainsAnyConfiguredPhrase(input, skill, "exitOn");

    /// <summary>
    /// 从技能清单中检查是否应该继续活动技能
    /// </summary>
    /// <param name="input">输入文本</param>
    /// <param name="skill">技能</param>
    /// <returns>是否应该继续活动技能</returns>
    private static bool ShouldContinueActiveSkillFromManifest(string? input, ISkill skill)
        => ContainsAnyConfiguredPhrase(input, skill, "continueOn");

    /// <summary>
    /// 检查输入是否包含配置的短语
    /// </summary>
    /// <param name="input">输入文本</param>
    /// <param name="skill">技能</param>
    /// <param name="metadataKey">元数据键</param>
    /// <returns>是否包含配置的短语</returns>
    private static bool ContainsAnyConfiguredPhrase(string? input, ISkill skill, string metadataKey)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        if (skill.Manifest?.Metadata == null || !skill.Manifest.Metadata.TryGetValue(metadataKey, out var rawValue) || string.IsNullOrWhiteSpace(rawValue))
        {
            return false;
        }

        var phrases = rawValue.Split([',', ';', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return phrases.Any(phrase => input.Contains(phrase, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 查找强劲的竞争技能
    /// </summary>
    /// <param name="request">代理请求</param>
    /// <param name="state">对话状态</param>
    /// <param name="skills">技能列表</param>
    /// <returns>强劲的竞争技能，若没有则返回null</returns>
    private static ISkill? FindStrongCompetingSkill(AgentRequest request, ConversationState state, IReadOnlyList<ISkill> skills)
    {
        var input = request.Input ?? string.Empty;
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var activeSkill = skills.FirstOrDefault(s => string.Equals(s.Name, state.ActiveSkill, StringComparison.Ordinal));
        if (activeSkill == null)
        {
            return null;
        }

        var activeScore = ScoreSkill(input, activeSkill);
        ISkill? bestCompetingSkill = null;
        var bestCompetingScore = 0;

        foreach (var skill in skills)
        {
            if (string.Equals(skill.Name, state.ActiveSkill, StringComparison.Ordinal))
            {
                continue;
            }

            var score = ScoreSkill(input, skill);
            if (score > bestCompetingScore)
            {
                bestCompetingScore = score;
                bestCompetingSkill = skill;
            }
        }

        if (bestCompetingSkill == null)
        {
            return null;
        }

        return bestCompetingScore >= 4 && bestCompetingScore >= activeScore + 2
            ? bestCompetingSkill
            : null;
    }

    /// <summary>
    /// 为技能评分
    /// </summary>
    /// <param name="input">输入文本</param>
    /// <param name="skill">技能</param>
    /// <returns>技能评分</returns>
    private static int ScoreSkill(string input, ISkill skill)
    {
        var score = 0;

        if (input.Contains(skill.Name, StringComparison.OrdinalIgnoreCase))
        {
            score += 5;
        }

        if (!string.IsNullOrWhiteSpace(skill.Description))
        {
            score += CountKeywordHits(input, skill.Description!) * 2;
        }

        var triggers = skill.Manifest?.Triggers ?? [];
        foreach (var trigger in triggers)
        {
            if (!string.IsNullOrWhiteSpace(trigger) && input.Contains(trigger, StringComparison.OrdinalIgnoreCase))
            {
                score += 4;
            }
        }

        if (triggers.Count(t => !string.IsNullOrWhiteSpace(t) && input.Contains(t, StringComparison.OrdinalIgnoreCase)) > 1)
        {
            score += 2;
        }

        return score;
    }

    /// <summary>
    /// 计算关键词命中次数
    /// </summary>
    /// <param name="input">输入文本</param>
    /// <param name="text">待检查文本</param>
    /// <returns>关键词命中次数</returns>
    private static int CountKeywordHits(string input, string text)
    {
        var count = 0;
        foreach (var token in text.Split(new[] { ' ', ',', '.', ';', ':', '-', '_', '/', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (token.Length >= 4 && input.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                count++;
            }
        }

        return count;
    }
}
