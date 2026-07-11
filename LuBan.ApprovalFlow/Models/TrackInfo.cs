namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 流程跟踪信息，用于展示流程执行轨迹。
/// </summary>
public class TrackInfo
{
    /// <summary>
    /// 流程记录ID。
    /// </summary>
    public long RecordId { get; set; }
    /// <summary>
    /// 流程名称。
    /// </summary>
    public string FlowName { get; set; } = string.Empty;
    /// <summary>
    /// 流程状态。
    /// </summary>
    public string Status { get; set; } = string.Empty;
    /// <summary>
    /// 发起人信息。
    /// </summary>
    public UserInfo Initiator { get; set; }
    /// <summary>
    /// 流程开始时间。
    /// </summary>
    public DateTime StartedAt { get; set; }
    /// <summary>
    /// 流程完成时间。
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    /// <summary>
    /// 节点轨迹信息列表。
    /// </summary>
    public List<NodeTrackInfo>? Nodes { get; set; }
    /// <summary>
    /// 边轨迹信息列表。
    /// </summary>
    public List<EdgeTrackInfo>? Edges { get; set; }
    /// <summary>
    /// 历史步骤列表。
    /// </summary>
    public List<StepInfo>? HistorySteps { get; set; }
}

/// <summary>
/// 用户信息，描述用户的基本属性。
/// </summary>
public class UserInfo
{
    /// <summary>
    /// 用户ID。
    /// </summary>
    public long UserId { get; set; }
    /// <summary>
    /// 用户名称。
    /// </summary>
    public string UserName { get; set; } = string.Empty;
}

/// <summary>
/// 节点轨迹信息，描述单个节点的执行状态。
/// </summary>
public class NodeTrackInfo
{
    /// <summary>
    /// 节点ID。
    /// </summary>
    public string NodeId { get; set; } = string.Empty;
    /// <summary>
    /// 节点名称。
    /// </summary>
    public string NodeName { get; set; } = string.Empty;
    /// <summary>
    /// 节点类型。
    /// </summary>
    public string NodeType { get; set; } = string.Empty;
    /// <summary>
    /// 节点状态。
    /// </summary>
    public string NodeStatus { get; set; } = string.Empty;
    /// <summary>
    /// 节点在画布上的X坐标。
    /// </summary>
    public double? X { get; set; }
    /// <summary>
    /// 节点在画布上的Y坐标。
    /// </summary>
    public double? Y { get; set; }
    /// <summary>
    /// 节点开始时间。
    /// </summary>
    public DateTime? StartedAt { get; set; }
    /// <summary>
    /// 节点处理完成时间。
    /// </summary>
    public DateTime? ProcessedAt { get; set; }
    /// <summary>
    /// 节点处理时长（秒）。
    /// </summary>
    public int? Duration { get; set; }
    /// <summary>
    /// 通过人数。
    /// </summary>
    public int ApprovedCount { get; set; }
    /// <summary>
    /// 拒绝人数。
    /// </summary>
    public int RejectedCount { get; set; }
    /// <summary>
    /// 总处理人数。
    /// </summary>
    public int TotalCount { get; set; }
    /// <summary>
    /// 已通过用户列表。
    /// </summary>
    public List<UserInfo>? ApprovedUsers { get; set; }
    /// <summary>
    /// 已拒绝用户列表。
    /// </summary>
    public List<UserInfo>? RejectedUsers { get; set; }
    /// <summary>
    /// 待处理用户列表。
    /// </summary>
    public List<UserInfo>? PendingUsers { get; set; }
    /// <summary>
    /// 路由结果。
    /// </summary>
    public string? RouteResult { get; set; }
    /// <summary>
    /// 步骤详情列表。
    /// </summary>
    public List<StepInfo>? Steps { get; set; }
}

/// <summary>
/// 边轨迹信息，描述连线状态。
/// </summary>
public class EdgeTrackInfo
{
    /// <summary>
    /// 源节点ID。
    /// </summary>
    public string SourceId { get; set; } = string.Empty;
    /// <summary>
    /// 目标节点ID。
    /// </summary>
    public string TargetId { get; set; } = string.Empty;
    /// <summary>
    /// 边状态。
    /// </summary>
    public string Status { get; set; } = string.Empty;
}