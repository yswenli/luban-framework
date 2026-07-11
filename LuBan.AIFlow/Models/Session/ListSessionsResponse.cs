namespace LuBan.AIFlow.Models.Session;

/// <summary>
/// 会话列表响应模型
/// </summary>
public class ListSessionsResponse
{
    /// <summary>
    /// 响应代码，0 表示成功
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// 会话列表数据
    /// </summary>
    [JsonPropertyName("data")]
    public List<SessionInfo> Data { get; set; } = new();
}