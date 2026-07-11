namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 撤回请求，发起人撤回已提交的流程。
/// </summary>
public class WithdrawRequest
{
    /// <summary>
    /// 流程记录ID。
    /// </summary>
    public long RecordId { get; set; }
    /// <summary>
    /// 撤回原因/意见。
    /// </summary>
    public string? Comment { get; set; }
    /// <summary>
    /// 操作人用户ID（通常为发起人）。
    /// </summary>
    public long ActorUserId { get; set; }
}