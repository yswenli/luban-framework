namespace LuBan.AIFlow.Models.File;

/// <summary>
/// 文件解析请求模型
/// </summary>
public class ParseFileRequest
{
    /// <summary>
    /// 文件 ID 列表
    /// </summary>
    [JsonPropertyName("file_ids")]
    public List<string> FileIds { get; set; } = new();
}