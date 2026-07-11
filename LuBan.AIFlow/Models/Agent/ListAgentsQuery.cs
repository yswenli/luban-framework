namespace LuBan.AIFlow.Models.Agent;

/// <summary>
/// 获取Agent列表查询参数
/// </summary>
public class ListAgentsQuery
{
    /// <summary>
    /// Agent ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// Agent 名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 页码
    /// </summary>
    [JsonPropertyName("page")]
    public int? Page { get; set; }

    /// <summary>
    /// 每页数量
    /// </summary>
    [JsonPropertyName("page_size")]
    public int? PageSize { get; set; }

    /// <summary>
    /// 排序字段
    /// </summary>
    [JsonPropertyName("orderby")]
    public string OrderBy { get; set; }

    /// <summary>
    /// 是否降序
    /// </summary>
    [JsonPropertyName("desc")]
    public bool? Desc { get; set; }
}