namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 退回请求，将流程退回到指定节点。
/// </summary>
public class ReturnRequest
{
    /// <summary>
    /// 流程记录ID。
    /// </summary>
    public long RecordId { get; set; }
    /// <summary>
    /// 退回目标节点ID，为空则退回到发起节点。
    /// </summary>
    public string? ReturnToNodeId { get; set; }
    /// <summary>
    /// 退回原因/意见。
    /// </summary>
    public string? Comment { get; set; }
    /// <summary>
    /// 表单数据载荷。
    /// </summary>
    public object? Payload { get; set; }
    /// <summary>
    /// 操作人用户ID。
    /// </summary>
    public long ActorUserId { get; set; }
}