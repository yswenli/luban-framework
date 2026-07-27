namespace LuBan.AIAgent.Retrieval;

/// <summary>
/// 文本语义切块器接口
/// </summary>
public interface ICodeChunker
{
    /// <summary>
    /// 语言标识，如 csharp、html
    /// </summary>
    string Language { get; }

    /// <summary>
    /// 支持的扩展名（含点，如 .cs）
    /// </summary>
    IReadOnlyList<string> Extensions { get; }

    /// <summary>
    /// 切块
    /// </summary>
    IReadOnlyList<CodeChunk> Chunk(string filePath, string content);
}
