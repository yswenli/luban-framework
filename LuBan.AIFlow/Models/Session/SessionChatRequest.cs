namespace LuBan.AIFlow.Models.Session;

/// <summary>
/// 会话聊天请求模型
/// </summary>
public class SessionChatRequest
{
    /// <summary>
    /// 消息内容
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 是否流式响应
    /// </summary>
    [JsonPropertyName("stream")]
    public bool Stream { get; set; }
}