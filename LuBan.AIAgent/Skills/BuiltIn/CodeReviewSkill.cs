/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Skills.BuiltIn
*文件名： CodeReviewSkill
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：代码审查内置 Skill，审查代码质量并给出改进建议
*
*****************************************************************************/
namespace LuBan.AIAgent.Skills.BuiltIn;

/// <summary>
/// 代码审查 Skill
/// </summary>
public class CodeReviewSkill : SkillBase
{
    /// <summary>
    /// Skill 唯一标识
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
    /// Skill 使用示例
    /// </summary>
    public override IEnumerable<string> Examples => new[]
    {
        "审查这个文件的代码：Program.cs",
        "检查我的代码有没有性能问题",
        "帮我 review 一下这段代码"
    };

    /// <summary>
    /// Skill 自动激活触发关键词
    /// </summary>
    public override IEnumerable<string> TriggerKeywords => new[]
    {
        "review",
        "审查",
        "代码审查",
        "code review",
        "检查一下代码",
        "帮我 review"
    };

    /// <summary>
    /// Skill 的提示词模板内容
    /// </summary>
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