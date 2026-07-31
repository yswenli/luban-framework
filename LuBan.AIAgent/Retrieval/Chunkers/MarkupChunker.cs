/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Retrieval.Chunkers
*文件名： MarkupChunker
*版本号： V1.0.0.0
*唯一标识：2d72fe32-965d-4aa0-be6e-2e1452dfe00f
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：标记语言切块器
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：标记语言切块器
*
*****************************************************************************/
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace LuBan.AIAgent.Retrieval.Chunkers;

/// <summary>
/// 标记语言切块器（HTML / XML / Razor / Vue）
/// </summary>
public class MarkupChunker : CodeChunkerBase
{
    private static readonly Regex ScriptStyleRegex = new(@"<(script|style)[\s\S]*?</\s*\1\s*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex BlockSplitRegex = new(@"</?(?:p|div|section|article|header|footer|main|aside|nav|table|ul|ol|li|tr|td|th|h[1-6]|pre|blockquote|form|figure|figcaption|br|hr)[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex TagRegex = new(@"<[^>]+>", RegexOptions.Compiled);
    private static readonly Regex SpaceRegex = new(@"\s+", RegexOptions.Compiled);

    /// <inheritdoc />
    public override string Language { get; }

    /// <inheritdoc />
    public override IReadOnlyList<string> Extensions { get; }

    /// <summary>
    /// 创建标记语言切块器
    /// </summary>
    public MarkupChunker(string language, string[] extensions)
    {
        Language = language;
        Extensions = extensions;
    }

    /// <inheritdoc />
    public override IReadOnlyList<CodeChunk> Chunk(string filePath, string content)
    {
        var cleaned = ScriptStyleRegex.Replace(content, " ");
        var blocks = BlockSplitRegex.Split(cleaned);
        var texts = new List<string>();
        foreach (var b in blocks)
        {
            var t = TagRegex.Replace(b, " ");
            t = WebUtility.HtmlDecode(t);
            t = SpaceRegex.Replace(t, " ").Trim();
            if (t.Length > 0) texts.Add(t);
        }
        if (texts.Count == 0)
            return AssignIndices(WindowAll(filePath, content));

        var chunks = new List<CodeChunk>();
        var group = new StringBuilder();
        foreach (var t in texts)
        {
            if (t.Length > MaxChars)
            {
                FlushGroup();
                for (int i = 0; i < t.Length; i += MaxChars - 200)
                    chunks.Add(new CodeChunk { FilePath = filePath, ChunkType = "Block", Language = Language, Content = t.Substring(i, Math.Min(MaxChars, t.Length - i)), StartLine = 0, EndLine = 0 });
                continue;
            }
            if (group.Length + t.Length > TargetChars && group.Length > 0) FlushGroup();
            if (group.Length > 0) group.Append('\n');
            group.Append(t);
        }
        FlushGroup();
        return AssignIndices(chunks);

        void FlushGroup()
        {
            if (group.Length == 0) return;
            chunks.Add(new CodeChunk { FilePath = filePath, ChunkType = "Block", Language = Language, Content = group.ToString(), StartLine = 0, EndLine = 0 });
            group.Clear();
        }
    }
}
