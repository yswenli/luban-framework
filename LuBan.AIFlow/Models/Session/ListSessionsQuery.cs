namespace LuBan.AIFlow.Models.Session;

/// <summary>
/// 会话列表查询模型
/// </summary>
public class ListSessionsQuery
{
    /// <summary>
    /// 页码
    /// </summary>
    [JsonPropertyName("page")]
    public int Page { get; set; }

    /// <summary>
    /// 每页数量
    /// </summary>
    [JsonPropertyName("page_size")]
    public int PageSize { get; set; }

    /// <summary>
    /// 排序字段
    /// </summary>
    [JsonPropertyName("order_by")]
    public string OrderBy { get; set; } = string.Empty;

    /// <summary>
    /// 是否降序
    /// </summary>
    [JsonPropertyName("desc")]
    public bool Desc { get; set; }

    /// <summary>
    /// 会话名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 会话 ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}