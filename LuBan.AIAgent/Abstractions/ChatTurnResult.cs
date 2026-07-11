namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 聊天回合结果记录，包含聊天响应、工具执行结果和待批准请求
/// </summary>
public record ChatTurnResult
{
    /// <summary>
    /// 聊天响应
    /// </summary>
    public ChatResponse Response { get; init; } = null!;
    
    /// <summary>
    /// 工具执行结果列表
    /// </summary>
    public IReadOnlyList<ToolResult>? ToolResults { get; init; }
    
    /// <summary>
    /// 待批准请求
    /// </summary>
    public ToolApprovalRequest? PendingApprovalRequest { get; init; }
}
