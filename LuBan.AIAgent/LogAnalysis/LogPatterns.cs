/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.LogAnalysis
*文件名： LogPatterns
*描述：日志级别与时间戳解析（内置探测 + 可配置正则）
*****************************************************************************/
using System.Globalization;

namespace LuBan.AIAgent.LogAnalysis;

/// <summary>
/// 级别/时间戳解析：优先使用配置正则（需含命名组 <c>lvl</c> / <c>ts</c>），否则用内置探测。
/// 正则匹配超时（<see cref="RegexMatchTimeoutException"/>）按「未匹配」处理，不向上抛出。
/// </summary>
internal sealed class LogPatterns
{
    internal const string DefaultLevelPattern =
        @"\b(?<lvl>TRACE|DEBUG|INFO|INFORMATION|WARN|WARNING|ERROR|FATAL|CRITICAL)\b";

    internal const string DefaultTimestampPattern =
        @"(?<ts>\d{4}-\d{2}-\d{2}[T ]\d{2}:\d{2}:\d{2}(?:[.,]\d+)?(?:Z|[+-]\d{2}:?\d{2})?)";

    private readonly Regex _level;
    private readonly Regex _timestamp;

    public LogPatterns(string? levelPattern, string? timestampPattern, int matchTimeoutMs)
    {
        var timeout = TimeSpan.FromMilliseconds(Math.Max(1, matchTimeoutMs));
        _level = Compile(levelPattern, DefaultLevelPattern, timeout, RegexOptions.IgnoreCase);
        _timestamp = Compile(timestampPattern, DefaultTimestampPattern, timeout);
    }

    private static Regex Compile(string? pattern, string fallback, TimeSpan timeout, RegexOptions extra = RegexOptions.None)
    {
        var options = RegexOptions.Compiled | RegexOptions.CultureInvariant | extra;
        var effective = string.IsNullOrWhiteSpace(pattern) ? fallback : pattern;
        try
        {
            return new Regex(effective, options, timeout);
        }
        catch (ArgumentException)
        {
            return new Regex(fallback, options, timeout);
        }
    }

    /// <summary>解析行级别；未匹配返回 <see cref="LogSeverity.Unknown"/>。</summary>
    public LogSeverity ParseLevel(string line)
    {
        try
        {
            var m = _level.Match(line);
            if (!m.Success) return LogSeverity.Unknown;
            return GroupOrMatch(m, "lvl").ToUpperInvariant() switch
            {
                "TRACE" => LogSeverity.Trace,
                "DEBUG" => LogSeverity.Debug,
                "INFO" or "INFORMATION" => LogSeverity.Info,
                "WARN" or "WARNING" => LogSeverity.Warn,
                "ERROR" => LogSeverity.Error,
                "FATAL" or "CRITICAL" => LogSeverity.Fatal,
                _ => LogSeverity.Unknown
            };
        }
        catch (RegexMatchTimeoutException)
        {
            return LogSeverity.Unknown;
        }
    }

    /// <summary>尝试解析行时间戳（统一按 UTC 处理）。</summary>
    public bool TryParseTimestamp(string line, out DateTimeOffset timestamp)
    {
        timestamp = default;
        try
        {
            var m = _timestamp.Match(line);
            if (!m.Success) return false;
            var raw = GroupOrMatch(m, "ts");
            if (string.IsNullOrEmpty(raw)) return false;
            var normalized = raw.Replace(' ', 'T').Replace(',', '.');
            return DateTimeOffset.TryParse(
                normalized,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out timestamp);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    /// <summary>取命名组值；组不存在或无值时回退整段匹配。</summary>
    internal static string GroupOrMatch(Match match, string groupName)
    {
        var group = match.Groups[groupName];
        return group.Success ? group.Value : match.Value;
    }
}