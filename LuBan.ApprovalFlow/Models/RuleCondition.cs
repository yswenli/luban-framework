/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： RuleCondition.cs
*版本号： V1.0.0.0
*唯一标识：72b11d3c-31f9-48ea-ab34-1f20dc17a977
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：RuleCondition 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：RuleCondition 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 规则条件，定义单个条件表达式。
/// </summary>
public class RuleCondition
{
    /// <summary>
    /// 条件字段名称。
    /// </summary>
    public string Field { get; set; } = string.Empty;
    /// <summary>
    /// 比较操作符：eq/ne/gt/gte/lt/lte/contains/startswith/endswith/empty/notempty/in/notin。
    /// </summary>
    public string Operator { get; set; } = ConstOperatorType.Equal;
    /// <summary>
    /// 比较值。
    /// </summary>
    public object? Value { get; set; }
}