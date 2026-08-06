using LuBan.AIAgent.Configuration;

namespace LuBan.AIAgent.Skills;

/// <summary>
/// 自定义 Skill 适配器，将 CustomSkillConfig 包装为 Skill（纯提示词模板）
/// </summary>
public class CustomSkill : SkillBase
{
    private readonly CustomSkillConfig _config;

    public CustomSkill(CustomSkillConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public override string Id => _config.Id;
    public override string Name => _config.Name ?? "";
    public override string Description => _config.Description ?? "";
    public override string Category => _config.Category ?? "";
    public override IEnumerable<string> Examples => _config.Examples ?? Enumerable.Empty<string>();
    public override IEnumerable<string> TriggerKeywords => _config.TriggerKeywords ?? Enumerable.Empty<string>();
    public override string PromptTemplate => _config.PromptTemplate ?? "";
}