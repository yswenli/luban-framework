/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.LogAnalysis
*文件名： LogSummaryFormatter
*描述：LogSummary → Markdown 概览渲染（附件与 LogStats 共用）
*****************************************************************************/
using System.Globalization;

namespace LuBan.AIAgent.LogAnalysis;

/// <summary>把 <see cref="LogSummary"/> 渲染为 Markdown 概览（附件与 log 工具的 LogStats 共用）。</summary>
internal static class LogSummaryFormatter
{
    private static readonly LogSeverity[] DisplayOrder =
    [
        LogSeverity.Fatal, LogSeverity.Error, LogSeverity.Warn,
        LogSeverity.Info, LogSeverity.Debug, LogSeverity.Trace, LogSeverity.Unknown
    ];

    /// <summary>渲染概览。</summary>
    /// <param name="summary">聚合结果。</param>
    /// <param name="maxResultChars">结果字符上限；≤0 表示不限制。</param>
    public static string Format(LogSummary summary, int maxResultChars)
    {
        var sb = new StringBuilder();
        var sizeMb = (summary.FileSizeBytes / 1024d / 1024d).ToString("F2", CultureInfo.InvariantCulture);

        sb.AppendLine("# 日志概览");
        sb.AppendLine();
        sb.AppendLine($"- 文件：`{summary.FilePath}`");
        sb.AppendLine($"- 大小：{sizeMb} MB（{summary.FileSizeBytes} 字节）");
        sb.AppendLine($"- 已处理行数：{summary.TotalLines}");
        sb.AppendLine($"- 时间范围：{Fmt(summary.FirstTimestamp)} ~ {Fmt(summary.LastTimestamp)}");
        if (summary.Truncated)
            sb.AppendLine($"- ⚠ 结果可能不完整（原因：{summary.TruncationReason ?? "unknown"}）");
        sb.AppendLine();

        sb.AppendLine("## 级别统计");
        sb.AppendLine();
        foreach (var level in DisplayOrder)
        {
            if (summary.LevelCounts.TryGetValue(level, out var count) && count > 0)
                sb.AppendLine($"- {level}: {count}");
        }
        sb.AppendLine();

        sb.AppendLine($"## Top 错误签名（共 {summary.TopErrors.Count} 类）");
        sb.AppendLine();
        if (summary.TopErrors.Count == 0)
        {
            sb.AppendLine("- （无 ERROR/FATAL 行）");
        }
        else
        {
            foreach (var error in summary.TopErrors)
            {
                var samples = error.SampleLineNumbers.Count == 0
                    ? "-"
                    : string.Join(", ", error.SampleLineNumbers);
                sb.AppendLine($"- ×{error.Count} 行 {samples}: {error.Template}");
            }
        }

        if (summary.OtherErrorCount > 0)
            sb.AppendLine($"- 另有 {summary.OtherErrorCount} 条错误未归类（超出签名上限）");
        sb.AppendLine();
        sb.AppendLine("> 可用 `LogGrep` / `LogSlice` 按行号/关键字/级别/时间钻取细节。");

        var text = sb.ToString();
        if (maxResultChars > 0 && text.Length > maxResultChars)
        {
            const string suffix = "\n…（已截断）";
            var take = Math.Max(0, maxResultChars - suffix.Length);
            text = text[..take] + suffix;
        }
        return text;
    }

    private static string Fmt(DateTimeOffset? timestamp)
        => timestamp?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) ?? "(未识别)";
}