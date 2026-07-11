namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 统计信息，描述用户的审批统计数据。
/// </summary>
public class StatisticsInfo
{
    /// <summary>
    /// 用户ID。
    /// </summary>
    public long UserId { get; set; }
    /// <summary>
    /// 用户名称。
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 发起的流程总数。
    /// </summary>
    public int InitiatedTotal { get; set; }
    /// <summary>
    /// 发起的待处理流程数。
    /// </summary>
    public int InitiatedPending { get; set; }
    /// <summary>
    /// 发起的已通过流程数。
    /// </summary>
    public int InitiatedApproved { get; set; }
    /// <summary>
    /// 发起的已取消流程数。
    /// </summary>
    public int InitiatedCancelled { get; set; }
    /// <summary>
    /// 发起的已拒绝流程数。
    /// </summary>
    public int InitiatedRejected { get; set; }

    /// <summary>
    /// 审批的流程总数。
    /// </summary>
    public int ApprovedTotal { get; set; }
    /// <summary>
    /// 待审批的流程数。
    /// </summary>
    public int ApprovedPending { get; set; }
    /// <summary>
    /// 已审批通过的流程数。
    /// </summary>
    public int ApprovedApproved { get; set; }
    /// <summary>
    /// 已审批取消的流程数。
    /// </summary>
    public int ApprovedCancelled { get; set; }
    /// <summary>
    /// 已审批拒绝的流程数。
    /// </summary>
    public int ApprovedRejected { get; set; }
    /// <summary>
    /// 已审批退回的流程数。
    /// </summary>
    public int ApprovedReturned { get; set; }

    /// <summary>
    /// 统计更新时间。
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}