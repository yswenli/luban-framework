namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 工具执行结果记录，包含工具执行的详细信息
/// </summary>
public record ToolResult
{
    /// <summary>
    /// 工具调用ID
    /// </summary>
    public string ToolCallId { get; init; } = string.Empty;
    
    /// <summary>
    /// 执行结果内容
    /// </summary>
    public string Content { get; init; } = string.Empty;
    
    /// <summary>
    /// 是否执行成功
    /// </summary>
    public bool IsSuccess { get; init; } = true;
    
    /// <summary>
    /// 执行状态
    /// </summary>
    public ToolExecutionStatus Status { get; init; } = ToolExecutionStatus.Completed;
    
    /// <summary>
    /// 审批请求ID
    /// </summary>
    public string? ApprovalRequestId { get; init; }
    
    /// <summary>
    /// 附加数据
    /// </summary>
    public object? Data { get; init; }
}
