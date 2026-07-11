namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 工具审批请求记录，用于请求工具执行的审批
/// </summary>
public record ToolApprovalRequest
{
    /// <summary>
    /// 请求ID
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    
    /// <summary>
    /// 工具调用ID
    /// </summary>
    public string ToolCallId { get; init; } = string.Empty;
    
    /// <summary>
    /// 工具名称
    /// </summary>
    public string ToolName { get; init; } = string.Empty;
    
    /// <summary>
    /// 工具参数
    /// </summary>
    public string Arguments { get; init; } = string.Empty;
    
    /// <summary>
    /// 会话ID
    /// </summary>
    public string? SessionId { get; init; }
    
    /// <summary>
    /// 对话ID
    /// </summary>
    public string? ConversationId { get; init; }
    
    /// <summary>
    /// 聊天历史记录
    /// </summary>
    public IReadOnlyList<ChatMessage> ChatHistory { get; init; } = [];
    
    /// <summary>
    /// 请求时间（UTC）
    /// </summary>
    public DateTimeOffset RequestedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// 审批原因
    /// </summary>
    public string? Reason { get; init; }
}
