namespace LuBan.AIFlow.Models.Chunk;

/// <summary>
/// 更新分块请求模型
/// </summary>
public class UpdateChunkRequest
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("metadata")]
    public object? Metadata { get; set; }
}