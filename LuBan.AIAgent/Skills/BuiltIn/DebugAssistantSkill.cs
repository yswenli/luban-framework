/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Skills.BuiltIn
*文件名： DebugAssistantSkill
*版本号： V1.0.0.0
*唯一标识：a1b2c3d4-e5f6-7890-abcd-ef1234567004
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/3
*描述：调试助手 Skill - 诊断 bug、分析根因、提供修复方案
*
*=================================================
*修改标记
*修改时间：2026/8/3
*修改人： yswenli
*版本号： V1.0.0.0
*描述：调试助手 Skill - 诊断 bug、分析根因、提供修复方案
*
*****************************************************************************/

namespace LuBan.AIAgent.Skills.BuiltIn;

/// <summary>
/// 调试助手 Skill - 诊断 bug、分析根因、提供修复方案
/// </summary>
public class DebugAssistantSkill : SkillBase
{
    /// <summary>
    /// Skill ID
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
    /// 使用示例
    /// </summary>
    public override IEnumerable<string> Examples => new[]
    {
        "这个 NullReferenceException 怎么解决",
        "程序运行报错了，帮我看看堆栈",
        "这个 bug 时复时不复，怎么排查"
    };

    /// <summary>
    /// 执行 Skill
    /// </summary>
    public override async Task<SkillResult> ExecuteAsync(SkillContext context, string input)
    {
        UpdateStatus(context, "正在诊断问题...");

        var systemPrompt = @"你是一个资深的调试专家和问题排查专家。请分析用户提供的错误信息和代码，诊断 bug 根因：

1. **错误分析**：解析错误信息/异常类型/堆栈跟踪，定位问题发生的根源
2. **根因定位**：追溯问题的根本原因，不只是表面症状
3. **修复方案**：提供具体的代码修复方案（可能多个）
4. **验证方法**：如何验证修复是否有效
5. **预防措施**：如何避免类似问题再次发生

分析维度：
- 空引用 / 类型转换 / 索引越界 / 并发竞争 / 资源泄漏
- 异步死锁 / 线程安全 / 缓存一致性
- 配置问题 / 环境差异 / 依赖版本冲突

请用以下格式输出：
🔴 **错误定位**：
- 异常类型：
- 触发位置：
- 错误消息：

🔍 **根因分析**：
> 问题的根本原因是...

💊 **修复方案**：

**方案一**（推荐）：
```csharp
// 修复代码
```
- 说明：...

**方案二**（备选）：
```csharp
// 修复代码
```
- 说明：...

✅ **验证方法**：
- ...

🛡️ **预防措施**：
- ...";

        var result = await CallAgentAsync(context, $"{systemPrompt}\n\n{input}");

        return SkillResult.Ok(result ?? "");
    }
}
