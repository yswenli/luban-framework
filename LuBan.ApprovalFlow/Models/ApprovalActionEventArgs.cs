namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 审批动作事件参数，当用户执行审批操作时触发。
/// </summary>
public class ApprovalActionEventArgs : EventArgs
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
    /// 操作人用户ID。
    /// </summary>
    public long ActorUserId { get; }
    /// <summary>
    /// 操作人名称。
    /// </summary>
    public string ActorName { get; }
    /// <summary>
    /// 操作人角色。
    /// </summary>
    public string? ActorRole { get; }
    /// <summary>
    /// 操作动作：approve/reject/return等。
    /// </summary>
    public string Action { get; }
    /// <summary>
    /// 审批意见。
    /// </summary>
    public string? Comment { get; }
    /// <summary>
    /// 表单数据载荷。
    /// </summary>
    public object? Payload { get; }
    /// <summary>
    /// 操作时间。
    /// </summary>
    public DateTime ActionTime { get; }
    /// <summary>
    /// 流程变量字典。
    /// </summary>
    public Dictionary<string, object>? Variables { get; }
    /// <summary>
    /// 是否系统自动操作。
    /// </summary>
    public bool IsSystemAction { get; }
    /// <summary>
    /// 业务主键。
    /// </summary>
    public string? BusinessKey { get; }

    /// <summary>
    /// 构造审批动作事件参数。
    /// </summary>
    public ApprovalActionEventArgs(long recordId, string nodeId, string nodeName, long actorUserId, string actorName, string? actorRole, string action, string? comment, object? payload, DateTime actionTime, Dictionary<string, object>? variables, bool isSystemAction = false, string? businessKey = null)
    {
        RecordId = recordId;
        NodeId = nodeId;
        NodeName = nodeName;
        ActorUserId = actorUserId;
        ActorName = actorName;
        ActorRole = actorRole;
        Action = action;
        Comment = comment;
        Payload = payload;
        ActionTime = actionTime;
        Variables = variables;
        IsSystemAction = isSystemAction;
        BusinessKey = businessKey;
    }
}