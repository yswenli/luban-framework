/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Retrieval.Chunkers
*文件名： HeaderChunker
*版本号： V1.0.0.0
*唯一标识：48ac2e36-3e17-4d75-a155-133d1b08875f
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：标题切块器
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：标题切块器
*
*****************************************************************************/
using System.Text.RegularExpressions;

namespace LuBan.AIAgent.Retrieval.Chunkers;

/// <summary>
/// 标题分节切块器（Markdown / LaTeX）
/// </summary>
public class HeaderChunker : CodeChunkerBase
{
    private readonly Regex _headingRegex;
    private readonly int _captionGroup;

    /// <inheritdoc />
    public override string Language { get; }

    /// <inheritdoc />
    public override IReadOnlyList<string> Extensions { get; }

    /// <summary>
    /// 创建标题分节切块器
    /// </summary>
    public HeaderChunker(string language, string[] extensions, string headingPattern, int markerGroup, int captionGroup)
    {
        Language = language;
        Extensions = extensions;
        _headingRegex = new Regex(headingPattern, RegexOptions.Compiled);
        _captionGroup = captionGroup;
    }

    /// <inheritdoc />
    public override IReadOnlyList<CodeChunk> Chunk(string filePath, string content)
    {
        content = content.Replace("\r\n", "\n");
        var lines = content.Split('\n');

        var headings = new List<(int line, string caption)>();
        for (int i = 0; i < lines.Length; i++)
        {
            var m = _headingRegex.Match(lines[i]);
            if (m.Success)
                headings.Add((i, m.Groups[_captionGroup].Value.Trim()));
        }

        if (headings.Count == 0)
            return AssignIndices(WindowAll(filePath, content));

        var chunks = new List<CodeChunk>();
        if (headings[0].line > 0 && JoinLines(lines, 1, headings[0].line).Trim().Length > 0)
            chunks.AddRange(WindowSplit(filePath, Language, lines, 1, headings[0].line, "Window", null));

        for (int h = 0; h < headings.Count; h++)
        {
            int start = headings[h].line + 1;
            int end = h + 1 < headings.Count ? headings[h + 1].line : lines.Length;
            if (JoinLines(lines, start, end).Length > MaxChars)
                chunks.AddRange(WindowSplit(filePath, Language, lines, start, end, "Section", headings[h].caption));
            else
                chunks.Add(new CodeChunk
                {
                    FilePath = filePath,
                    StartLine = start,
                    EndLine = end,
                    ChunkType = "Section",
                    SymbolName = headings[h].caption,
                    Language = Language,
                    Content = JoinLines(lines, start, end)
                });
        }
        return AssignIndices(MergeSmall(chunks));
    }
}
