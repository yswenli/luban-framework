namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 超时配置，定义节点超时处理策略。
/// </summary>
public class TimeoutConfig
{
    /// <summary>
    /// 超时时间（小时）。
    /// </summary>
    public int? TimeoutHours { get; set; }
    /// <summary>
    /// 超时动作：auto_approve/auto_reject/notify/transfer。
    /// </summary>
    public string? TimeoutAction { get; set; }
    /// <summary>
    /// 提前通知时间（小时）。
    /// </summary>
    public int? NotifyBeforeHours { get; set; }
    /// <summary>
    /// 通知间隔时间列表（小时）。
    /// </summary>
    public List<int>? NotifyIntervalHours { get; set; }
    /// <summary>
    /// 通知目标列表。
    /// </summary>
    public List<NotifyTarget>? NotifyTargets { get; set; }
}

/// <summary>
/// 通知目标，定义超时通知的接收对象。
/// </summary>
public class NotifyTarget
{
    /// <summary>
    /// 目标类型：user/role/initiator/assignee。
    /// </summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>
    /// 用户ID（当Type为user时使用）。
    /// </summary>
    public long? UserId { get; set; }
    /// <summary>
    /// 角色编码（当Type为role时使用）。
    /// </summary>
    public string? Role { get; set; }
}