namespace LuBan.AIFlow.Models.ChatAssistant;

/// <summary>
/// 聊天助手更新请求模型
/// </summary>
public class UpdateChatAssistantRequest
{
    /// <summary>
    /// 聊天id
    /// </summary>
    [JsonPropertyName("chat_id")]
    public string ChatId { get; set; }

    /// <summary>
    /// 聊天助手名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 聊天助手头像（可选）
    /// </summary>
    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    /// <summary>
    /// 数据集ID列表
    /// </summary>
    [JsonPropertyName("dataset_ids")]
    public List<string> DataSetIds { get; set; }

    /// <summary>
    /// 聊天助手描述（可选）
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 聊天助手配置（可选）
    /// </summary>
    [JsonPropertyName("config")]
    public object? Config { get; set; }
}