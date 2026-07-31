/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Retrieval.Chunkers
*文件名： IndentChunker
*版本号： V1.0.0.0
*唯一标识：88e58eae-e06d-4f54-b017-c0f7fdf4481e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：缩进切块器
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：缩进切块器
*
*****************************************************************************/
using System.Text.RegularExpressions;

namespace LuBan.AIAgent.Retrieval.Chunkers;

/// <summary>
/// 缩进层级切块器（Python / YAML）
/// </summary>
public class IndentChunker : CodeChunkerBase
{
    private readonly Regex _keywordRegex;

    /// <inheritdoc />
    public override string Language { get; }

    /// <inheritdoc />
    public override IReadOnlyList<string> Extensions { get; }

    /// <summary>
    /// 创建缩进切块器
    /// </summary>
    public IndentChunker(string language, string[] extensions, string keywordPattern)
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

        var regions = new List<(int start, int end, string type, string? symbol)>();
        int i = 0;
        while (i < lines.Length)
        {
            var m = _keywordRegex.Match(lines[i]);
            if (!m.Success) { i++; continue; }
            int indent = IndentOf(lines[i]);
            int j = i + 1;
            while (j < lines.Length)
            {
                var l = lines[j];
                if (!string.IsNullOrWhiteSpace(l) && !l.TrimStart().StartsWith('#') && IndentOf(l) <= indent)
                    break;
                j++;
            }
            regions.Add((i + 1, j, DetectType(lines[i]), ExtractSymbol(lines[i])));
            i = j;
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

    private static int IndentOf(string line)
    {
        int n = 0;
        foreach (var c in line)
        {
            if (c == ' ') n++;
            else if (c == '\t') n += 4;
            else break;
        }
        return n;
    }

    private static string DetectType(string line)
    {
        if (Regex.IsMatch(line, @"^\s*(async\s+def|def)\s+")) return "Function";
        if (Regex.IsMatch(line, @"^\s*class\s+")) return "Class";
        return "Key";
    }

    private static string? ExtractSymbol(string line)
    {
        var m = Regex.Match(line, @"(?:async\s+def|def|class)\s+([A-Za-z_][\w]*)");
        if (m.Success) return m.Groups[1].Value;
        m = Regex.Match(line, @"^\s*([A-Za-z_][\w.\-]*)\s*:");
        return m.Success ? m.Groups[1].Value : null;
    }
}
