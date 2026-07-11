namespace LuBan.AIFlow.Models.Dataset;

/// <summary>
/// 数据集信息模型
/// </summary>
public class DatasetInfo
{
    /// <summary>
    /// 数据集 ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 数据集名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 数据集描述
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 创建时间戳
    /// </summary>
    [JsonPropertyName("create_time")]
    public long CreateTime { get; set; }

    /// <summary>
    /// 更新时间戳
    /// </summary>
    [JsonPropertyName("update_time")]
    public long UpdateTime { get; set; }
}