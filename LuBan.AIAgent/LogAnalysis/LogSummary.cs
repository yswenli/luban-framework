namespace LuBan.AIAgent.LogAnalysis;

/// <summary>日志聚合概览。</summary>
/// <param name="FilePath">文件绝对路径。</param>
/// <param name="FileSizeBytes">文件字节数。</param>
/// <param name="TotalLines">已处理行数。</param>
/// <param name="Truncated">是否因超时/超限提前结束。</param>
/// <param name="TruncationReason">提前结束原因："timeout"/"size"/null。</param>
/// <param name="FirstTimestamp">首个探测到的时间戳。</param>
/// <param name="LastTimestamp">最后一个探测到的时间戳。</param>
/// <param name="LevelCounts">各级别计数。</param>
/// <param name="TopErrors">Top-N 错误签名。</param>
/// <param name="OtherErrorCount">超出签名上限被归并的错误计数。</param>
public sealed record LogSummary(
    string FilePath,
    long FileSizeBytes,
    long TotalLines,
    bool Truncated,
    string? TruncationReason,
    DateTimeOffset? FirstTimestamp,
    DateTimeOffset? LastTimestamp,
    IReadOnlyDictionary<LogLevel, long> LevelCounts,
    IReadOnlyList<ErrorSignature> TopErrors,
    long OtherErrorCount);
