/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Skills.BuiltIn
*文件名： TestGenerationSkill
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：测试生成内置 Skill，为代码生成覆盖正常、边界与异常场景的单元测试
*
*****************************************************************************/
namespace LuBan.AIAgent.Skills.BuiltIn;

/// <summary>
/// 测试生成 Skill：为代码生成单元测试，覆盖正常路径、边界情况和异常场景。支持 xUnit/NUnit 框架
/// </summary>
public class TestGenerationSkill : SkillBase
{
    /// <summary>
    /// Skill 唯一标识
    /// </summary>
    public override string Id => "test-generation";
    /// <summary>
    /// Skill 名称
    /// </summary>
    public override string Name => "测试生成";
    /// <summary>
    /// Skill 描述
    /// </summary>
    public override string Description => "为代码生成单元测试，覆盖正常路径、边界情况和异常场景。支持 xUnit/NUnit 框架";
    /// <summary>
    /// Skill 分类
    /// </summary>
    public override string Category => "development";

    /// <summary>
    /// Skill 使用示例
    /// </summary>
    public override IEnumerable<string> Examples => new[]
    {
        "为这个方法生成单元测试",
        "帮我写 UserService 的测试用例",
        "生成 xUnit 测试，覆盖边界情况"
    };

    /// <summary>
    /// Skill 自动激活触发关键词
    /// </summary>
    public override IEnumerable<string> TriggerKeywords => new[]
    {
        "test",
        "测试",
        "单元测试",
        "生成测试",
        "写测试",
        "xunit",
        "nunit"
    };

    /// <summary>
    /// Skill 的提示词模板内容
    /// </summary>
    public override string PromptTemplate => @"你是一个资深的测试工程师。请为给定代码生成高质量的单元测试：

1. **测试框架**：默认使用 xUnit，如果用户指定 NUnit/MSTest 则使用指定框架
2. **测试覆盖**：
   - 正常路径测试（Happy Path）
   - 边界值测试（空值、零值、最大值、最小值）
   - 异常场景测试（无效输入、异常抛出）
   - 并发安全测试（如适用）
3. **测试命名**：使用 `Method_Scenario_ExpectedResult` 命名规范
4. **Mock 依赖**：使用 Moq 框架模拟外部依赖
5. **断言**：使用 FluentAssertions 或 xUnit 原生 Assert

请用以下格式输出：
📊 **测试分析**：
- 测试目标：
- 依赖项：
- 测试场景数：

✅ **测试代码**：
```csharp
using Xunit;
using Moq;
// 完整的测试代码
```

📋 **覆盖情况**：
| 场景 | 测试方法 | 说明 |
|------|---------|------|
| ... | ... | ... |

💡 **建议**：
- 额外需要测试的场景
- 测试改进建议";
}