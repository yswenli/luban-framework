/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： GatewayRuleResult.cs
*版本号： V1.0.0.0
*唯一标识：0b499d3d-b27b-45d1-b6eb-cd6109b04054
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：GatewayRuleResult 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：GatewayRuleResult 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 网关规则匹配结果，表示条件判断后的路由信息。
/// </summary>
public class GatewayRuleResult
{
    /// <summary>
    /// 是否匹配到规则。
    /// </summary>
    public bool Matched { get; set; }
    /// <summary>
    /// 匹配的边ID。
    /// </summary>
    public string? EdgeId { get; set; }
    /// <summary>
    /// 匹配的边文本。
    /// </summary>
    public string? EdgeText { get; set; }
    /// <summary>
    /// 匹配的规则对象。
    /// </summary>
    public GatewayRule? Rule { get; set; }
}