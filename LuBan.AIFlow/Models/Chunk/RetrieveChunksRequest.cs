namespace LuBan.AIFlow.Models.Chunk;

/// <summary>
/// 检索分块请求模型
/// </summary>
public class RetrieveChunksRequest
{
    [JsonPropertyName("query")]
    public string Query { get; set; } = string.Empty;

    [JsonPropertyName("top_k")]
    public int TopK { get; set; } = 5;
}