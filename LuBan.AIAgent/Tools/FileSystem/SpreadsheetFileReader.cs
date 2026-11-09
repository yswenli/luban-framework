/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__ _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Tools.FileSystem
*文件名： SpreadsheetFileReader
*版本号： V1.0.0.0
*唯一标识：d1e5b7c2-3f48-4a91-8b6d-7c8e9f0a1b23
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/11/6
*描述：表格文件（xlsx/csv/tsv）转 markdown 表格，支持工作表选择与行分页
*
*****************************************************************************/

using MiniExcelLibs;

namespace LuBan.AIAgent.Tools.FileSystem;

/// <summary>
/// 表格文件读取：把工作簿/CSV 转成 markdown 表格，供模型直接理解数据。
/// <para>
/// 支持 <c>.xlsx</c>（MiniExcel）与 <c>.csv</c>/<c>.tsv</c>（内置分隔符解析，含引号转义）。
/// 旧版 <c>.xls</c> 为二进制 BIFF 格式，不做解析，返回可操作提示。
/// </para>
/// </summary>
internal static class SpreadsheetFileReader
{
    /// <summary>默认返回的数据行数（不含表头）。</summary>
    internal const int DefaultRowLimit = 200;

    /// <summary>单次最多返回的数据行数。</summary>
    internal const int MaxRowLimit = 2000;

    /// <summary>单次返回的最大字符数，防止宽表撑爆上下文。</summary>
    internal const int MaxChars = 128 * 1024;

    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".xlsx", ".csv", ".tsv"
    };

    /// <summary>判断扩展名是否可直接解析。</summary>
    internal static bool IsSupported(string path) => SupportedExtensions.Contains(Path.GetExtension(path));

    /// <summary>判断是否为不支持的旧版 Excel 二进制格式。</summary>
    internal static bool IsLegacyXls(string path)
        => string.Equals(Path.GetExtension(path), ".xls", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// 读取表格文件并格式化为 markdown 表格。
    /// </summary>
    /// <param name="path">表格文件路径。</param>
    /// <param name="sheetName">工作表名（仅 xlsx；为空时取第一个工作表）。</param>
    /// <param name="offset">起始数据行（从 1 开始，不含表头）。</param>
    /// <param name="limit">本次最多返回的数据行数。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    internal static async Task<ToolResult<string>> ReadAsync(
        string path, string? sheetName, int offset, int limit, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (IsLegacyXls(path))
        {
            return ToolResult.Fail<string>(
                $"{path} 是旧版 Excel 二进制格式（.xls/BIFF），无法直接解析。"
                + "请先另存为 .xlsx 或 .csv，再调用 ReadSpreadsheet。");
        }

        if (!IsSupported(path))
        {
            return ToolResult.Fail<string>(
                $"{path} 不是支持的表格格式。ReadSpreadsheet 支持 .xlsx / .csv / .tsv；"
                + "文本文件请用 ReadFile，PDF/Word 等请说明需求后改用对应工具。");
        }

        if (!File.Exists(path))
            return ToolResult.Fail<string>($"未找到文件: {path}。请先用 ListDirectory 核对路径。");

        offset = offset < 1 ? 1 : offset;
        limit = limit <= 0 ? DefaultRowLimit : Math.Min(limit, MaxRowLimit);

        try
        {
            string caption;
            List<IReadOnlyList<string>> allRows;

            if (IsDelimited(path))
            {
                var (text, _) = await TextFileCodec.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
                allRows = ParseDelimited(text, Path.GetExtension(path).Equals(".tsv", StringComparison.OrdinalIgnoreCase) ? '\t' : ',');
                caption = $"## 表格: {Path.GetFileName(path)}";
            }
            else
            {
                var sheetNames = MiniExcel.GetSheetNames(path).ToList();
                if (sheetNames.Count == 0)
                    return ToolResult.Fail<string>($"{path} 中没有工作表。");

                var sheet = string.IsNullOrWhiteSpace(sheetName) ? sheetNames[0] : sheetName.Trim();
                var matched = sheetNames.FirstOrDefault(s => string.Equals(s, sheet, StringComparison.Ordinal));
                if (matched is null)
                {
                    return ToolResult.Fail<string>(
                        $"未找到工作表 \"{sheet}\"。可用工作表: {string.Join("、", sheetNames)}。"
                        + "请在 sheetName 参数中传入其中之一。");
                }

                allRows = ReadXlsxRows(path, matched);
                var others = sheetNames.Where(s => !string.Equals(s, matched, StringComparison.Ordinal)).ToList();
                caption = $"## 表格: {Path.GetFileName(path)} / 工作表: {matched}"
                    + (others.Count > 0 ? $"（其它工作表: {string.Join("、", others)}）" : string.Empty);
            }

            return ToolResult.Ok<string>(BuildMarkdown(caption, allRows, offset, limit));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger.Error("表格读取异常", ex, path);
            return ToolResult.Fail<string>(
                $"读取表格失败: {path}。文件可能损坏、被占用或格式与扩展名不符。详细信息: {ex.Message}");
        }
    }

    private static bool IsDelimited(string path)
        => Path.GetExtension(path).Equals(".csv", StringComparison.OrdinalIgnoreCase)
           || Path.GetExtension(path).Equals(".tsv", StringComparison.OrdinalIgnoreCase);

    private static List<IReadOnlyList<string>> ReadXlsxRows(string path, string sheetName)
    {
        var rows = MiniExcel.Query(path, useHeaderRow: false, sheetName: sheetName)
            .Cast<IDictionary<string, object>>()
            .ToList();

        // 列顺序按首次出现顺序固定，避免不同行键集合不一致导致列错位
        var columns = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var row in rows)
        {
            foreach (var key in row.Keys)
            {
                if (seen.Add(key)) columns.Add(key);
            }
        }

        var result = new List<IReadOnlyList<string>>(rows.Count);
        foreach (var row in rows)
        {
            result.Add(columns.Select(c => row.TryGetValue(c, out var v) ? v?.ToString() ?? string.Empty : string.Empty).ToList());
        }
        return result;
    }

    private static string BuildMarkdown(string caption, List<IReadOnlyList<string>> rows, int offset, int limit)
    {
        var sb = new StringBuilder();
        var totalDataRows = Math.Max(0, rows.Count - 1);

        if (rows.Count == 0)
            return $"{caption}\n\n（空表：没有任何行）";

        var width = rows.Max(r => r.Count);
        var header = NormalizeRow(rows[0], width, isHeader: true);
        var dataRows = rows.Skip(1).ToList();
        var pageStart = Math.Min(offset - 1, dataRows.Count);
        var page = dataRows.Skip(pageStart).Take(limit).ToList();

        sb.AppendLine(caption);
        sb.AppendLine();
        sb.AppendLine($"| {string.Join(" | ", header)} |");
        sb.AppendLine($"| {string.Join(" | ", header.Select(_ => "---"))} |");

        var truncated = false;
        foreach (var row in page)
        {
            sb.Append("| ").Append(string.Join(" | ", NormalizeRow(row, width, isHeader: false))).AppendLine(" |");
            if (sb.Length > MaxChars)
            {
                truncated = true;
                break;
            }
        }
        sb.AppendLine();

        var returned = page.Count;
        if (dataRows.Count == 0)
        {
            sb.AppendLine("（只有表头，没有数据行）");
        }
        else if (truncated)
        {
            sb.AppendLine($"[内容已截断] 已达 {MaxChars / 1024}KB 上限，本次仅返回第 {offset} 行起的部分数据。"
                + $"请缩小 limit 或增大 offset 分段读取。");
        }
        else if (pageStart + returned < dataRows.Count)
        {
            sb.AppendLine($"[分页] 共 {dataRows.Count} 行数据，本次返回第 {offset}-{offset + returned - 1} 行。"
                + $"继续读取请传 offset={offset + returned}。");
        }
        else
        {
            sb.AppendLine($"[分页] 共 {totalDataRows} 行数据，本次返回第 {offset}-{offset + Math.Max(returned - 1, 0)} 行（已到末尾）。");
        }

        return sb.ToString().TrimEnd();
    }

    private static IReadOnlyList<string> NormalizeRow(IReadOnlyList<string> row, int width, bool isHeader)
    {
        var cells = new List<string>(width);
        for (var i = 0; i < width; i++)
        {
            var value = i < row.Count ? row[i] : string.Empty;
            if (isHeader && string.IsNullOrWhiteSpace(value))
                value = $"列{i + 1}";
            cells.Add(Escape(value));
        }
        return cells;
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value
            .Replace("|", "\\|")
            .Replace("\r\n", "<br>")
            .Replace("\n", "<br>")
            .Replace("\r", "<br>")
            .Trim();
    }

    /// <summary>
    /// 解析带引号转义的分隔符文本（RFC4180 简化实现：支持 "" 转义与字段内换行）。
    /// </summary>
    private static List<IReadOnlyList<string>> ParseDelimited(string text, char delimiter)
    {
        var rows = new List<IReadOnlyList<string>>();
        var fields = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    field.Append(c);
                }
                continue;
            }

            switch (c)
            {
                case '"' when field.Length == 0:
                    inQuotes = true;
                    break;
                case '\r':
                    break;
                case '\n':
                    fields.Add(field.ToString());
                    field.Clear();
                    rows.Add(fields.ToList());
                    fields.Clear();
                    break;
                default:
                    if (c == delimiter)
                    {
                        fields.Add(field.ToString());
                        field.Clear();
                    }
                    else
                    {
                        field.Append(c);
                    }
                    break;
            }
        }

        if (field.Length > 0 || fields.Count > 0)
        {
            fields.Add(field.ToString());
            rows.Add(fields.ToList());
        }

        return rows;
    }
}