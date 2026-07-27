/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Skills.BuiltIn
*文件名： CodeReviewSkill
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：代码审查 Skill
*
*****************************************************************************/

namespace LuBan.AIAgent.Skills.BuiltIn;

/// <summary>
/// 代码审查 Skill
/// </summary>
public class CodeReviewSkill : SkillBase
{
    /// <summary>
    /// Skill ID
    /// </summary>
    public override string Id => "code-review";

    /// <summary>
    /// Skill 名称
    /// </summary>
    public override string Name => "代码审查";

    /// <summary>
    /// Skill 描述
    /// </summary>
    public override string Description => "审查代码，发现潜在问题、改进代码质量、提供最佳实践建议";

    /// <summary>
    /// Skill 分类
    /// </summary>
    public override string Category => "development";

    /// <summary>
    /// 使用示例
    /// </summary>
    public override IEnumerable<string> Examples => new[]
    {
        "审查这个文件的代码：Program.cs",
        "检查我的代码有没有性能问题",
        "帮我 review 一下这段代码"
    };

    /// <summary>
    /// 执行 Skill
    /// </summary>
    public override async Task<SkillResult> ExecuteAsync(SkillContext context, string input)
    {
        UpdateStatus(context, "正在审查代码...");

        var systemPrompt = @"你是一个资深的代码审查专家。请对代码进行全面的审查：

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

        var result = await CallAgentAsync(context, $"{systemPrompt}\n\n{input}");

        return SkillResult.Ok(result ?? "");
    }
}
