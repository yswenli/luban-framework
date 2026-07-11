namespace LuBan.AIFlow.Models.Chunk;

/// <summary>
/// 分块信息模型
/// </summary>
public class ChunkInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("metadata")]
    public object Metadata { get; set; } = new();

    [JsonPropertyName("create_time")]
    public long CreateTime { get; set; }

    [JsonPropertyName("update_time")]
    public long UpdateTime { get; set; }
}