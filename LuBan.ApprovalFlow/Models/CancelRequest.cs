namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 取消请求，取消整个审批流程。
/// </summary>
public class CancelRequest
{
    /// <summary>
    /// 流程记录ID。
    /// </summary>
    public long RecordId { get; set; }
    /// <summary>
    /// 取消原因/意见。
    /// </summary>
    public string? Comment { get; set; }
    /// <summary>
    /// 操作人用户ID。
    /// </summary>
    public long ActorUserId { get; set; }
    /// <summary>
    /// 操作人角色列表。
    /// </summary>
    public List<string>? ActorRoles { get; set; }
}