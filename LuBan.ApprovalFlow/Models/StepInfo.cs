namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 步骤信息，描述单次审批操作的详情。
/// </summary>
public class StepInfo
{
    /// <summary>
    /// 节点名称。
    /// </summary>
    public string? NodeName { get; set; }
    /// <summary>
    /// 操作人名称。
    /// </summary>
    public string ActorName { get; set; } = string.Empty;
    /// <summary>
    /// 操作人角色。
    /// </summary>
    public string? ActorRole { get; set; }
    /// <summary>
    /// 操作动作：approve/reject/return/cancel等。
    /// </summary>
    public string Action { get; set; } = string.Empty;
    /// <summary>
    /// 审批意见。
    /// </summary>
    public string? Comment { get; set; }
    /// <summary>
    /// 表单数据载荷。
    /// </summary>
    public object? Payload { get; set; }
    /// <summary>
    /// 操作时间。
    /// </summary>
    public DateTime ActionTime { get; set; }
    /// <summary>
    /// 是否系统自动操作。
    /// </summary>
    public bool IsSystemAction { get; set; }
}