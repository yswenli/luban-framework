/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Wiki.Extractors
*文件名： ExcelExtractor
*版本号： V1.0.0.0
*唯一标识：f1a2b3c4-d5e6-4789-9abc-def012345678
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/20
*描述：Excel 提取器，每个 sheet 转 markdown 表格
*
*****************************************************************************/
using System.Text;

using MiniExcelLibs;

namespace LuBan.AIAgent.Wiki.Extractors;

/// <summary>
/// Excel 提取器：每个 sheet 转 markdown 表格。
/// `.xlsx` 为正式支持；`.xls`（BIFF）MiniExcel 不支持，会抛异常由上层告警跳过（spec Q16）。
/// </summary>
public class ExcelExtractor : ISourceExtractor
{
    /// <inheritdoc />
    public IReadOnlyCollection<string> Extensions { get; } = new[] { ".xlsx", ".xls" };

    /// <inheritdoc />
    public Task<ExtractedSource> ExtractAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();
        foreach (var sheet in MiniExcel.GetSheetNames(filePath))
        {
            sb.Append("## ").Append(sheet).Append('\n');
            var rows = MiniExcel.Query(filePath, useHeaderRow: true, sheetName: sheet).Cast<IDictionary<string, object>>();
            var headers = rows.FirstOrDefault()?.Keys.ToArray() ?? Array.Empty<string>();
            if (headers.Length > 0)
            {
                sb.Append("| ").Append(string.Join(" | ", headers)).Append(" |\n");
                sb.Append("| ").Append(string.Join(" | ", headers.Select(_ => "---"))).Append(" |\n");
            }
            foreach (var row in rows)
            {
                sb.Append("| ").Append(string.Join(" | ", headers.Select(h => row.TryGetValue(h, out var v) ? v?.ToString() ?? "" : ""))).Append(" |\n");
                cancellationToken.ThrowIfCancellationRequested();
            }
            sb.Append('\n');
        }

        var markdown = sb.ToString();
        var truncated = markdown.Length > TextExtractor.MaxChars;
        if (truncated) markdown = markdown[..TextExtractor.MaxChars];
        return Task.FromResult(new ExtractedSource { FilePath = filePath, Text = markdown, Markdown = markdown, Truncated = truncated });
    }
}
