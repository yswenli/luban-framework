/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Retrieval.Chunkers
*文件名： JsonChunker
*版本号： V1.0.0.0
*唯一标识：107cba1a-c992-4144-9d38-bc52d6a396b3
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：JSON 切块器
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：JSON 切块器
*
*****************************************************************************/
using System.Text;
using System.Text.Json;

namespace LuBan.AIAgent.Retrieval.Chunkers;

/// <summary>
/// JSON / ipynb 切块器
/// </summary>
public class JsonChunker : CodeChunkerBase
{
    /// <inheritdoc />
    public override string Language { get; }

    /// <inheritdoc />
    public override IReadOnlyList<string> Extensions { get; }

    /// <summary>
    /// 创建 JSON 切块器
    /// </summary>
    public JsonChunker(string language, string[] extensions)
    {
        Language = language;
        Extensions = extensions;
    }

    /// <inheritdoc />
    public override IReadOnlyList<CodeChunk> Chunk(string filePath, string content)
    {
        content = content.Replace("\r\n", "\n");
        JsonDocument doc;
        try { doc = JsonDocument.Parse(content); }
        catch { return AssignIndices(WindowAll(filePath, content)); }

        using (doc)
        {
            var root = doc.RootElement;
            if (filePath.EndsWith(".ipynb", StringComparison.OrdinalIgnoreCase)
                && root.ValueKind == JsonValueKind.Object
                && root.TryGetProperty("cells", out var cells)
                && cells.ValueKind == JsonValueKind.Array)
            {
                return AssignIndices(ChunkNotebook(filePath, cells));
            }
            if (root.ValueKind == JsonValueKind.Object)
                return AssignIndices(ChunkObject(filePath, root));
            if (root.ValueKind == JsonValueKind.Array)
                return AssignIndices(ChunkArray(filePath, root));
            return AssignIndices(WindowAll(filePath, content));
        }
    }

    private List<CodeChunk> ChunkNotebook(string filePath, JsonElement cells)
    {
        var chunks = new List<CodeChunk>();
        foreach (var cell in cells.EnumerateArray())
        {
            var type = cell.TryGetProperty("cell_type", out var ct) ? ct.GetString() ?? "code" : "code";
            var sb = new StringBuilder();
            if (cell.TryGetProperty("source", out var src) && src.ValueKind == JsonValueKind.Array)
                foreach (var line in src.EnumerateArray()) sb.Append(line.GetString());
            var text = sb.ToString();
            if (string.IsNullOrWhiteSpace(text)) continue;
            var chunkType = type == "markdown" ? "Markdown" : "Code";
            if (text.Length > MaxChars)
            {
                var subLines = text.Replace("\r\n", "\n").Split('\n');
                chunks.AddRange(WindowSplit(filePath, Language, subLines, 1, subLines.Length, chunkType, null));
            }
            else
            {
                chunks.Add(new CodeChunk { FilePath = filePath, ChunkType = chunkType, Language = Language, Content = text, StartLine = 0, EndLine = 0 });
            }
        }
        return chunks.Count > 0 ? chunks : WindowAll(filePath, "");
    }

    private List<CodeChunk> ChunkObject(string filePath, JsonElement root)
    {
        var chunks = new List<CodeChunk>();
        var group = new StringBuilder();
        string? groupKey = null;

        foreach (var prop in root.EnumerateObject())
        {
            var text = "\"" + prop.Name + "\":" + prop.Value.GetRawText();
            if (text.Length > MaxChars)
            {
                FlushGroup();
                var subLines = text.Split('\n');
                chunks.AddRange(WindowSplit(filePath, Language, subLines, 1, subLines.Length, "Object", prop.Name));
                continue;
            }
            if (group.Length + text.Length > TargetChars && group.Length > 0) FlushGroup();
            groupKey ??= prop.Name;
            if (group.Length > 0) group.Append(",\n");
            group.Append(text);
        }
        FlushGroup();

        void FlushGroup()
        {
            if (group.Length == 0) return;
            chunks.Add(new CodeChunk { FilePath = filePath, ChunkType = "Object", SymbolName = groupKey, Language = Language, Content = group.ToString(), StartLine = 0, EndLine = 0 });
            group.Clear();
            groupKey = null;
        }
        return chunks;
    }

    private List<CodeChunk> ChunkArray(string filePath, JsonElement root)
    {
        var chunks = new List<CodeChunk>();
        var group = new StringBuilder();
        foreach (var item in root.EnumerateArray())
        {
            var text = item.GetRawText();
            if (group.Length + text.Length > TargetChars && group.Length > 0) Flush();
            if (group.Length > 0) group.Append(",\n");
            group.Append(text);
        }
        Flush();
        void Flush()
        {
            if (group.Length == 0) return;
            chunks.Add(new CodeChunk { FilePath = filePath, ChunkType = "Array", Language = Language, Content = group.ToString(), StartLine = 0, EndLine = 0 });
            group.Clear();
        }
        return chunks.Count > 0 ? chunks : WindowAll(filePath, "[]");
    }
}
