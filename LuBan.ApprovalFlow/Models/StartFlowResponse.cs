namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 启动流程响应，返回流程启动后的结果信息。
/// </summary>
public class StartFlowResponse
{
    /// <summary>
    /// 流程记录ID。
    /// </summary>
    public long RecordId { get; set; }
    /// <summary>
    /// 流程状态：pending/running/finished。
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
    /// 待处理任务列表。
    /// </summary>
    public List<PendingTaskInfo>? PendingTasks { get; set; }
}