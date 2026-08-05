namespace LuBan.AIAgent.Skills.BuiltIn;

/// <summary>
/// 代码审查 Skill
/// </summary>
public class CodeReviewSkill : SkillBase
{
    public override string Id => "code-review";
    public override string Name => "代码审查";
    public override string Description => "审查代码，发现潜在问题、改进代码质量、提供最佳实践建议";
    public override string Category => "development";

    public override IEnumerable<string> Examples => new[]
    {
        "审查这个文件的代码：Program.cs",
        "检查我的代码有没有性能问题",
        "帮我 review 一下这段代码"
    };

    public override IEnumerable<string> TriggerKeywords => new[]
    {
        "review",
        "审查",
        "代码审查",
        "code review",
        "检查一下代码",
        "帮我 review"
    };

    public override string PromptTemplate => @"你是一个资深的代码审查专家。请对代码进行全面的审查：

1. **代码质量**：可读性、可维护性、命名规范
2. **潜在问题**：Bug、安全漏洞、性能问题
3. **最佳实践**：是否遵循最佳实践和设计模式
4. **改进建议**：具体的改进方案

请用以下格式输出：
✅ **优点**：
- ...

⚠️ **问题**：
- ...

💡 **改进建议**：
- ...";
}