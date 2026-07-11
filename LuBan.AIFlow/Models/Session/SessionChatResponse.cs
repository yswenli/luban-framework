namespace LuBan.AIFlow.Models.Session;

/// <summary>
/// 会话聊天响应模型
/// </summary>
public class SessionChatResponse
{
    /// <summary>
    /// 响应代码，0 表示成功
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// 响应消息
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 响应数据
    /// </summary>
    [JsonPropertyName("data")]
    public SessionChatData? Data { get; set; }
}