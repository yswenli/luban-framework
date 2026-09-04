/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： GatewayRule.cs
*版本号： V1.0.0.0
*唯一标识：907582d8-7179-444b-86c5-29fd18e52f39
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：GatewayRule 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：GatewayRule 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 网关规则，定义条件网关的分支路由规则。
/// </summary>
public class GatewayRule
{
    /// <summary>
    /// 关联的边ID，用于确定路由目标。
    /// </summary>
    public string EdgeId { get; set; } = string.Empty;
    /// <summary>
    /// 边的显示文本，如"通过"、"退回"等。
    /// </summary>
    public string? EdgeText { get; set; }
    /// <summary>
    /// 条件列表，用于判断是否匹配此规则。
    /// </summary>
    public List<RuleCondition>? Conditions { get; set; }
    /// <summary>
    /// 条件逻辑：and/or，默认and。
    /// </summary>
    public string Logic { get; set; } = "and";
}