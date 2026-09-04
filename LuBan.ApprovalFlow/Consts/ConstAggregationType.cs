/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Consts
*文件名： ConstAggregationType.cs
*版本号： V1.0.0.0
*唯一标识：d50017d8-f1e4-47c1-ba9a-8567c6fb6cde
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：ConstAggregationType 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ConstAggregationType 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Consts;

/// <summary>
/// 聚合类型常量，用于定义多审批人时的聚合策略
/// </summary>
public class ConstAggregationType
{
    /// <summary>
    /// 自动通过，节点启动时自动通过，无需审批人操作
    /// </summary>
    public const string AutoApprove = "auto_approve";

    /// <summary>
    /// 全部通过，所有审批人都需同意
    /// </summary>
    public const string AllApprove = "all_approve";

    /// <summary>
    /// 任一通过，任意一人同意即可
    /// </summary>
    public const string AnyApprove = "any_approve";

    /// <summary>
    /// 多数通过，超过半数同意即可
    /// </summary>
    public const string MajorityApprove = "majority_approve";

    /// <summary>
    /// 百分比通过，按设定百分比同意即可
    /// </summary>
    public const string PercentageApprove = "percentage_approve";

    /// <summary>
    /// 自定义聚合规则
    /// </summary>
    public const string Custom = "custom";
}