namespace LuBan.AIFlow.Models.Session;

/// <summary>
/// 更新会话请求模型
/// </summary>
public class UpdateSessionRequest
{
    /// <summary>
    /// 会话标题
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
}