using LuBan.AIAgent.Abstractions;

namespace LuBan.AIAgent.Core;

/// <summary>
/// 基于规则的技能规划器，用于根据输入选择合适的技能
/// </summary>
public class RuleBasedSkillPlanner : ISkillPlanner
{
    /// <summary>
    /// 异步规划技能
    /// </summary>
    /// <param name="request">代理请求</param>
    /// <param name="skills">技能列表</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>技能规划</returns>
    public Task<SkillPlan> PlanAsync(AgentRequest request, IReadOnlyList<ISkill> skills, CancellationToken cancellationToken = default)
    {
        var input = request.Input ?? string.Empty;
        if (string.IsNullOrWhiteSpace(input) || skills.Count == 0)
        {
            return Task.FromResult(new SkillPlan());
        }

        var allowed = request.AllowedSkills is { Count: > 0 }
            ? new HashSet<string>(request.AllowedSkills, StringComparer.OrdinalIgnoreCase)
            : null;

        ISkill? bestSkill = null;
        var bestScore = 0;

        foreach (var skill in skills)
        {
            if (allowed != null && !allowed.Contains(skill.Name))
            {
                continue;
            }

            var score = ScoreSkill(input, skill);
            if (score > bestScore)
            {
                bestScore = score;
                bestSkill = skill;
            }
        }

        if (bestSkill == null || bestScore < 4)
        {
            return Task.FromResult(new SkillPlan());
        }

        return Task.FromResult(new SkillPlan
        {
            ShouldUseSkill = true,
            SkillName = bestSkill.Name,
            Reason = $"Matched skill '{bestSkill.Name}' with score {bestScore}."
        });
    }

    /// <summary>
    /// 为技能评分
    /// </summary>
    /// <param name="input">输入内容</param>
    /// <param name="skill">技能</param>
    /// <returns>技能评分</returns>
    private static int ScoreSkill(string input, ISkill skill)
    {
        var score = 0;
        var comparison = StringComparison.OrdinalIgnoreCase;

        if (input.Contains(skill.Name, comparison))
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
            if (!string.IsNullOrWhiteSpace(trigger) && input.Contains(trigger, comparison))
            {
                score += 4;
            }
        }

        if (triggers.Count(t => !string.IsNullOrWhiteSpace(t) && input.Contains(t, comparison)) > 1)
        {
            score += 2;
        }

        return score;
    }

    /// <summary>
    /// 计算关键词命中数
    /// </summary>
    /// <param name="input">输入内容</param>
    /// <param name="text">文本</param>
    /// <returns>关键词命中数</returns>
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
