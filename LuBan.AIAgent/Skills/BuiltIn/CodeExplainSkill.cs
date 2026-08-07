/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Skills.BuiltIn
*文件名： CodeExplainSkill
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：代码解释内置 Skill，解释代码逻辑、数据流与设计模式
*
*****************************************************************************/
namespace LuBan.AIAgent.Skills.BuiltIn;

/// <summary>
/// 代码解释 Skill：解释代码的逻辑、数据流、设计模式，帮助快速理解陌生代码或复杂实现
/// </summary>
public class CodeExplainSkill : SkillBase
{
    /// <summary>
    /// Skill 唯一标识
    /// </summary>
    public override string Id => "code-explain";
    /// <summary>
    /// Skill 名称
    /// </summary>
    public override string Name => "代码解释";
    /// <summary>
    /// Skill 描述
    /// </summary>
    public override string Description => "解释代码的逻辑、数据流、设计模式，帮助快速理解陌生代码或复杂实现";
    /// <summary>
    /// Skill 分类
    /// </summary>
    public override string Category => "development";

    /// <summary>
    /// Skill 使用示例
    /// </summary>
    public override IEnumerable<string> Examples => new[]
    {
        "解释一下这段代码在做什么",
        "这个方法的数据流是怎样的",
        "这里用了什么设计模式"
    };

    /// <summary>
    /// Skill 自动激活触发关键词
    /// </summary>
    public override IEnumerable<string> TriggerKeywords => new[]
    {
        "explain",
        "解释",
        "讲解",
        "什么意思",
        "这段代码",
        "怎么理解",
        "数据流"
    };

    /// <summary>
    /// Skill 的提示词模板内容
    /// </summary>
    public override string PromptTemplate => @"你是一个资深的技术讲师和代码解读专家。请用通俗易懂的方式解释代码：

1. **整体概述**：用一句话概括代码的功能和目的
2. **逐段解析**：按逻辑块拆分代码，解释每个块的作用
3. **数据流**：描述数据的输入、处理、输出流程
4. **设计模式**：识别代码中使用的设计模式（如有）
5. **关键点**：标注需要注意的细节（线程安全、性能考量、边界处理等）
6. **调用关系**：说明这段代码与外部的关系（被谁调用、调用了谁）

请用以下格式输出：
📌 **一句话概述**：
> ...

📖 **逐段解析**：

**第1段**（行 x-y）：
- 作用：...
- 说明：...

**第2段**（行 x-y）：
- 作用：...
- 说明：...

🔄 **数据流**：
输入 → 处理步骤 → 输出

🎨 **设计模式**：（如有）
- 模式名称：如何应用的

⚠️ **注意事项**：
- ...";
}