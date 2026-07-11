namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 工具执行结果，包含工具执行的结果和待审批请求
/// </summary>
public record ToolExecutionOutcome
{
    /// <summary>
    /// 工具执行结果
    /// </summary>
    public ToolResult Result { get; init; } = null!;
    
    /// <summary>
    /// 待审批请求
    /// </summary>
    public ToolApprovalRequest? PendingApprovalRequest { get; init; }
}
