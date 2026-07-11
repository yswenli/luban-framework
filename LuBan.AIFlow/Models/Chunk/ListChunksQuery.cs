namespace LuBan.AIFlow.Models.Chunk;

/// <summary>
/// 分块列表查询参数
/// </summary>
public class ListChunksQuery
{
    [JsonPropertyName("page")]
    public int Page { get; set; } = 1;

    [JsonPropertyName("page_size")]
    public int PageSize { get; set; } = 30;

    [JsonPropertyName("orderby")]
    public string OrderBy { get; set; } = "create_time";

    [JsonPropertyName("desc")]
    public bool Desc { get; set; } = true;

    [JsonPropertyName("id")]
    public string? Id { get; set; }
}