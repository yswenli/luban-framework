namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 任务查询请求，用于查询待办或已办任务列表。
/// </summary>
public class TaskQueryRequest
{
    /// <summary>
    /// 页码，默认1。
    /// </summary>
    public int Page { get; set; } = 1;
    /// <summary>
    /// 每页数量，默认20。
    /// </summary>
    public int PageSize { get; set; } = 20;
    /// <summary>
    /// 流程名称筛选。
    /// </summary>
    public string? FlowName { get; set; }
    /// <summary>
    /// 节点状态筛选。
    /// </summary>
    public string? NodeStatus { get; set; }
    /// <summary>
    /// 开始时间筛选。
    /// </summary>
    public DateTime? StartTime { get; set; }
    /// <summary>
    /// 结束时间筛选。
    /// </summary>
    public DateTime? EndTime { get; set; }
}