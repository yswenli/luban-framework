/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Skills.BuiltIn
*文件名： CodeRefactorSkill
*版本号： V1.0.0.0
*唯一标识：a1b2c3d4-e5f6-7890-abcd-ef1234567001
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/3
*描述：代码重构 Skill - 改善代码结构、消除重复、应用设计模式
*
*=================================================
*修改标记
*修改时间：2026/8/3
*修改人： yswenli
*版本号： V1.0.0.0
*描述：代码重构 Skill - 改善代码结构、消除重复、应用设计模式
*
*****************************************************************************/

namespace LuBan.AIAgent.Skills.BuiltIn;

/// <summary>
/// 代码重构 Skill - 改善代码结构、消除重复、应用设计模式
/// </summary>
public class CodeRefactorSkill : SkillBase
{
    /// <summary>
    /// Skill ID
    /// </summary>
    public override string Id => "code-refactor";

    /// <summary>
    /// Skill 名称
    /// </summary>
    public override string Name => "代码重构";

    /// <summary>
    /// Skill 描述
    /// </summary>
    public override string Description => "分析代码结构，提供重构方案：提取方法、消除重复、应用设计模式、简化复杂逻辑";

    /// <summary>
    /// Skill 分类
    /// </summary>
    public override string Category => "development";

    /// <summary>
    /// 使用示例
    /// </summary>
    public override IEnumerable<string> Examples => new[]
    {
        "重构这个方法，太长了",
        "帮我消除这段代码的重复逻辑",
        "这个类职责太多，怎么拆分",
        "用策略模式重构这段代码"
    };

    /// <summary>
    /// 自动激活触发关键词
    /// </summary>
    public override IEnumerable<string> TriggerKeywords => new[]
    {
        "refactor",
        "重构",
        "优化代码",
        "改结构",
        "消除重复",
        "拆分",
        "设计模式"
    };

    /// <summary>
    /// 执行 Skill
    /// </summary>
    public override async Task<SkillResult> ExecuteAsync(SkillContext context, string input)
    {
        UpdateStatus(context, "正在分析代码结构...");

        var systemPrompt = @"你是一个资深的软件架构师和重构专家。请对代码进行重构分析：

1. **坏味识别**：识别代码坏味（过长方法、过大类、重复代码、过长参数列表、发散修改、霰弹式修改等）
2. **重构方案**：针对每个问题提供具体的重构手法：
   - 提取方法（Extract Method）
   - 提取类（Extract Class）
   - 以策略模式消除条件逻辑
   - 以多态取代条件表达式
   - 移动方法/字段
   - 内联临时变量
   - 引入参数对象
3. **重构后代码**：给出重构后的完整代码
4. **改进说明**：解释每处改动的原因和收益

请用以下格式输出：
🔍 **代码坏味**：
- ...

📋 **重构方案**：
- ...

✨ **重构后代码**：
```csharp
// 重构后的代码
```

📝 **改动说明**：
- ...";

        var result = await CallAgentAsync(context, $"{systemPrompt}\n\n{input}");

        return SkillResult.Ok(result ?? "");
    }
}
