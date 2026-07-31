/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Retrieval.Chunkers
*文件名： SectionChunker
*版本号： V1.0.0.0
*唯一标识：688b9e69-e1c0-4aaa-91a4-b335f41cf669
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：章节切块器
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：章节切块器
*
*****************************************************************************/
using System.Text.RegularExpressions;

namespace LuBan.AIAgent.Retrieval.Chunkers;

/// <summary>
/// 节切块器（INI / TOML / CFG 的 [section] 边界）
/// </summary>
public class SectionChunker : CodeChunkerBase
{
    private static readonly Regex SectionRegex = new(@"^\s*\[([^\]]+)\]", RegexOptions.Compiled);

    /// <inheritdoc />
    public override string Language { get; }

    /// <inheritdoc />
    public override IReadOnlyList<string> Extensions { get; }

    /// <summary>
    /// 创建节切块器
    /// </summary>
    public SectionChunker(string language, string[] extensions)
    {
        Language = language;
        Extensions = extensions;
    }

    /// <inheritdoc />
    public override IReadOnlyList<CodeChunk> Chunk(string filePath, string content)
    {
        content = content.Replace("\r\n", "\n");
        var lines = content.Split('\n');

        var sections = new List<(int line, string name)>();
        for (int i = 0; i < lines.Length; i++)
        {
            var m = SectionRegex.Match(lines[i]);
            if (m.Success) sections.Add((i, m.Groups[1].Value.Trim()));
        }

        if (sections.Count == 0)
            return AssignIndices(WindowAll(filePath, content));

        var chunks = new List<CodeChunk>();
        for (int s = 0; s < sections.Count; s++)
        {
            int start = sections[s].line + 1;
            int end = s + 1 < sections.Count ? sections[s + 1].line : lines.Length;
            if (JoinLines(lines, start, end).Length > MaxChars)
                chunks.AddRange(WindowSplit(filePath, Language, lines, start, end, "Section", sections[s].name));
            else
                chunks.Add(new CodeChunk
                {
                    FilePath = filePath,
                    StartLine = start,
                    EndLine = end,
                    ChunkType = "Section",
                    SymbolName = sections[s].name,
                    Language = Language,
                    Content = JoinLines(lines, start, end)
                });
        }
        return AssignIndices(MergeSmall(chunks));
    }
}
