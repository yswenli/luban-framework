namespace LuBan.AIFlow.Models.Session;

/// <summary>
/// 创建会话请求模型
/// </summary>
public class CreateSessionRequest
{
    /// <summary>
    /// 要创建的聊天会话的名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }
    /// <summary>
    /// 可选的用户定义 ID
    /// </summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }
}