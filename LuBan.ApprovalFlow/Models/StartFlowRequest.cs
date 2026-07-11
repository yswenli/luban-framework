namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 启动流程请求，包含启动审批流所需的所有参数。
/// </summary>
public class StartFlowRequest
{
    /// <summary>
    /// 流程定义ID（与FlowCode二选一）。
    /// </summary>
    public long? FlowId { get; set; }
    /// <summary>
    /// 流程编码（与FlowId二选一）。
    /// </summary>
    public string? FlowCode { get; set; }
    /// <summary>
    /// 业务主键，用于关联业务数据。
    /// </summary>
    public string? BusinessKey { get; set; }
    /// <summary>
    /// 表单数据载荷。
    /// </summary>
    public object? FormPayload { get; set; }
    /// <summary>
    /// 流程变量字典。
    /// </summary>
    public Dictionary<string, object>? Variables { get; set; }
    /// <summary>
    /// 发起人用户ID。
    /// </summary>
    public long InitiatorUserId { get; set; }
    /// <summary>
    /// 发起人名称。
    /// </summary>
    public string InitiatorName { get; set; } = string.Empty;
}