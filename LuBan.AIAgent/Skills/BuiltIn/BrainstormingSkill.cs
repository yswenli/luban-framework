/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Skills.BuiltIn
*文件名： BrainstormingSkill
*版本号： V1.0.0.0
*唯一标识：82801bf2-910f-46b1-ab68-fb5200067bea
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：头脑风暴 Skill - 在实现功能前探索需求和设计
*
*=================================================
*修改标记
*修改时间：2026/7/27
*修改人： yswenli
*版本号： V1.0.0.0
*描述：头脑风暴 Skill - 在实现功能前探索需求和设计
*
*****************************************************************************/

namespace LuBan.AIAgent.Skills.BuiltIn;

/// <summary>
/// 头脑风暴 Skill - 在实现功能前探索需求和设计
/// </summary>
public class BrainstormingSkill : SkillBase
{
    /// <summary>
    /// Skill ID
    /// </summary>
    public override string Id => "brainstorming";

    /// <summary>
    /// Skill 名称
    /// </summary>
    public override string Name => "头脑风暴";

    /// <summary>
    /// Skill 描述
    /// </summary>
    public override string Description => "在实现任何功能前，先探索用户意图、需求和设计方案。适用于：创建新功能、构建组件、添加功能、修改行为";

    /// <summary>
    /// Skill 分类
    /// </summary>
    public override string Category => "creative";

    /// <summary>
    /// 使用示例
    /// </summary>
    public override IEnumerable<string> Examples => new[]
    {
        "我想实现一个用户登录功能",
        "帮我设计一个商品搜索页面",
        "需要添加一个数据导出功能"
    };

    /// <summary>
    /// 自动激活触发关键词
    /// </summary>
    public override IEnumerable<string> TriggerKeywords => new[]
    {
        "brainstorm",
        "头脑风暴",
        "设计",
        "方案",
        "如何实现",
        "帮我设计",
        "设计一下"
    };

    /// <summary>
    /// 执行 Skill
    /// </summary>
    public override async Task<SkillResult> ExecuteAsync(SkillContext context, string input)
    {
        UpdateStatus(context, "正在分析需求...");

        var systemPrompt = @"你是一个资深的产品经理和技术架构师。在实现任何功能前，请帮助用户：

1. **理解需求**：深入理解用户想要实现什么，背后的动机是什么
2. **探索方案**：提供多种可能的实现方案，分析各自的优缺点
3. **澄清问题**：提出关键问题，帮助用户明确需求细节
4. **提供建议**：基于最佳实践给出建议

请用结构化的方式输出：
- 📋 需求理解
- 💡 实现方案（至少2-3个）
- ❓ 需要澄清的问题
- ✅ 推荐方案";

        UpdateStatus(context, "正在生成分析和建议...");

        var result = await CallAgentAsync(context, $"{systemPrompt}\n\n用户需求：{input}");

        UpdateStatus(context, "分析完成");

        return SkillResult.Ok(result ?? "");
    }
}
