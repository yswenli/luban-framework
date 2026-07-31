/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Retrieval.Chunkers
*文件名： StatementChunker
*版本号： V1.0.0.0
*唯一标识：cb20c880-97f4-4524-b057-bdd2fdb0a550
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：语句切块器
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：语句切块器
*
*****************************************************************************/
using System.Text.RegularExpressions;

namespace LuBan.AIAgent.Retrieval.Chunkers;

/// <summary>
/// 语句切块器（SQL / Prisma）
/// </summary>
public class StatementChunker : CodeChunkerBase
{
    /// <inheritdoc />
    public override string Language { get; }

    /// <inheritdoc />
    public override IReadOnlyList<string> Extensions { get; }

    /// <summary>
    /// 创建语句切块器
    /// </summary>
    public StatementChunker(string language, string[] extensions)
    {
        Language = language;
        Extensions = extensions;
    }

    /// <inheritdoc />
    public override IReadOnlyList<CodeChunk> Chunk(string filePath, string content)
    {
        content = content.Replace("\r\n", "\n");
        var mask = ComputeSqlMask(content);
        var offsets = ComputeLineOffsets(content);

        var statements = new List<(int start, int end)>();
        int stmtStart = 0;
        for (int i = 0; i < content.Length; i++)
        {
            if (mask[i] && content[i] == ';')
            {
                statements.Add((stmtStart, i + 1));
                stmtStart = i + 1;
            }
        }

        if (statements.Count == 0)
            return AssignIndices(WindowAll(filePath, content));

        var chunks = new List<CodeChunk>();
        foreach (var (s, e) in statements)
        {
            var text = content[s..e].Trim();
            if (text.Length == 0) continue;
            int sLine = LineOfIndex(offsets, s) + 1;
            int eLine = LineOfIndex(offsets, e - 1) + 1;
            if (text.Length > MaxChars)
            {
                var subLines = text.Split('\n');
                var sub = WindowSplit(filePath, Language, subLines, 1, subLines.Length, "Statement", ExtractSymbol(text));
                foreach (var c in sub) { c.StartLine += sLine - 1; c.EndLine += sLine - 1; }
                chunks.AddRange(sub);
            }
            else
            {
                chunks.Add(new CodeChunk
                {
                    FilePath = filePath,
                    StartLine = sLine,
                    EndLine = eLine,
                    ChunkType = "Statement",
                    SymbolName = ExtractSymbol(text),
                    Language = Language,
                    Content = text
                });
            }
        }
        return AssignIndices(MergeSmall(chunks));
    }

    private static bool[] ComputeSqlMask(string content)
    {
        var mask = new bool[content.Length];
        int state = 0;
        for (int i = 0; i < content.Length; i++)
        {
            char c = content[i];
            char next = i + 1 < content.Length ? content[i + 1] : '\0';
            switch (state)
            {
                case 0:
                    mask[i] = true;
                    if (c == '-' && next == '-') { state = 1; i++; }
                    else if (c == '/' && next == '*') { state = 2; i++; }
                    else if (c == '\'') state = 3;
                    else if (c == '"') state = 4;
                    break;
                case 1:
                    if (c == '\n') { state = 0; mask[i] = true; }
                    break;
                case 2:
                    if (c == '*' && next == '/') { state = 0; mask[i + 1] = true; i++; }
                    break;
                case 3:
                    if (c == '\'' && next == '\'') i++;
                    else if (c == '\'') state = 0;
                    break;
                case 4:
                    if (c == '"') state = 0;
                    break;
            }
        }
        return mask;
    }

    private static string? ExtractSymbol(string statement)
    {
        var m = Regex.Match(statement, @"^\s*(?:--[^\n]*\n)*\s*(CREATE|ALTER|DROP)\s+(?:OR\s+REPLACE\s+)?(?:TABLE|VIEW|INDEX|PROCEDURE|FUNCTION|TRIGGER)?\s*([\w.\[\]]+)?", RegexOptions.IgnoreCase);
        if (m.Success && m.Groups[2].Success) return m.Groups[2].Value;
        m = Regex.Match(statement, @"^\s*(\w+)");
        return m.Success ? m.Groups[1].Value : null;
    }
}
