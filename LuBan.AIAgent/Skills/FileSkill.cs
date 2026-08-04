namespace LuBan.AIAgent.Skills;

/// <summary>
/// 文件级 Skill 适配器，将 SKILL.md 文件包装为 ISkill
/// </summary>
public class FileSkill : ISkill
{
    private readonly FileSkillConfig _config;

    public FileSkill(FileSkillConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public string Id => _config.Id;
    public string Name => _config.Name;
    public string Description => _config.Description;
    public string Category => _config.Category;
    public IEnumerable<string> Examples => _config.Examples;
    public string? PromptTemplate => _config.PromptTemplate;
    public string SourcePath => _config.SourcePath;

    public Task<SkillResult> ExecuteAsync(SkillContext context, string input)
    {
        if (context.Agent == null)
            return Task.FromResult(SkillResult.Fail("Agent 不可用"));

        var prompt = _config.PromptTemplate.Contains("{input}")
            ? _config.PromptTemplate.Replace("{input}", input)
            : $"{_config.PromptTemplate}\n\n{input}";

        return Task.FromResult(SkillResult.Ok(prompt));
    }
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
    public string SourcePath { get; set; } = "";
}
