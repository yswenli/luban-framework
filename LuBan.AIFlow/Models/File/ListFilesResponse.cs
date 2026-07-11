namespace LuBan.AIFlow.Models.File;

/// <summary>
/// 文件列表响应模型
/// </summary>
public class ListFilesResponse
{
    /// <summary>
    /// 响应代码，0 表示成功
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// 文件列表数据
    /// </summary>
    [JsonPropertyName("data")]
    public List<FileInfo> Data { get; set; } = new();
}