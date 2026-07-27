using System.Text.RegularExpressions;

namespace LuBan.AIAgent.Retrieval.Chunkers;

/// <summary>
/// 关键字配对切块器（Ruby / Lua / VB：opener 加深、end 闭合）
/// </summary>
public class KeywordEndChunker : CodeChunkerBase
{
    private readonly Regex _openRegex;
    private readonly Regex _closeRegex;

    /// <inheritdoc />
    public override string Language { get; }

    /// <inheritdoc />
    public override IReadOnlyList<string> Extensions { get; }

    /// <summary>
    /// 创建关键字配对切块器
    /// </summary>
    public KeywordEndChunker(string language, string[] extensions, string openPattern, string closePattern)
    {
        Language = language;
        Extensions = extensions;
        _openRegex = new Regex(openPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        _closeRegex = new Regex(closePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }

    /// <inheritdoc />
    public override IReadOnlyList<CodeChunk> Chunk(string filePath, string content)
    {
        content = content.Replace("\r\n", "\n");
        var lines = content.Split('\n');

        var regions = new List<(int start, int end, string type, string? symbol)>();
        int depth = 0;
        int regionStart = -1;
        string regionType = "Function";
        string? regionSymbol = null;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (depth == 0)
            {
                var m = Regex.Match(line, @"^\s*(def|class|module|function|sub)\b", RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    regionStart = i;
                    regionType = m.Value.Equals("class", StringComparison.OrdinalIgnoreCase) ||
                                  m.Value.Equals("module", StringComparison.OrdinalIgnoreCase) ? "Class" : "Function";
                    var sm = Regex.Match(line[(m.Index + m.Length)..], @"[A-Za-z_][\w.]*");
                    regionSymbol = sm.Success ? sm.Value : null;
                }
            }
            if (_openRegex.IsMatch(line)) depth++;
            if (_closeRegex.IsMatch(line))
            {
                depth--;
                if (depth <= 0 && regionStart >= 0)
                {
                    regions.Add((regionStart + 1, i + 1, regionType, regionSymbol));
                    depth = 0;
                    regionStart = -1;
                }
            }
        }

        if (regions.Count == 0)
            return AssignIndices(WindowAll(filePath, content));

        var chunks = new List<CodeChunk>();
        foreach (var (start, end, type, symbol) in regions)
        {
            if (JoinLines(lines, start, end).Length > MaxChars)
                chunks.AddRange(WindowSplit(filePath, Language, lines, start, end, type, symbol));
            else
                chunks.Add(new CodeChunk
                {
                    FilePath = filePath,
                    StartLine = start,
                    EndLine = end,
                    ChunkType = type,
                    SymbolName = symbol,
                    Language = Language,
                    Content = JoinLines(lines, start, end)
                });
        }
        return AssignIndices(MergeSmall(chunks));
    }
}
