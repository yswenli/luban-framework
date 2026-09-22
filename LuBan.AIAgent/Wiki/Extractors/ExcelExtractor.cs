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
/// 仅 `.xlsx` 为正式支持；`.xls`（BIFF）MiniExcel 不支持，故不再注册（spec E1）。
/// 表头按列序号读取（`useHeaderRow:false`，键为 A/B/C…），首行值作为表头展示名，避免空表头/丢列（spec E2）。
/// </summary>
public class ExcelExtractor : ISourceExtractor
{
    /// <summary>
    /// 索引场景的单文件提取字符上限：远高于 <see cref="TextExtractor.MaxChars"/>（摄入场景受上下文长度约束），
    /// 避免大表在索引时被静默截断。仍设上限以防单文件产生巨量分块。
    /// </summary>
    public const int IndexingMaxChars = 2_000_000;

    /// <inheritdoc />
    public IReadOnlyCollection<string> Extensions { get; } = new[] { ".xlsx" };

    /// <inheritdoc />
    public Task<ExtractedSource> ExtractAsync(string filePath, CancellationToken cancellationToken = default)
        => ExtractAsync(filePath, TextExtractor.MaxChars, cancellationToken);

    /// <summary>
    /// 按指定字符上限提取（索引场景传 <see cref="IndexingMaxChars"/>）。
    /// 超出上限时截断并在 <see cref="ExtractedSource.Truncated"/> 标记。
    /// </summary>
    public Task<ExtractedSource> ExtractAsync(string filePath, int maxChars, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();
        var truncated = false;
        foreach (var sheet in MiniExcel.GetSheetNames(filePath))
        {
            if (truncated) break;
            sb.Append("## ").Append(sheet).Append('\n');

            var rows = MiniExcel.Query(filePath, useHeaderRow: false, sheetName: sheet)
                .Cast<IDictionary<string, object>>()
                .ToList();

            var columns = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var row in rows)
                foreach (var key in row.Keys)
                    if (seen.Add(key)) columns.Add(key);

            if (columns.Count == 0)
            {
                sb.Append('\n');
                continue;
            }

            var headers = BuildHeaders(columns, rows[0]);
            sb.Append("| ").Append(string.Join(" | ", headers)).Append(" |\n");
            sb.Append("| ").Append(string.Join(" | ", headers.Select(_ => "---"))).Append(" |\n");

            for (var r = 1; r < rows.Count; r++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                sb.Append("| ")
                  .Append(string.Join(" | ", columns.Select(c => Escape(rows[r].TryGetValue(c, out var v) ? v?.ToString() : null))))
                  .Append(" |\n");
                if (sb.Length > maxChars) { truncated = true; break; }
            }
            sb.Append('\n');
        }

        var markdown = sb.ToString();
        if (markdown.Length > maxChars)
        {
            markdown = markdown[..maxChars];
            truncated = true;
        }
        return Task.FromResult(new ExtractedSource { FilePath = filePath, Text = markdown, Markdown = markdown, Truncated = truncated });
    }

    private static string[] BuildHeaders(IReadOnlyList<string> columns, IDictionary<string, object> firstRow)
    {
        var used = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var headers = new string[columns.Count];
        for (var i = 0; i < columns.Count; i++)
        {
            var raw = firstRow.TryGetValue(columns[i], out var v) ? v?.ToString()?.Trim() : null;
            var name = string.IsNullOrWhiteSpace(raw) ? $"列{columns[i]}" : raw!;
            if (used.TryGetValue(name, out var count))
            {
                used[name] = count + 1;
                name = $"{name}({count + 1})";
            }
            else used[name] = 1;
            headers[i] = Escape(name);
        }
        return headers;
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Replace("|", "\\|").Replace("\r\n", "<br>").Replace("\n", "<br>").Replace("\r", "<br>");
    }
}
