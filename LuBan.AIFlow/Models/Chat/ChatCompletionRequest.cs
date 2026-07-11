namespace LuBan.AIFlow.Models.Chat;

/// <summary>
/// 聊天补全请求模型
/// </summary>
public class ChatCompletionRequest
{
    /// <summary>
    /// 模型名称
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; } = "model";

    /// <summary>
    /// 聊天消息列表
    /// </summary>
    [JsonPropertyName("messages")]
    public List<ChatMessage> Messages { get; set; } = new();

    /// <summary>
    /// 是否使用流式响应
    /// </summary>
    [JsonPropertyName("stream")]
    public bool Stream { get; set; }
}