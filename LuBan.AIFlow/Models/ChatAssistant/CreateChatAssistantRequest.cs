namespace LuBan.AIFlow.Models.ChatAssistant;

/// <summary>
/// 聊天助手创建请求模型
/// </summary>
public class CreateChatAssistantRequest
{
    /// <summary>
    /// 聊天助手名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 数据集ID列表
    /// </summary>
    [JsonPropertyName("dataset_ids")]
    public List<string> DataSetIds { get; set; }

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
    /// 聊天助手配置
    /// </summary>
    [JsonPropertyName("config")]
    public object Config { get; set; } = new();
}