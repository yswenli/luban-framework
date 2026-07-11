namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 转办事件参数，当任务被转办给其他人时触发。
/// </summary>
public class TransferEventArgs : EventArgs
{
    /// <summary>
    /// 流程记录ID。
    /// </summary>
    public long RecordId { get; }
    /// <summary>
    /// 节点ID。
    /// </summary>
    public string NodeId { get; }
    /// <summary>
    /// 节点名称。
    /// </summary>
    public string NodeName { get; }
    /// <summary>
    /// 原任务所属用户ID。
    /// </summary>
    public long OriginalUserId { get; }
    /// <summary>
    /// 原任务所属用户名称。
    /// </summary>
    public string OriginalUserName { get; }
    /// <summary>
    /// 目标用户ID。
    /// </summary>
    public long TargetUserId { get; }
    /// <summary>
    /// 目标用户名称。
    /// </summary>
    public string TargetUserName { get; }
    /// <summary>
    /// 转办原因/意见。
    /// </summary>
    public string? Comment { get; }
    /// <summary>
    /// 转办时间。
    /// </summary>
    public DateTime TransferTime { get; }

    /// <summary>
    /// 构造转办事件参数。
    /// </summary>
    public TransferEventArgs(long recordId, string nodeId, string nodeName, long originalUserId, string originalUserName, long targetUserId, string targetUserName, string? comment, DateTime transferTime)
    {
        RecordId = recordId;
        NodeId = nodeId;
        NodeName = nodeName;
        OriginalUserId = originalUserId;
        OriginalUserName = originalUserName;
        TargetUserId = targetUserId;
        TargetUserName = targetUserName;
        Comment = comment;
        TransferTime = transferTime;
    }
}