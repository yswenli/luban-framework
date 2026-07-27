namespace LuBan.AIAgent.Retrieval.Chunkers;

/// <summary>
/// 切块器基类：窗口切分、合并、代码掩码等公共能力
/// </summary>
public abstract class CodeChunkerBase : ICodeChunker
{
    /// <summary>目标块字符数</summary>
    public const int TargetChars = 1200;
    /// <summary>块字符数硬上限</summary>
    public const int MaxChars = 3000;
    /// <summary>块字符数下限（低于则尝试与相邻同类型块合并）</summary>
    public const int MinChars = 150;
    /// <summary>相邻窗口重叠行数</summary>
    public const int OverlapLines = 3;

    /// <inheritdoc />
    public abstract string Language { get; }

    /// <inheritdoc />
    public abstract IReadOnlyList<string> Extensions { get; }

    /// <inheritdoc />
    public abstract IReadOnlyList<CodeChunk> Chunk(string filePath, string content);

    /// <summary>
    /// 规范化换行并按行切分
    /// </summary>
    protected static string[] SplitLines(string content)
        => content.Replace("\r\n", "\n").Split('\n');

    /// <summary>
    /// 取行区间文本（1 起始，含两端）
    /// </summary>
    protected static string JoinLines(string[] lines, int startLine, int endLine)
        => string.Join('\n', lines.Skip(startLine - 1).Take(endLine - startLine + 1));

    /// <summary>
    /// 对行区间滑动窗口切分，每块不超过 MaxChars，相邻重叠 OverlapLines 行
    /// </summary>
    protected List<CodeChunk> WindowSplit(string filePath, string language, string[] lines,
        int startLine, int endLine, string chunkType, string? symbol)
    {
        var chunks = new List<CodeChunk>();
        int i = startLine;
        while (i <= endLine)
        {
            int j = i;
            int len = 0;
            while (j <= endLine)
            {
                int lineLen = lines[j - 1].Length + 1;
                if (len + lineLen > MaxChars && j > i) break;
                len += lineLen;
                j++;
                if (len >= TargetChars) break;
            }
            chunks.Add(new CodeChunk
            {
                FilePath = filePath,
                StartLine = i,
                EndLine = j,
                ChunkType = chunkType,
                SymbolName = symbol,
                Language = language,
                Content = JoinLines(lines, i, j)
            });
            if (j >= endLine) break;
            i = Math.Max(i + 1, j - OverlapLines + 1);
        }
        return chunks;
    }

    /// <summary>
    /// 全文滑窗
    /// </summary>
    protected List<CodeChunk> WindowAll(string filePath, string content)
    {
        var lines = SplitLines(content);
        return WindowSplit(filePath, Language, lines, 1, lines.Length, "Window", null);
    }

    /// <summary>
    /// 过小的相邻同类型 chunk 合并
    /// </summary>
    protected static List<CodeChunk> MergeSmall(List<CodeChunk> chunks)
    {
        for (int i = 0; i < chunks.Count - 1; i++)
        {
            var cur = chunks[i];
            if (cur.Content.Length >= MinChars) continue;
            var next = chunks[i + 1];
            if (cur.ChunkType != next.ChunkType) continue;
            if (cur.Content.Length + next.Content.Length > MaxChars) continue;
            next.Content = cur.Content + "\n" + next.Content;
            next.StartLine = cur.StartLine;
            next.SymbolName ??= cur.SymbolName;
            chunks.RemoveAt(i);
            i--;
        }
        return chunks;
    }

    /// <summary>
    /// 赋切块序号
    /// </summary>
    protected static List<CodeChunk> AssignIndices(List<CodeChunk> chunks)
    {
        for (int i = 0; i < chunks.Count; i++) chunks[i].ChunkIndex = i;
        return chunks;
    }

    /// <summary>
    /// 代码掩码：true=代码，false=注释或字符串内容（泛 C 系：// /* */ "..." '...' `...` @"..."）
    /// </summary>
    protected static bool[] ComputeCodeMask(string content)
    {
        var mask = new bool[content.Length];
        int state = 0; // 0=code 1=lineComment 2=blockComment 3=dq 4=sq 5=backtick 6=verbatim
        for (int i = 0; i < content.Length; i++)
        {
            char c = content[i];
            char next = i + 1 < content.Length ? content[i + 1] : '\0';
            switch (state)
            {
                case 0:
                    mask[i] = true;
                    if (c == '/' && next == '/') { state = 1; i++; }
                    else if (c == '/' && next == '*') { state = 2; i++; }
                    else if (c == '@' && next == '"') { state = 6; i++; }
                    else if (c == '"') state = 3;
                    else if (c == '\'') state = 4;
                    else if (c == '`') state = 5;
                    break;
                case 1:
                    if (c == '\n') { state = 0; mask[i] = true; }
                    break;
                case 2:
                    if (c == '*' && next == '/') { state = 0; mask[i + 1] = true; i++; }
                    break;
                case 3:
                    if (c == '\\') i++;
                    else if (c == '"') state = 0;
                    break;
                case 4:
                    if (c == '\\') i++;
                    else if (c == '\'') state = 0;
                    break;
                case 5:
                    if (c == '\\') i++;
                    else if (c == '`') state = 0;
                    break;
                case 6:
                    if (c == '"' && next == '"') i++;
                    else if (c == '"') state = 0;
                    break;
            }
        }
        return mask;
    }

    /// <summary>
    /// 每行起始字符偏移
    /// </summary>
    protected static int[] ComputeLineOffsets(string content)
    {
        var offsets = new List<int> { 0 };
        for (int i = 0; i < content.Length; i++)
            if (content[i] == '\n') offsets.Add(i + 1);
        return offsets.ToArray();
    }

    /// <summary>
    /// 字符偏移所在行（0 起始）
    /// </summary>
    protected static int LineOfIndex(int[] lineOffsets, int charIndex)
    {
        int line = Array.BinarySearch(lineOffsets, charIndex);
        if (line < 0) line = ~line - 1;
        return line;
    }
}
