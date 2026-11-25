/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Sessions
*文件名： FragmentedTextNormalizer
*描述：收拢流式输出中被空行切碎的正文段落
*
*****************************************************************************/

using System.Text;
using System.Text.RegularExpressions;

namespace LuBan.AIAgent.Sessions;

/// <summary>
/// 收拢流式输出中被「空行」切碎的正文段落。
/// </summary>
/// <remarks>
/// 上游按帧下发时可能在句子中间补出空行（形如「第二\n\n个文件已读」），
/// 落库后前端按 Markdown 渲染，一句正文会被摊成十几个段落。
/// 本类只在整段文本明显碎片化时出手，且不跨越标题、列表、表格、引用、代码块等块级结构；
/// 判断为正常文档时原样返回（返回与入参是同一实例，调用方可用引用比较判断是否改动过）。
/// </remarks>
internal static class FragmentedTextNormalizer
{
    /// <summary>判定为「被切碎」所需的最少段落数。</summary>
    private const int MinParts = 6;

    /// <summary>不长于该长度的段落按碎片计。</summary>
    private const int FragmentMaxLength = 8;

    /// <summary>判定为「被切碎」所需的最少碎片段落数。</summary>
    private const int MinFragments = 2;

    /// <summary>句末标点：其后出现的空行是真实段落边界。</summary>
    private const string SentenceTerminators = "。！？；：…!?;:）)」』】》”’\"";

    /// <summary>块级行的首字符。</summary>
    private const string BlockStarts = "#>-|+*`~<";

    private static readonly Regex BlankLineRun = new(@"(?:\r?\n){2,}", RegexOptions.Compiled);

    /// <summary>
    /// 收拢碎片空行。文本未被切碎（或本就是单行）时原样返回。
    /// </summary>
    /// <param name="text">待处理的正文。</param>
    /// <param name="force">true 表示跳过碎片化判定直接收拢，用于调用方已在整段文本上判定过的场合。</param>
    internal static string Normalize(string? text, bool force = false)
    {
        if (string.IsNullOrEmpty(text) || text.IndexOf('\n') < 0)
            return text ?? string.Empty;

        if (!force && !LooksFragmented(text))
            return text;

        return Collapse(text);
    }

    /// <summary>
    /// 判断文本是否被流式空行切碎：段落数足够多，且其中至少有两个「短正文段落」。
    /// 代码块内容不参与判定。
    /// </summary>
    internal static bool LooksFragmented(string? text)
    {
        if (string.IsNullOrEmpty(text) || text.IndexOf('\n') < 0)
            return false;

        var parts = SplitParts(StripFencedBlocks(text));
        if (parts.Count < MinParts)
            return false;

        var fragments = 0;
        foreach (var part in parts)
        {
            if (part.Length > FragmentMaxLength || IsBlockPart(part))
                continue;
            if (++fragments >= MinFragments)
                return true;
        }

        return false;
    }

    /// <summary>逐处判断空行是否该收拢；代码块内的空行原样保留。</summary>
    private static string Collapse(string text)
    {
        var fences = FindFencedBlocks(text);
        var inInlineCode = BuildInlineCodeMap(text, fences);
        var sb = new StringBuilder(text.Length);
        var last = 0;

        foreach (Match match in BlankLineRun.Matches(text))
        {
            var start = match.Index;
            var end = start + match.Length;
            sb.Append(text, last, start - last);
            last = end;

            if (InsideAny(fences, start))
            {
                sb.Append(match.Value);
                continue;
            }

            var prev = start > 0 ? text[start - 1] : '\0';
            var nextIndex = end;
            while (nextIndex < text.Length && (text[nextIndex] == ' ' || text[nextIndex] == '\t'))
                nextIndex++;
            var next = nextIndex < text.Length ? text[nextIndex] : '\0';

            // 上一行若是表格行、标题、列表项等块级结构，空行同样必须保留
            var prevLineStart = FirstNonSpaceOfLine(text, start - 1);
            var keep = prev == '\0' || next == '\0'
                       || SentenceTerminators.IndexOf(prev) >= 0
                       || IsBlockStart(text, nextIndex)
                       || IsBlockStart(text, prevLineStart);
            if (keep)
            {
                sb.Append(match.Value.Contains('\r') ? "\r\n\r\n" : "\n\n");
                continue;
            }

            // 无空格的情况下直接拼接；英文单词相接时补一个空格（行内代码里不补，避免破坏路径与标识符）
            if (start > 0 && !inInlineCode[start - 1]
                && char.IsAsciiLetterOrDigit(prev) && char.IsAsciiLetterOrDigit(next))
                sb.Append(' ');
        }

        sb.Append(text, last, text.Length - last);
        return sb.ToString();
    }

    /// <summary>去掉围栏代码块后的文本，用于碎片化判定（避免把代码里的空行算成碎片）。</summary>
    private static string StripFencedBlocks(string text)
    {
        var fences = FindFencedBlocks(text);
        if (fences.Count == 0)
            return text;

        var sb = new StringBuilder(text.Length);
        var last = 0;
        foreach (var (start, end) in fences)
        {
            sb.Append(text, last, start - last);
            sb.Append('\n');
            last = end;
        }
        sb.Append(text, last, text.Length - last);
        return sb.ToString();
    }

    /// <summary>按空行切段，去掉首尾空白并丢弃空段。</summary>
    private static List<string> SplitParts(string text)
    {
        var parts = new List<string>();
        var last = 0;
        foreach (Match match in BlankLineRun.Matches(text))
        {
            AddPart(parts, text, last, match.Index);
            last = match.Index + match.Length;
        }
        AddPart(parts, text, last, text.Length);
        return parts;
    }

    private static void AddPart(List<string> parts, string text, int start, int end)
    {
        var s = start;
        var e = end;
        while (s < e && char.IsWhiteSpace(text[s])) s++;
        while (e > s && char.IsWhiteSpace(text[e - 1])) e--;
        if (e > s) parts.Add(text[s..e]);
    }

    /// <summary>判断段落本身是不是块级 Markdown 结构（标题 / 列表 / 表格 / 分隔线 / 代码围栏）。</summary>
    private static bool IsBlockPart(string part)
    {
        if (part.Length == 0)
            return true;

        if (BlockStarts.IndexOf(part[0]) >= 0)
            return true;

        if (char.IsAsciiDigit(part[0]))
        {
            var i = 0;
            while (i < part.Length && char.IsAsciiDigit(part[i])) i++;
            if (i < part.Length && (part[i] == '.' || part[i] == ')')
                && (i + 1 >= part.Length || part[i + 1] == ' '))
                return true;
        }

        foreach (var ch in part)
        {
            if (ch is not ('-' or '*' or '_' or '=' or '|' or '#' or '+' or '~' or '.' or ' '))
                return false;
        }
        return true;
    }

    /// <summary>判断指定位置的字符是否开启一个块级行（行首已跳过缩进）。</summary>
    private static bool IsBlockStart(string text, int index)
    {
        if (index < 0 || index >= text.Length)
            return true;

        var c = text[index];
        if (c == '*')
            return index + 1 < text.Length && text[index + 1] == ' ';
        if (c == '-')
            return index + 1 < text.Length && (text[index + 1] == ' ' || text[index + 1] == '-');
        if (c == '`')
            return index + 2 < text.Length && text[index + 1] == '`' && text[index + 2] == '`';
        if (BlockStarts.IndexOf(c) >= 0)
            return true;

        if (char.IsAsciiDigit(c))
        {
            var i = index;
            while (i < text.Length && char.IsAsciiDigit(text[i])) i++;
            return i < text.Length && text[i] == '.' && (i + 1 >= text.Length || text[i + 1] == ' ');
        }

        return false;
    }

    /// <summary>返回 <paramref name="pos"/> 所在行第一个非空白字符的下标；空行返回 -1。</summary>
    private static int FirstNonSpaceOfLine(string text, int pos)
    {
        if (pos < 0 || text.Length == 0)
            return -1;

        var start = text.LastIndexOf('\n', Math.Min(pos, text.Length - 1)) + 1;
        while (start < text.Length && (text[start] == ' ' || text[start] == '\t')) start++;
        return start < text.Length ? start : -1;
    }

    /// <summary>找出所有围栏代码块（``` 或 ~~~）的区间；未闭合的围栏保护到文末。</summary>
    private static List<(int Start, int End)> FindFencedBlocks(string text)
    {
        var fences = new List<(int Start, int End)>();
        var lineStart = 0;
        var openStart = -1;
        var openChar = '\0';

        while (lineStart <= text.Length)
        {
            var lineEnd = text.IndexOf('\n', lineStart);
            if (lineEnd < 0) lineEnd = text.Length;

            var p = lineStart;
            while (p < lineEnd && (text[p] == ' ' || text[p] == '\t')) p++;

            if (p + 2 < text.Length && (text[p] == '`' || text[p] == '~')
                && text[p + 1] == text[p] && text[p + 2] == text[p])
            {
                if (openStart < 0)
                {
                    openStart = lineStart;
                    openChar = text[p];
                }
                else if (text[p] == openChar)
                {
                    fences.Add((openStart, lineEnd));
                    openStart = -1;
                }
            }

            if (lineEnd >= text.Length) break;
            lineStart = lineEnd + 1;
        }

        if (openStart >= 0) fences.Add((openStart, text.Length));
        return fences;
    }

    /// <summary>标记每个字符是否处于行内代码（`...`）之内；围栏代码块整体跳过。</summary>
    private static bool[] BuildInlineCodeMap(string text, List<(int Start, int End)> fences)
    {
        var map = new bool[text.Length];
        var inside = false;
        var i = 0;

        while (i < text.Length)
        {
            var fence = fences.Find(f => i >= f.Start && i < f.End);
            if (fence.Start < fence.End)
            {
                i = fence.End;
                inside = false;
                continue;
            }

            if (text[i] == '`')
            {
                var run = 1;
                while (i + run < text.Length && text[i + run] == '`') run++;
                if (run >= 3)
                {
                    i += run;
                    continue;
                }
                inside = !inside;
                map[i] = inside;
                i += run;
                continue;
            }

            map[i] = inside;
            i++;
        }

        return map;
    }

    private static bool InsideAny(List<(int Start, int End)> fences, int index)
    {
        foreach (var (start, end) in fences)
        {
            if (index >= start && index < end)
                return true;
        }
        return false;
    }
}