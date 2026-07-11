namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 聊天请求记录，包含发送给AI模型的消息和配置
/// </summary>
public record ChatRequest
{
    /// <summary>
    /// 模型ID
    /// </summary>
    public string ModelId { get; init; } = string.Empty;
    
    /// <summary>
    /// 消息列表
    /// </summary>
    public IReadOnlyList<ChatMessage> Messages { get; init; } = [];
    
    /// <summary>
    /// 聊天选项
    /// </summary>
    public ChatOptions? Options { get; init; }
}
