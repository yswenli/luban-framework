namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 待处理任务信息，描述用户待办任务的详情。
/// </summary>
public class PendingTaskInfo
{
    /// <summary>
    /// 任务ID。
    /// </summary>
    public long TaskId { get; set; }
    /// <summary>
    /// 流程记录ID。
    /// </summary>
    public long RecordId { get; set; }
    /// <summary>
    /// 流程名称。
    /// </summary>
    public string FlowName { get; set; } = string.Empty;
    /// <summary>
    /// 节点ID。
    /// </summary>
    public string NodeId { get; set; } = string.Empty;
    /// <summary>
    /// 节点名称。
    /// </summary>
    public string NodeName { get; set; } = string.Empty;
    /// <summary>
    /// 节点状态。
    /// </summary>
    public string NodeStatus { get; set; } = string.Empty;
    /// <summary>
    /// 业务主键。
    /// </summary>
    public string? BusinessKey { get; set; }
    /// <summary>
    /// 发起人名称。
    /// </summary>
    public string InitiatorName { get; set; } = string.Empty;
    /// <summary>
    /// 任务创建时间。
    /// </summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>
    /// 任务截止时间。
    /// </summary>
    public DateTime? DueTime { get; set; }
    /// <summary>
    /// 是否已读。
    /// </summary>
    public bool IsRead { get; set; }
    /// <summary>
    /// 表单数据。
    /// </summary>
    public Dictionary<string, object>? FormData { get; set; }
    /// <summary>
    /// 是否委托任务。
    /// </summary>
    public bool IsDelegated { get; set; }
    /// <summary>
    /// 是否转办任务。
    /// </summary>
    public bool IsTransferred { get; set; }
    /// <summary>
    /// 原任务所属用户ID（委托或转办时）。
    /// </summary>
    public long? OriginalUserId { get; set; }
}