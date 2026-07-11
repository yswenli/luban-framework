namespace LuBan.AIFlow.Models.Dataset;

/// <summary>
/// 数据集更新请求模型
/// </summary>
public class UpdateDatasetRequest
{
    /// <summary>
    /// 数据集名称
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 数据集描述
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}