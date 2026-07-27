namespace LuBan.AIAgent.Retrieval.Chunkers;

/// <summary>
/// CSS 规则块切块器（按 {} 配对）
/// </summary>
public class RuleBlockChunker : CodeChunkerBase
{
    /// <inheritdoc />
    public override string Language { get; }

    /// <inheritdoc />
    public override IReadOnlyList<string> Extensions { get; }

    /// <summary>
    /// 创建 CSS 规则块切块器
    /// </summary>
    public RuleBlockChunker(string language, string[] extensions)
    {
        Language = language;
        Extensions = extensions;
    }

    /// <inheritdoc />
    public override IReadOnlyList<CodeChunk> Chunk(string filePath, string content)
    {
        content = content.Replace("\r\n", "\n");
        var mask = ComputeCodeMask(content);
        var offsets = ComputeLineOffsets(content);
        var lines = content.Split('\n');

        var regions = new List<(int start, int end, string? selector)>();
        int depth = 0;
        int selectorStart = -1;

        for (int i = 0; i < content.Length; i++)
        {
            if (!mask[i]) continue;
            if (content[i] == '{')
            {
                if (depth == 0) selectorStart = FindSelectorStart(content, i);
                depth++;
            }
            else if (content[i] == '}')
            {
                depth--;
                if (depth == 0 && selectorStart >= 0)
                {
                    int startLine = LineOfIndex(offsets, selectorStart) + 1;
                    int endLine = LineOfIndex(offsets, i) + 1;
                    var selector = content[selectorStart..i].Trim();
                    if (selector.Length > 80) selector = selector[..80];
                    regions.Add((startLine, endLine, selector));
                    selectorStart = -1;
                }
            }
        }

        if (regions.Count == 0)
            return AssignIndices(WindowAll(filePath, content));

        var chunks = new List<CodeChunk>();
        foreach (var (start, end, selector) in regions)
        {
            if (JoinLines(lines, start, end).Length > MaxChars)
                chunks.AddRange(WindowSplit(filePath, Language, lines, start, end, "Rule", selector));
            else
                chunks.Add(new CodeChunk
                {
                    FilePath = filePath,
                    StartLine = start,
                    EndLine = end,
                    ChunkType = "Rule",
                    SymbolName = selector,
                    Language = Language,
                    Content = JoinLines(lines, start, end)
                });
        }
        return AssignIndices(MergeSmall(chunks));
    }

    private static int FindSelectorStart(string content, int braceIdx)
    {
        int s = braceIdx - 1;
        while (s >= 0 && content[s] != '}' && content[s] != '{' && content[s] != ';') s--;
        s++;
        while (s < braceIdx && char.IsWhiteSpace(content[s])) s++;
        return s;
    }
}
