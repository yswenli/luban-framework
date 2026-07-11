namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 转办请求，将当前任务转给其他人处理。
/// </summary>
public class TransferRequest
{
    /// <summary>
    /// 流程记录ID。
    /// </summary>
    public long RecordId { get; set; }
    /// <summary>
    /// 节点ID。
    /// </summary>
    public string NodeId { get; set; } = string.Empty;
    /// <summary>
    /// 目标用户ID。
    /// </summary>
    public long TargetUserId { get; set; }
    /// <summary>
    /// 目标用户名称。
    /// </summary>
    public string TargetUserName { get; set; } = string.Empty;
    /// <summary>
    /// 转办原因/意见。
    /// </summary>
    public string? Comment { get; set; }
    /// <summary>
    /// 操作人用户ID。
    /// </summary>
    public long ActorUserId { get; set; }
}