namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 工具执行上下文，包含执行工具所需的所有信息
/// </summary>
public record ToolExecutionContext
{
    /// <summary>
    /// 默认构造函数
    /// </summary>
    public ToolExecutionContext()
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="toolCall">工具调用</param>
    public ToolExecutionContext(ToolCall toolCall)
    {
        ToolCall = toolCall;
    }

    /// <summary>
    /// 工具调用
    /// </summary>
    public ToolCall ToolCall { get; init; } = null!;
    
    /// <summary>
    /// 聊天历史记录
    /// </summary>
    public IReadOnlyList<ChatMessage> ChatHistory { get; init; } = [];
    
    /// <summary>
    /// 服务提供者
    /// </summary>
    public IServiceProvider? ServiceProvider { get; init; }
    
    /// <summary>
    /// 会话ID
    /// </summary>
    public string? SessionId { get; init; }
    
    /// <summary>
    /// 对话ID
    /// </summary>
    public string? ConversationId { get; init; }
    
    /// <summary>
    /// 元数据
    /// </summary>
    public IReadOnlyDictionary<string, object?> Metadata { get; init; } = new Dictionary<string, object?>();
}
