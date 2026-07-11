namespace LuBan.AIFlow.Models.Common;

/// <summary>
/// 聊天消息模型
/// </summary>
public class ChatMessage
{
    /// <summary>
    /// 消息角色，可选值：system、user、assistant
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; set; } = "user";

    /// <summary>
    /// 消息内容
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}