/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： AggregationResult.cs
*版本号： V1.0.0.0
*唯一标识：92fb6e16-157b-4990-baba-6978d92fc61f
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：AggregationResult 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：AggregationResult 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 聚合结果，表示多实例节点的聚合审批结果状态。
/// </summary>
public class AggregationResult
{
    /// <summary>
    /// 待处理状态。
    /// </summary>
    public static AggregationResult Pending { get; } = new("pending");
    /// <summary>
    /// 已通过状态。
    /// </summary>
    public static AggregationResult Approved { get; } = new("approved");
    /// <summary>
    /// 已拒绝状态。
    /// </summary>
    public static AggregationResult Rejected { get; } = new("rejected");
    /// <summary>
    /// 已退回状态。
    /// </summary>
    public static AggregationResult Returned { get; } = new("returned");
    /// <summary>
    /// 已取消状态。
    /// </summary>
    public static AggregationResult Cancelled { get; } = new("cancelled");

    /// <summary>
    /// 结果值。
    /// </summary>
    public string Value { get; }

    private AggregationResult(string value)
    {
        Value = value;
    }

    /// <summary>
    /// 返回字符串表示。
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// 隐式转换为字符串。
    /// </summary>
    public static implicit operator string(AggregationResult result) => result.Value;
}