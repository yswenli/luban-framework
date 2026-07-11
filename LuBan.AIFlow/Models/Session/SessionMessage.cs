namespace LuBan.AIFlow.Models.Session;

/// <summary>
/// 会话消息模型
/// </summary>
public class SessionMessage
{
    /// <summary>
    /// 消息角色
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// 消息内容
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}