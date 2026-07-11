namespace LuBan.AIFlow.Models.Chat;

/// <summary>
/// 聊天补全响应模型
/// </summary>
public class ChatCompletionResponse
{
    /// <summary>
    /// 响应 ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 响应选项列表
    /// </summary>
    [JsonPropertyName("choices")]
    public List<ChatChoice> Choices { get; set; } = new();

    /// <summary>
    /// 创建时间戳
    /// </summary>
    [JsonPropertyName("created")]
    public long Created { get; set; }

    /// <summary>
    /// 模型名称
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// 响应对象类型
    /// </summary>
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    /// <summary>
    /// Token 使用情况统计
    /// </summary>
    [JsonPropertyName("usage")]
    public TokenUsage? Usage { get; set; }
}