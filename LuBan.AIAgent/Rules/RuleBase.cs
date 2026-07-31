/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Rules
*文件名： RuleBase
*版本号： V1.0.0.0
*唯一标识：37ab0c9a-cede-4105-bf72-84ce362efc3b
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：规则基类
*
*=================================================
*修改标记
*修改时间：2026/7/27
*修改人： yswenli
*版本号： V1.0.0.0
*描述：规则基类
*
*****************************************************************************/

namespace LuBan.AIAgent.Rules;

/// <summary>
/// 规则基类
/// </summary>
public abstract class RuleBase : IRule
{
    /// <summary>
    /// 规则 ID
    /// </summary>
    public abstract string Id { get; }

    /// <summary>
    /// 规则名称
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// 规则描述
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// 规则优先级
    /// </summary>
    public virtual int Priority => 0;

    /// <summary>
    /// 规则是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 检查规则是否适用
    /// </summary>
    public abstract bool IsApplicable(RuleContext context);

    /// <summary>
    /// 执行规则
    /// </summary>
    public abstract Task<RuleResult> ExecuteAsync(RuleContext context);

    /// <summary>
    /// 快速创建允许结果
    /// </summary>
    protected static RuleResult Allow(string? message = null) => RuleResult.AllowResult(message);

    /// <summary>
    /// 快速创建拒绝结果
    /// </summary>
    protected static RuleResult Deny(string message) => RuleResult.DenyResult(message);

    /// <summary>
    /// 快速创建修改参数结果
    /// </summary>
    protected static RuleResult ModifyArgs(Dictionary<string, object?> args, string? message = null)
        => RuleResult.ModifyResult(args, message);
}
