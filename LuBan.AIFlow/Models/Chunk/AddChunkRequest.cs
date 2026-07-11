namespace LuBan.AIFlow.Models.Chunk;

/// <summary>
/// 添加分块请求模型
/// </summary>
public class AddChunkRequest
{
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("metadata")]
    public object? Metadata { get; set; }
}