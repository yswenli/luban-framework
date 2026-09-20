/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Wiki.Extractors
*文件名： DelimitedTextExtractor
*版本号： V1.0.0.0
*唯一标识：d2f0c3d2-1b2c-4e5f-9a01-2b3c4d5e6f75
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/20
*描述：CSV / TSV 提取器：转 markdown 表格
*
*****************************************************************************/
using System.Text;

namespace LuBan.AIAgent.Wiki.Extractors;

/// <summary>CSV / TSV 提取器：转 markdown 表格。</summary>
public class DelimitedTextExtractor : ISourceExtractor
{
    /// <inheritdoc />
    public IReadOnlyCollection<string> Extensions { get; } = new[] { ".csv", ".tsv" };

    /// <inheritdoc />
    public async Task<ExtractedSource> ExtractAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var raw = await File.ReadAllTextAsync(filePath, cancellationToken);
        var delimiter = Path.GetExtension(filePath).Equals(".tsv", StringComparison.OrdinalIgnoreCase) ? '\t' : ',';
        var lines = raw.Replace("\r\n", "\n").Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var sb = new StringBuilder();
        var rows = 0;
        foreach (var line in lines)
        {
            var cells = line.Split(delimiter).Select(c => c.Trim()).ToArray();
            sb.Append("| ").Append(string.Join(" | ", cells)).Append(" |\n");
            if (rows == 0) sb.Append("| ").Append(string.Join(" | ", cells.Select(_ => "---"))).Append(" |\n");
            rows++;
            if (rows >= 2000) break;
        }

        var markdown = sb.ToString();
        return new ExtractedSource { FilePath = filePath, Text = raw, Markdown = markdown };
    }
}
