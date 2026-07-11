namespace LuBan.AIFlow.Models.ChatAssistant;

/// <summary>
/// 聊天助手信息模型
/// </summary>
public class ChatAssistantInfo
{
    /// <summary>
    /// 聊天助手唯一标识
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 聊天助手名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 聊天助手描述
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 聊天助手头像
    /// </summary>
    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    /// <summary>
    /// 聊天助手配置信息
    /// </summary>
    [JsonPropertyName("config")]
    public object Config { get; set; } = new();

    /// <summary>
    /// 创建时间（Unix时间戳，毫秒）
    /// </summary>
    [JsonPropertyName("create_time")]
    public long CreateTime { get; set; }

    /// <summary>
    /// 更新时间（Unix时间戳，毫秒）
    /// </summary>
    [JsonPropertyName("update_time")]
    public long UpdateTime { get; set; }
}