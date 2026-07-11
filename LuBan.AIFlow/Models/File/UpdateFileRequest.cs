namespace LuBan.AIFlow.Models.File;

/// <summary>
/// 文件更新请求模型
/// </summary>
public class UpdateFileRequest
{
    /// <summary>
    /// 文件名称
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}