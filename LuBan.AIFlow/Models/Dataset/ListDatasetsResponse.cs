namespace LuBan.AIFlow.Models.Dataset;

/// <summary>
/// 数据集列表响应模型
/// </summary>
public class ListDatasetsResponse
{
    /// <summary>
    /// 响应代码，0 表示成功
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// 数据集列表数据
    /// </summary>
    [JsonPropertyName("data")]
    public List<DatasetInfo> Data { get; set; } = new();
}