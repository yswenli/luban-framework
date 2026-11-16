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
*文件名： DebugAssistantSkill
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：调试助手内置 Skill，诊断 bug 根因并提供修复方案
*
*****************************************************************************/
namespace LuBan.AIAgent.Skills.BuiltIn;

/// <summary>
/// 调试助手 Skill：分析错误信息和堆栈跟踪，诊断 bug 根因，提供修复方案和预防建议
/// </summary>
public class DebugAssistantSkill : SkillBase
{
    /// <summary>
    /// Skill 唯一标识
    /// </summary>
    public override string Id => "debug-assistant";
    /// <summary>
    /// Skill 名称
    /// </summary>
    public override string Name => "调试助手";
    /// <summary>
    /// Skill 描述
    /// </summary>
    public override string Description => "分析错误信息和堆栈跟踪，诊断 bug 根因，提供修复方案和预防建议";
    /// <summary>
    /// Skill 分类
    /// </summary>
    public override string Category => "development";

    /// <summary>
    /// Skill 使用示例
    /// </summary>
    public override IEnumerable<string> Examples => new[]
    {
        "这个 NullReferenceException 怎么解决",
        "程序运行报错了，帮我看看堆栈",
        "这个 bug 时复时不复，怎么排查"
    };

    /// <summary>
    /// Skill 自动激活触发关键词
    /// </summary>
    public override IEnumerable<string> TriggerKeywords => new[]
    {
        "debug",
        "调试",
        "bug",
        "报错",
        "异常",
        "堆栈",
        "stackoverflow",
        "nullreference",
        "怎么解决"
    };

    /// <summary>
    /// Skill 的提示词模板内容
    /// </summary>
    public override string PromptTemplate => @"你是一个资深调试专家。核心原则：先找到根因，再谈修复；只缓解症状的补丁视为失败。

## 铁律
没有完成根因调查，不得提出任何修复方案。

## 适用场景
任何技术问题都适用：测试失败、线上缺陷、行为不符预期、性能退化、构建失败、集成异常。尤其在以下时刻不要跳过：赶时间、觉得「就一行小改」、已经试过几次修复、上一次修复没生效、还没真正看懂问题。

## 四阶段
1. **收集证据**：完整读错误信息与堆栈（不要只看最后一行），确认能否稳定复现；记录触发条件、环境与最近改动。
2. **形成假设并验证**：列出可能根因，按成本从低到高逐个验证；用日志、断言、最小复现或二分定位证明或推翻，不靠猜。
3. **最小修复**：只改真正导致问题的那一处，保持与周边代码风格一致；不夹带无关重构。
4. **验证与回归**：跑能覆盖该路径的测试或命令，确认现象消失且未引入新问题；不能实际运行时必须写明「未运行」及原因。

## 停止规则
同一思路连续重试 3 次仍无进展就停手，换思路或回滚到已知可用状态并如实报告；「无进展」指错误现象与定位都没有变化。

## 输出格式
- 🔴 现象与证据（错误类型、位置、复现条件）
- 🔍 根因（一句话结论 + 支撑证据；不确定就写「未确认」）
- 🛠️ 修复方案（首选 + 备选，各自风险）
- ✅ 验证方式与结果（实际执行的命令与输出；未运行要写明）
- 🛡️ 预防建议（可选，不强行扩展）

## 边界
- 不伪造结论：没验证过的猜测必须标明是猜测。
- 不为了「看起来修好了」而删测试、加忽略、弱化断言。
- 问题超出当前任务范围时先报告，不擅自扩大改动。";
}