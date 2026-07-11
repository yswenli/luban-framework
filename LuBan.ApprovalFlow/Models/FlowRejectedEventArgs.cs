namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 流程拒绝事件参数，当流程被拒绝时触发。
/// </summary>
public class FlowRejectedEventArgs : EventArgs
{
    /// <summary>
    /// 流程记录ID。
    /// </summary>
    public long RecordId { get; }
    /// <summary>
    /// 流程编码。
    /// </summary>
    public string? FlowKey { get; }
    /// <summary>
    /// 流程名称。
    /// </summary>
    public string FlowName { get; }
    /// <summary>
    /// 拒绝原因/意见。
    /// </summary>
    public string? RejectReason { get; }
    /// <summary>
    /// 拒绝操作人用户ID。
    /// </summary>
    public long? RejectedByUserId { get; }
    /// <summary>
    /// 拒绝操作人名称。
    /// </summary>
    public string? RejectedByName { get; }
    /// <summary>
    /// 拒绝时间。
    /// </summary>
    public DateTime RejectedAt { get; }
    /// <summary>
    /// 业务主键。
    /// </summary>
    public string? BusinessKey { get; }
    /// <summary>
    /// 流程变量字典。
    /// </summary>
    public Dictionary<string, object>? Variables { get; }

    /// <summary>
    /// 构造流程拒绝事件参数。
    /// </summary>
    public FlowRejectedEventArgs(long recordId, string? flowKey, string flowName, string? rejectReason, long? rejectedByUserId, string? rejectedByName, DateTime rejectedAt, string? businessKey, Dictionary<string, object>? variables)
    {
        RecordId = recordId;
        FlowKey = flowKey;
        FlowName = flowName;
        RejectReason = rejectReason;
        RejectedByUserId = rejectedByUserId;
        RejectedByName = rejectedByName;
        RejectedAt = rejectedAt;
        BusinessKey = businessKey;
        Variables = variables;
    }
}