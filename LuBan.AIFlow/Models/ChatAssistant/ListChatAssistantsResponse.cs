namespace LuBan.AIFlow.Models.ChatAssistant;

/// <summary>
/// 聊天助手列表响应模型
/// </summary>
public class ListChatAssistantsResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("data")]
    public List<ChatAssistantInfo> Data { get; set; } = new();
}