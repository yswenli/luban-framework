using System.Text.RegularExpressions;

namespace LuBan.AIAgent.Retrieval.Chunkers;

/// <summary>
/// 大括号配对切块器（C 系语言家族：关键字定位 + 括号配对，仅取顶层区域）
/// </summary>
public class BracePairingChunker : CodeChunkerBase
{
    private readonly Regex _keywordRegex;

    /// <inheritdoc />
    public override string Language { get; }

    /// <inheritdoc />
    public override IReadOnlyList<string> Extensions { get; }

    /// <summary>
    /// 创建大括号配对切块器
    /// </summary>
    public BracePairingChunker(string language, string[] extensions, string keywordPattern)
    {
        Language = language;
        Extensions = extensions;
        _keywordRegex = new Regex(keywordPattern, RegexOptions.Compiled);
    }

    /// <inheritdoc />
    public override IReadOnlyList<CodeChunk> Chunk(string filePath, string content)
    {
        content = content.Replace("\r\n", "\n");
        var lines = content.Split('\n');
        var mask = ComputeCodeMask(content);
        var offsets = ComputeLineOffsets(content);

        var regions = new List<(int start, int end, string type, string? symbol)>();
        for (int i = 0; i < lines.Length; i++)
        {
            var m = _keywordRegex.Match(lines[i]);
            if (!m.Success) continue;
            int charPos = offsets[i] + m.Index;
            if (charPos >= mask.Length || !mask[charPos]) continue;
            if (regions.Count > 0 && regions[^1].end > i + 1) continue;
            int openIdx = FindOpeningBrace(content, mask, charPos);
            if (openIdx < 0) continue;
            int closeIdx = MatchBrace(content, mask, openIdx);
            if (closeIdx < 0) continue;
            regions.Add((i + 1, LineOfIndex(offsets, closeIdx) + 1, DetectType(m.Value), ExtractSymbol(lines[i], m)));
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

    private static int FindOpeningBrace(string content, bool[] mask, int from)
    {
        for (int i = from; i < content.Length; i++)
        {
            if (!mask[i]) continue;
            if (content[i] == '{') return i;
            if (content[i] == ';') return -1;
        }
        return -1;
    }

    private static int MatchBrace(string content, bool[] mask, int openIdx)
    {
        int depth = 0;
        for (int i = openIdx; i < content.Length; i++)
        {
            if (!mask[i]) continue;
            if (content[i] == '{') depth++;
            else if (content[i] == '}')
            {
                depth--;
                if (depth == 0) return i;
            }
        }
        return -1;
    }

    private static string DetectType(string keyword) => keyword.ToLowerInvariant() switch
    {
        "class" or "struct" or "record" or "object" or "trait" or "mixin" => "Class",
        "interface" or "protocol" => "Interface",
        "enum" => "Enum",
        "namespace" or "mod" or "module" => "Namespace",
        "func" or "function" or "fn" or "fun" or "def" => "Function",
        "type" or "typedef" => "Type",
        "impl" or "extension" => "Impl",
        "message" => "Message",
        "service" => "Service",
        "input" or "schema" => "Schema",
        _ => "Function"
    };

    private static string? ExtractSymbol(string line, Match m)
    {
        var after = line[(m.Index + m.Length)..];
        var sm = Regex.Match(after, @"[A-Za-z_][A-Za-z0-9_]*");
        return sm.Success ? sm.Value : null;
    }
}
