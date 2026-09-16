/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Rules
*文件名： ContextInjectBuilder
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/16
*描述：context-build 规则注入构造器，供常规对话与编排分支共用
*
*****************************************************************************/
namespace LuBan.AIAgent.Rules;

/// <summary>
/// context-build 规则注入构造器。抽取记忆召回等 context-build 规则的注入文本，
/// 供常规对话（<c>SessionChatHistoryProvider</c>）与编排分支（<c>Orchestrator</c>）共用，
/// 避免编排分支绕开会话历史后丢失长期记忆召回。
/// </summary>
public class ContextInjectBuilder
{
    private readonly RuleEngine? _ruleEngine;

    /// <summary>
    /// 创建 context-build 注入构造器。
    /// </summary>
    /// <param name="ruleEngine">规则引擎；为 null 时始终返回空结果。</param>
    public ContextInjectBuilder(RuleEngine? ruleEngine = null)
    {
        _ruleEngine = ruleEngine;
    }

    /// <summary>
    /// 对用户输入执行 context-build 规则评估，返回需要注入的上下文文本列表。
    /// 规则缺失、输入为空或评估异常时返回空列表，绝不抛出。
    /// </summary>
    /// <param name="userInput">用户原始输入。</param>
    /// <returns>注入文本列表（可能为空）。</returns>
    public async Task<IReadOnlyList<string>> BuildAsync(string? userInput)
    {
        if (_ruleEngine == null || string.IsNullOrWhiteSpace(userInput))
            return Array.Empty<string>();

        try
        {
            var eval = await _ruleEngine.EvaluateAsync(new RuleContext
            {
                ActionType = "context-build",
                UserInput = userInput
            });

            return eval.Inject.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        }
        catch (Exception ex)
        {
            Logger.Error("context-build 规则评估失败", ex, userInput);
            return Array.Empty<string>();
        }
    }

    /// <summary>
    /// 对用户输入执行 context-build 规则评估，并把注入文本合并为单个字符串。
    /// </summary>
    /// <param name="userInput">用户原始输入。</param>
    /// <returns>合并后的注入文本；无内容时返回 null。</returns>
    public async Task<string?> BuildTextAsync(string? userInput)
    {
        var parts = await BuildAsync(userInput);
        return parts.Count == 0 ? null : string.Join("\n\n", parts);
    }
}