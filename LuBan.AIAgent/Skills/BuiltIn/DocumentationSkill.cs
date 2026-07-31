/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Skills.BuiltIn
*文件名： DocumentationSkill
*版本号： V1.0.0.0
*唯一标识：9779b56b-d2d9-44ee-b88f-4c25367bda77
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：文档生成 Skill
*
*=================================================
*修改标记
*修改时间：2026/7/27
*修改人： yswenli
*版本号： V1.0.0.0
*描述：文档生成 Skill
*
*****************************************************************************/

namespace LuBan.AIAgent.Skills.BuiltIn;

/// <summary>
/// 文档生成 Skill
/// </summary>
public class DocumentationSkill : SkillBase
{
    /// <summary>
    /// Skill ID
    /// </summary>
    public override string Id => "documentation";

    /// <summary>
    /// Skill 名称
    /// </summary>
    public override string Name => "文档生成";

    /// <summary>
    /// Skill 描述
    /// </summary>
    public override string Description => "为代码生成文档、注释、README、API 文档等";

    /// <summary>
    /// Skill 分类
    /// </summary>
    public override string Category => "productivity";

    /// <summary>
    /// 使用示例
    /// </summary>
    public override IEnumerable<string> Examples => new[]
    {
        "为这个类生成 XML 文档注释",
        "生成 README.md 文档",
        "为 API 接口生成文档"
    };

    /// <summary>
    /// 执行 Skill
    /// </summary>
    public override async Task<SkillResult> ExecuteAsync(SkillContext context, string input)
    {
        UpdateStatus(context, "正在生成文档...");

        var systemPrompt = @"你是一个技术文档专家。请根据代码或需求生成高质量的文档：

1. **清晰的说明**：功能描述、使用方法、参数说明
2. **代码示例**：提供可运行的代码示例
3. **注意事项**：重要的注意事项和限制
4. **格式规范**：使用 Markdown 格式，结构清晰

根据需求生成以下类型的文档：
- XML 文档注释（用于 C# 代码）
- README.md
- API 文档
- 使用指南";

        var result = await CallAgentAsync(context, $"{systemPrompt}\n\n{input}");

        return SkillResult.Ok(result ?? "");
    }
}
