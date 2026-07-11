namespace LuBan.AIFlow.Models.File;

/// <summary>
/// 文件列表查询参数
/// </summary>
public class ListFilesQuery
{
    /// <summary>
    /// 页码，默认为 1
    /// </summary>
    [JsonPropertyName("page")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// 每页数量，默认为 30
    /// </summary>
    [JsonPropertyName("page_size")]
    public int PageSize { get; set; } = 30;

    /// <summary>
    /// 排序字段，可选值：create_time（默认）、update_time
    /// </summary>
    [JsonPropertyName("orderby")]
    public string OrderBy { get; set; } = "create_time";

    /// <summary>
    /// 是否降序排序，默认为 true
    /// </summary>
    [JsonPropertyName("desc")]
    public bool Desc { get; set; } = true;

    /// <summary>
    /// 文件名称过滤
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 文件 ID 过滤
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}