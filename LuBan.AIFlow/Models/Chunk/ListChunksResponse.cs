namespace LuBan.AIFlow.Models.Chunk;

/// <summary>
/// 分块列表响应模型
/// </summary>
public class ListChunksResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("data")]
    public List<ChunkInfo> Data { get; set; } = new();
}