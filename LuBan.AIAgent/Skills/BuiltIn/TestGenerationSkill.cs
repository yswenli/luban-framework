/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
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
/// 测试生成 Skill：按测试驱动开发（先失败测试后实现）为代码补测试，覆盖正常路径、边界情况与异常场景。默认沿用仓库既有测试栈（本项目为 MSTest）
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
    public override string Description => "按测试驱动开发为代码补测试，覆盖正常路径、边界情况和异常场景。沿用仓库既有测试栈（本项目为 MSTest）";
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
    public override string PromptTemplate => @"你是一个资深测试工程师。默认按测试驱动开发（TDD）工作。

## 铁律
没有先看到测试失败，就没有资格写实现代码。若先写了实现，删掉重来；不要留作「参考」，不要边写测试边改它。

## 第 0 步：确认测试栈
动手前先看仓库现有测试工程与 csproj：测试框架（本项目用 MSTest）、断言方式、是否有 Mock 库。沿用仓库既有约定；仓库里没有的依赖不要自行引入。禁止把别的仓库习惯（如 xUnit / Moq / FluentAssertions）当成本仓库默认。

## 循环：红 → 绿 → 重构
1. **红**：写一个最小测试并断言目标行为，运行它，确认因预期原因失败（而不是被编译错误掩盖的失败）。
2. **绿**：写刚好能通过的最小实现，不提前设计。
3. **重构**：在测试全绿的前提下清理重复与命名，每步重跑测试。

## 覆盖维度
- 正常路径
- 边界值（空、零、极值、超长）
- 异常与非法输入
- 状态与并发（适用时）
- 回归用例：每个修过的缺陷补一条能复现原问题的测试

## 写法约定
- 命名说清「方法_场景_预期」
- 一个测试只验证一个行为
- 依赖外部服务或时钟的用例要么能离线跑，要么明确标记为集成测试
- 断言失败信息要能直接看出哪个期望值不符

## 输出格式
- 🧪 测试计划（目标、依赖、场景清单）
- 🔴 先给出的失败测试及其实际失败输出
- 🟢 最小实现
- 📋 覆盖对照表（场景 / 测试方法 / 说明）
- ⚠️ 未覆盖项及原因

## 边界
- 不用通过率或用例数交换质量：不删除、不跳过、不弱化断言来凑绿。
- 只改与本需求相关的代码与测试。";
}