namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 工具审批决策记录，用于表示工具执行的审批结果
/// </summary>
public record ToolApprovalDecision
{
    /// <summary>
    /// 审批请求ID
    /// </summary>
    public string ApprovalRequestId { get; init; } = string.Empty;
    
    /// <summary>
    /// 是否已批准
    /// </summary>
    public bool Approved { get; init; }
    
    /// <summary>
    /// 是否待处理
    /// </summary>
    public bool IsPending { get; init; }
    
    /// <summary>
    /// 评论
    /// </summary>
    public string? Comment { get; init; }
    
    /// <summary>
    /// 决策时间（UTC）
    /// </summary>
    public DateTimeOffset DecidedAtUtc { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// 创建批准的决策
    /// </summary>
    /// <param name="approvalRequestId">审批请求ID</param>
    /// <param name="comment">评论</param>
    /// <returns>工具审批决策</returns>
    public static ToolApprovalDecision ApprovedDecision(string approvalRequestId, string? comment = null)
        => new()
        {
            ApprovalRequestId = approvalRequestId,
            Approved = true,
            Comment = comment
        };

    /// <summary>
    /// 创建拒绝的决策
    /// </summary>
    /// <param name="approvalRequestId">审批请求ID</param>
    /// <param name="comment">评论</param>
    /// <returns>工具审批决策</returns>
    public static ToolApprovalDecision DeniedDecision(string approvalRequestId, string? comment = null)
        => new()
        {
            ApprovalRequestId = approvalRequestId,
            Approved = false,
            Comment = comment
        };

    /// <summary>
    /// 创建待处理的决策
    /// </summary>
    /// <param name="approvalRequestId">审批请求ID</param>
    /// <param name="comment">评论</param>
    /// <returns>工具审批决策</returns>
    public static ToolApprovalDecision PendingDecision(string approvalRequestId, string? comment = null)
        => new()
        {
            ApprovalRequestId = approvalRequestId,
            IsPending = true,
            Comment = comment
        };
}
