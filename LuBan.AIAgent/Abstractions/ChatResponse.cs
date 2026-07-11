namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 聊天响应记录，包含AI模型的回复和相关信息
/// </summary>
public record ChatResponse
{
    /// <summary>
    /// 聊天消息
    /// </summary>
    public ChatMessage? Message { get; init; }
    
    /// <summary>
    /// 使用信息
    /// </summary>
    public UsageInfo? Usage { get; init; }
    
    /// <summary>
    /// 完成原因
    /// </summary>
    public string? FinishReason { get; init; }
    
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; init; }
    
    /// <summary>
    /// 错误消息
    /// </summary>
    public string? ErrorMessage { get; init; }
}
