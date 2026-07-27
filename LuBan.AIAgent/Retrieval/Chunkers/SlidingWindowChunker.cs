namespace LuBan.AIAgent.Retrieval.Chunkers;

/// <summary>
/// 滑动窗口切块器（兜底策略）
/// </summary>
public class SlidingWindowChunker : CodeChunkerBase
{
    private readonly string _language;

    /// <summary>
    /// 创建滑窗切块器
    /// </summary>
    public SlidingWindowChunker(string language = "text") => _language = language;

    /// <inheritdoc />
    public override string Language => _language;

    /// <inheritdoc />
    public override IReadOnlyList<string> Extensions => Array.Empty<string>();

    /// <inheritdoc />
    public override IReadOnlyList<CodeChunk> Chunk(string filePath, string content)
        => AssignIndices(WindowAll(filePath, content.Replace("\r\n", "\n")));
}
