namespace LuBan.AIFlow.Models.Chat;

/// <summary>
/// 聊天选择模型
/// </summary>
public class ChatChoice
{
    /// <summary>
    /// 聊天消息
    /// </summary>
    [JsonPropertyName("message")]
    public ChatMessage? Message { get; set; }

    /// <summary>
    /// 增量消息
    /// </summary>
    [JsonPropertyName("delta")]
    public ChatMessage? Delta { get; set; }

    /// <summary>
    /// 完成原因
    /// </summary>
    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }

    /// <summary>
    /// 选项索引
    /// </summary>
    [JsonPropertyName("index")]
    public int Index { get; set; }
}