/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Tools.Log
*文件名： LogToolGroup
*描述：大日志分析工具实现（LogStats / LogGrep / LogSlice）
*****************************************************************************/
using System.ComponentModel;
using System.Globalization;

namespace LuBan.AIAgent.Tools.Log;

/// <summary>大日志分析工具实现。全部只读，共享注入的 <see cref="ILogAggregator"/> 与 <see cref="LogToolOptions"/>。</summary>
public class LogToolGroup
{
    private const string TruncationSuffix = "\n…（已截断）";
    private const int SliceMaxBytes = 256 * 1024;

    private readonly ILogAggregator _aggregator;
    private readonly LogToolOptions _options;
    private readonly IToolConfirmationService _confirmation;
    private readonly LogPatterns _patterns;

    /// <summary>构造。</summary>
    public LogToolGroup(ILogAggregator aggregator, LogToolOptions options, IToolConfirmationService confirmation)
    {
        _aggregator = aggregator;
        _options = options ?? new LogToolOptions();
        _options.ApplyHardCaps();
        _confirmation = confirmation;
        _patterns = new LogPatterns(_options.LevelPattern, _options.TimestampPattern, _options.RegexMatchTimeoutMs);
    }

    private async Task<ToolResult<string>?> ConfirmOrBlockAsync(
        string toolName, string path, Dictionary<string, object?> arguments)
    {
        var outcome = await _confirmation.EvaluateAsync(toolName, path, arguments).ConfigureAwait(false);
        return outcome switch
        {
            EnumConfirmationOutcome.Allowed => null,
            EnumConfirmationOutcome.Planned => ToolResult.Plan<string>(),
            _ => ToolResult.Denied<string>()
        };
    }

    /// <summary>对日志文件生成聚合概览。</summary>
    /// <param name="path">日志文件路径。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    [Description("对日志文件生成聚合概览：级别统计、Top 错误签名（含样本行号）、时间范围。恒定内存，支持超大日志（默认上限 1GB）。")]
    public async Task<ToolResult<string>> LogStatsAsync(string path, CancellationToken cancellationToken = default)
    {
        if (await ConfirmOrBlockAsync(nameof(LogStatsAsync), path,
            new Dictionary<string, object?> { ["path"] = path }) is { } blocked)
        {
            return blocked;
        }

        if (Directory.Exists(path))
            return ToolResult.Fail<string>($"路径是目录: {path}。LogStats 需要日志文件路径。");

        try
        {
            var summary = await _aggregator.AggregateAsync(path, cancellationToken).ConfigureAwait(false);
            return ToolResult.Ok(LogSummaryFormatter.Format(summary, _options.MaxResultChars));
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) { return FileError(ex, path); }
    }

    /// <summary>在日志文件中按正则检索。</summary>
    /// <param name="path">日志文件路径。</param>
    /// <param name="pattern">正则表达式（大小写不敏感）。</param>
    /// <param name="maxMatches">命中上限；≤0 用配置默认值。</param>
    /// <param name="contextLines">每处命中附带的上下文行数（前/后各 N 行，受硬上限约束）。</param>
    /// <param name="startLine">起始行号（含）；≤0 表示从头。</param>
    /// <param name="endLine">结束行号（含）；≤0 表示到末尾。</param>
    /// <param name="level">仅保留指定级别（如 error/warn/info）；null 不过滤。</param>
    /// <param name="fromTime">起始时间（可解析的时间字符串）；null 不过滤。</param>
    /// <param name="toTime">结束时间；null 不过滤。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    [Description("在日志文件中按正则检索，返回 路径:行号: 内容；可选上下文行、级别与时间范围过滤。恒定内存。")]
    public async Task<ToolResult<string>> LogGrepAsync(
        string path,
        string pattern,
        int maxMatches = 0,
        int contextLines = 0,
        long startLine = 0,
        long endLine = 0,
        string? level = null,
        string? fromTime = null,
        string? toTime = null,
        CancellationToken cancellationToken = default)
    {
        if (await ConfirmOrBlockAsync(nameof(LogGrepAsync), path,
            new Dictionary<string, object?> { ["path"] = path, ["pattern"] = pattern }) is { } blocked)
        {
            return blocked;
        }

        if (Directory.Exists(path))
            return ToolResult.Fail<string>($"路径是目录: {path}。LogGrep 需要日志文件路径。");

        Regex regex;
        try
        {
            regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
                TimeSpan.FromMilliseconds(_options.RegexMatchTimeoutMs));
        }
        catch (ArgumentException ex)
        {
            return ToolResult.Fail<string>($"正则表达式无效: {pattern}。{ex.Message}");
        }

        var from = TryParseTime(fromTime);
        var to = TryParseTime(toTime);
        var levelFilter = ParseLevelFilter(level);
        var effectiveMax = maxMatches > 0 ? Math.Min(maxMatches, _options.MaxMatches) : _options.MaxMatches;
        if (contextLines < 0) contextLines = 0;
        if (contextLines > LogToolOptions.HardMaxContextLines) contextLines = LogToolOptions.HardMaxContextLines;

        var body = new StringBuilder();
        var before = new Queue<LogLine>();
        var hits = 0L;
        var skippedTimeout = 0;
        var truncatedByMax = false;
        var timedOut = false;
        var limitReached = false;
        var follow = 0;
        var lastEmitted = 0L;

        using var timeoutCts = new CancellationTokenSource(_options.MaxDurationMs);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            var reader = new LogLineReader(path, _options.MaxLineBytes);
            await foreach (var line in reader.ReadAsync(linked.Token))
            {
                if (startLine > 0 && line.LineNumber < startLine) continue;
                if (endLine > 0 && line.LineNumber > endLine) break;

                var isMatch = false;
                if (!limitReached)
                {
                    try
                    {
                        isMatch = regex.IsMatch(line.Text);
                    }
                    catch (RegexMatchTimeoutException)
                    {
                        skippedTimeout++;
                    }

                    if (isMatch && !PassesFilters(line.Text, levelFilter, from, to))
                        isMatch = false;
                }

                if (isMatch)
                {
                    while (before.Count > 0)
                    {
                        var context = before.Dequeue();
                        if (context.LineNumber > lastEmitted)
                        {
                            AppendGrepLine(body, path, context, isMatch: false);
                            lastEmitted = context.LineNumber;
                        }
                    }

                    AppendGrepLine(body, path, line, isMatch: true);
                    lastEmitted = line.LineNumber;
                    hits++;
                    follow = contextLines;

                    if (hits >= effectiveMax)
                        limitReached = true;
                }
                else if (follow > 0)
                {
                    AppendGrepLine(body, path, line, isMatch: false);
                    lastEmitted = line.LineNumber;
                    follow--;
                }
                else if (limitReached)
                {
                    truncatedByMax = true;
                    break;
                }
                else
                {
                    before.Enqueue(line);
                    while (before.Count > contextLines) before.Dequeue();
                }

                if (body.Length > _options.MaxResultChars)
                {
                    truncatedByMax = true;
                    break;
                }
            }
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            timedOut = true;
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) { return FileError(ex, path); }

        if (hits == 0)
        {
            var none = new StringBuilder("未找到匹配的内容");
            if (skippedTimeout > 0)
                none.Append($"（{skippedTimeout} 行的正则匹配超时被跳过）");
            if (timedOut)
                none.Append("（检索达到时长上限）");
            return ToolResult.Ok<string>(none.ToString());
        }

        var header = new StringBuilder();
        header.AppendLine($"命中 {hits} 处" + (truncatedByMax ? "（已达上限，可能被截断）" : string.Empty));
        if (skippedTimeout > 0)
            header.AppendLine($"（{skippedTimeout} 行的正则匹配超时被跳过）");
        if (timedOut)
            header.AppendLine("（检索达到时长上限，结果可能不完整）");

        var text = header.Append(body).ToString();
        if (text.Length > _options.MaxResultChars)
        {
            var take = Math.Max(0, _options.MaxResultChars - TruncationSuffix.Length);
            text = text[..take] + TruncationSuffix;
        }
        return ToolResult.Ok<string>(text);
    }

    /// <summary>按行号区间返回日志原文切片。</summary>
    /// <param name="path">日志文件路径。</param>
    /// <param name="startLine">起始行号（含），从 1 起。</param>
    /// <param name="lineCount">返回行数；≤0 用配置默认值，超过单次上限被钳制。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    [Description("按行号区间返回日志原文切片（行号从 1 起）。恒定内存。")]
    public async Task<ToolResult<string>> LogSliceAsync(
        string path, long startLine, int lineCount, CancellationToken cancellationToken = default)
    {
        if (await ConfirmOrBlockAsync(nameof(LogSliceAsync), path,
            new Dictionary<string, object?> { ["path"] = path }) is { } blocked)
        {
            return blocked;
        }

        if (Directory.Exists(path))
            return ToolResult.Fail<string>($"路径是目录: {path}。LogSlice 需要日志文件路径。");

        if (startLine < 1) startLine = 1;
        var effective = lineCount < 1 ? _options.MaxSliceLines : Math.Min(lineCount, _options.MaxSliceLines);

        var builder = new StringBuilder();
        var emitted = 0;
        var reachedEof = true;
        var timedOut = false;
        var lastLine = 0L;

        using var timeoutCts = new CancellationTokenSource(_options.MaxDurationMs);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            var reader = new LogLineReader(path, _options.MaxLineBytes);
            await foreach (var line in reader.ReadAsync(linked.Token))
            {
                if (emitted >= effective)
                {
                    reachedEof = false;
                    break;
                }

                lastLine = line.LineNumber;
                if (line.LineNumber < startLine) continue;

                builder.Append(line.LineNumber).Append(": ").Append(line.Text).Append('\n');
                emitted++;
            }
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            timedOut = true;
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) { return FileError(ex, path); }

        if (emitted == 0)
            return ToolResult.Ok<string>($"起始行 {startLine} 超出文件行数（共 {lastLine} 行）。");

        var text = builder.ToString();
        if (text.Length > SliceMaxBytes)
        {
            var take = Math.Max(0, SliceMaxBytes - TruncationSuffix.Length);
            text = text[..take] + TruncationSuffix;
        }
        if (timedOut)
            text += "\n[已截断] 达到时长上限。";
        else if (!reachedEof)
            text += $"\n[已截断] 单次最多返回 {effective} 行。";
        else
            text += $"\n[文件结束] 共 {lastLine} 行。";

        return ToolResult.Ok<string>(text);
    }

    private bool PassesFilters(string text, LogSeverity? levelFilter, DateTimeOffset? from, DateTimeOffset? to)
    {
        if (levelFilter is { } wanted && _patterns.ParseLevel(text) != wanted)
            return false;

        if (from is not null || to is not null)
        {
            if (!_patterns.TryParseTimestamp(text, out var timestamp))
                return false;
            if (from is { } f && timestamp < f) return false;
            if (to is { } t && timestamp > t) return false;
        }

        return true;
    }

    private static void AppendGrepLine(StringBuilder builder, string path, LogLine line, bool isMatch)
        => builder.Append(path).Append(':').Append(line.LineNumber)
            .Append(isMatch ? ": " : "- ").Append(line.Text).Append('\n');

    private static LogSeverity? ParseLevelFilter(string? level)
    {
        if (string.IsNullOrWhiteSpace(level)) return null;
        return level.Trim().ToUpperInvariant() switch
        {
            "TRACE" => LogSeverity.Trace,
            "DEBUG" => LogSeverity.Debug,
            "INFO" or "INFORMATION" => LogSeverity.Info,
            "WARN" or "WARNING" => LogSeverity.Warn,
            "ERROR" => LogSeverity.Error,
            "FATAL" or "CRITICAL" => LogSeverity.Fatal,
            _ => null
        };
    }

    private static DateTimeOffset? TryParseTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var timestamp)
            ? timestamp
            : null;
    }

    private static ToolResult<string> FileError(Exception ex, string path) => ex switch
    {
        FileNotFoundException => ToolResult.Fail<string>($"未找到文件: {path}。请检查路径是否正确。"),
        DirectoryNotFoundException => ToolResult.Fail<string>($"未找到目录: {path}。请检查路径是否正确。"),
        UnauthorizedAccessException => ToolResult.Fail<string>($"无法访问文件: {path}，权限不足。"),
        _ => ToolResult.Fail<string>($"操作失败: {ex.Message}")
    };
}
