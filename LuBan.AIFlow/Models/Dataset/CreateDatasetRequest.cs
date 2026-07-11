namespace LuBan.AIFlow.Models.Dataset;

/// <summary>
/// 数据集创建请求模型
/// </summary>
public class CreateDatasetRequest
{
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
}