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