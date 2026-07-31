/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Rules
*文件名： RuleCheckedAIFunction
*版本号： V1.0.0.0
*唯一标识：cddeaf29-da7e-469d-bf9d-d0ecf4201843
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/29
*描述：规则检查装饰器，工具调用前先经 RuleEngine 评估
*
*=================================================
*修改标记
*修改时间：2026/7/29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：规则检查装饰器，工具调用前先经 RuleEngine 评估
*
*****************************************************************************/

using Microsoft.Extensions.AI;

namespace LuBan.AIAgent.Rules;

/// <summary>
/// 规则检查装饰器：工具调用前先经 RuleEngine 评估，deny 直接返回拒绝文本给模型
/// </summary>
public class RuleCheckedAIFunction : DelegatingAIFunction
{
    private readonly RuleEngine _ruleEngine;

    /// <summary>
    /// 创建 RuleCheckedAIFunction 实例
    /// </summary>
    /// <param name="innerFunction">被装饰的工具函数</param>
    /// <param name="ruleEngine">规则引擎</param>
    public RuleCheckedAIFunction(AIFunction innerFunction, RuleEngine ruleEngine)
        : base(innerFunction)
    {
        _ruleEngine = ruleEngine;
    }

    /// <summary>
    /// 调用前评估规则；deny 终止，modify 用修改后参数执行
    /// </summary>
    protected override async ValueTask<object?> InvokeCoreAsync(
        AIFunctionArguments arguments, CancellationToken cancellationToken)
    {
        var result = await _ruleEngine.EvaluateAsync(new RuleContext
        {
            ActionType = "tool-call",
            Target = Name,
            Arguments = new Dictionary<string, object?>(arguments)
        });

        if (!result.Allow)
        {
            return $"工具 \"{Name}\" 的调用被规则拒绝：{result.Message ?? "未提供原因"}";
        }

        if (result.ModifiedArguments != null)
        {
            arguments = new AIFunctionArguments(result.ModifiedArguments);
        }

        return await base.InvokeCoreAsync(arguments, cancellationToken);
    }
}
