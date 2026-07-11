namespace LuBan.AIFlow.Models.Chunk;

/// <summary>
/// 检索分块响应模型
/// </summary>
public class RetrieveChunksResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("data")]
    public List<ChunkInfo> Data { get; set; } = new();
}