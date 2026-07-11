namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 聊天回合执行上下文，包含执行聊天回合所需的所有信息
/// </summary>
public record ChatTurnExecutionContext
{
    /// <summary>
    /// 聊天回合执行类型
    /// </summary>
    public ChatTurnExecutionKind Kind { get; init; }
    
    /// <summary>
    /// 模型ID
    /// </summary>
    public string ModelId { get; init; } = string.Empty;
    
    /// <summary>
    /// 输入文本
    /// </summary>
    public string? Input { get; init; }
    
    /// <summary>
    /// 消息列表
    /// </summary>
    public IReadOnlyList<ChatMessage> Messages { get; init; } = [];
    
    /// <summary>
    /// 聊天选项
    /// </summary>
    public ChatOptions? Options { get; set; }
    
    /// <summary>
    /// 会话ID
    /// </summary>
    public string? SessionId { get; init; }
    
    /// <summary>
    /// 对话ID
    /// </summary>
    public string? ConversationId { get; init; }
    
    /// <summary>
    /// 服务提供者
    /// </summary>
    public IServiceProvider? ServiceProvider { get; init; }
    
    /// <summary>
    /// 上下文项字典
    /// </summary>
    public IDictionary<string, object?> Items { get; } = new Dictionary<string, object?>();
}
