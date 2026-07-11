namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 代理执行结果记录，包含执行后的输出、状态和历史记录
/// </summary>
public record AgentResult
{
    /// <summary>
    /// 执行输出
    /// </summary>
    public string Output { get; init; } = string.Empty;
    
    /// <summary>
    /// 是否执行成功
    /// </summary>
    public bool IsSuccess { get; init; }
    
    /// <summary>
    /// 错误消息
    /// </summary>
    public string? ErrorMessage { get; init; }
    
    /// <summary>
    /// 更新后的聊天历史记录
    /// </summary>
    public IReadOnlyList<ChatMessage>? UpdatedHistory { get; init; }
}
