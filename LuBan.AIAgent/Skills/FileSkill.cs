namespace LuBan.AIAgent.Skills;

/// <summary>
/// 文件级 Skill 适配器，将 SKILL.md 文件包装为 Skill（纯提示词模板）
/// </summary>
public class FileSkill : SkillBase
{
    private readonly FileSkillConfig _config;

    public FileSkill(FileSkillConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public override string Id => _config.Id;
    public override string Name => _config.Name;
    public override string Description => _config.Description;
    public override string Category => _config.Category;
    public override IEnumerable<string> Examples => _config.Examples;
    public override IEnumerable<string> TriggerKeywords => _config.TriggerKeywords;
    public override string PromptTemplate => _config.PromptTemplate;
    public string SourcePath => _config.SourcePath;
}

/// <summary>
/// 文件级 Skill 配置数据
/// </summary>
public class FileSkillConfig
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "custom";
    public string PromptTemplate { get; set; } = "";
    public List<string> Examples { get; set; } = new();
    public List<string> TriggerKeywords { get; set; } = new();
    public string SourcePath { get; set; } = "";
}