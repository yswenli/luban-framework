namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 审批响应，返回审批操作后的结果信息。
/// </summary>
public class ApprovalResponse
{
    /// <summary>
    /// 流程记录ID。
    /// </summary>
    public long RecordId { get; set; }
    /// <summary>
    /// 流程状态：pending/running/finished/rejected。
    /// </summary>
    public string Status { get; set; } = string.Empty;
    /// <summary>
    /// 当前节点ID。
    /// </summary>
    public string? CurrentNodeId { get; set; }
    /// <summary>
    /// 当前节点名称。
    /// </summary>
    public string? CurrentNodeName { get; set; }
    /// <summary>
    /// 聚合结果，用于多实例节点：pending/approved/rejected/returned/cancelled。
    /// </summary>
    public string? AggregationResult { get; set; }
}