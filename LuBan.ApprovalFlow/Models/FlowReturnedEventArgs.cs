namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 流程退回事件参数，当流程被退回到指定节点时触发。
/// </summary>
public class FlowReturnedEventArgs : EventArgs
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
    /// 退回目标节点ID。
    /// </summary>
    public string ReturnToNodeId { get; }
    /// <summary>
    /// 退回目标节点名称。
    /// </summary>
    public string? ReturnToNodeName { get; }
    /// <summary>
    /// 退回原因/意见。
    /// </summary>
    public string? ReturnComment { get; }
    /// <summary>
    /// 退回操作人用户ID。
    /// </summary>
    public long ReturnedByUserId { get; }
    /// <summary>
    /// 退回操作人名称。
    /// </summary>
    public string ReturnedByName { get; }
    /// <summary>
    /// 退回时间。
    /// </summary>
    public DateTime ReturnedAt { get; }
    /// <summary>
    /// 业务主键。
    /// </summary>
    public string? BusinessKey { get; }
    /// <summary>
    /// 流程变量字典。
    /// </summary>
    public Dictionary<string, object>? Variables { get; }

    /// <summary>
    /// 构造流程退回事件参数。
    /// </summary>
    public FlowReturnedEventArgs(long recordId, string? flowKey, string flowName, string returnToNodeId, string? returnToNodeName, string? returnComment, long returnedByUserId, string returnedByName, DateTime returnedAt, string? businessKey, Dictionary<string, object>? variables)
    {
        RecordId = recordId;
        FlowKey = flowKey;
        FlowName = flowName;
        ReturnToNodeId = returnToNodeId;
        ReturnToNodeName = returnToNodeName;
        ReturnComment = returnComment;
        ReturnedByUserId = returnedByUserId;
        ReturnedByName = returnedByName;
        ReturnedAt = returnedAt;
        BusinessKey = businessKey;
        Variables = variables;
    }
}