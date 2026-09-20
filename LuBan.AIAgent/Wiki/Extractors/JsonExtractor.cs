/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Wiki.Extractors
*文件名： JsonExtractor
*版本号： V1.0.0.0
*唯一标识：c2f0c3d2-1b2c-4e5f-9a01-2b3c4d5e6f74
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/20
*描述：JSON / JSONC / JSONL 提取器
*
*****************************************************************************/
using System.Text;
using System.Text.Json;

namespace LuBan.AIAgent.Wiki.Extractors;

/// <summary>JSON / JSONC / JSONL 提取器。</summary>
public class JsonExtractor : ISourceExtractor
{
    /// <inheritdoc />
    public IReadOnlyCollection<string> Extensions { get; } =
        new[] { ".json", ".jsonc", ".jsonl", ".ndjson" };

    /// <inheritdoc />
    public async Task<ExtractedSource> ExtractAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var raw = await File.ReadAllTextAsync(filePath, cancellationToken);
        var ext = Path.GetExtension(filePath);
        var isLines = ext.Equals(".jsonl", StringComparison.OrdinalIgnoreCase)
                      || ext.Equals(".ndjson", StringComparison.OrdinalIgnoreCase);

        var sb = new StringBuilder();
        if (isLines)
        {
            var i = 0;
            foreach (var line in raw.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                i++;
                sb.Append("### line ").Append(i).Append('\n').Append(Pretty(line.Trim())).Append("\n\n");
            }
        }
        else
        {
            sb.Append(Pretty(raw));
        }

        var text = sb.ToString();
        var truncated = text.Length > TextExtractor.MaxChars;
        if (truncated) text = text[..TextExtractor.MaxChars];
        return new ExtractedSource { FilePath = filePath, Text = raw, Markdown = text, Truncated = truncated };
    }

    private static string Pretty(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            return json;
        }
    }
}