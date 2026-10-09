/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.LogAnalysis
*文件名： LogAggregator
*描述：单遍流式日志聚合（恒定内存）
*****************************************************************************/
namespace LuBan.AIAgent.LogAnalysis;

/// <summary>
/// 单遍流式日志聚合器：内存恒定，只保留有界的签名表（≤ MaxDistinctSignatures）
/// 与每签名的有界样本行号（≤ SampleLinesPerSignature）。内部超时按截断处理而非抛出；
/// 外部取消则正常抛出 <see cref="OperationCanceledException"/>。
/// </summary>
internal sealed class LogAggregator : ILogAggregator
{
    private readonly LogToolOptions _options;
    private readonly LogPatterns _patterns;

    /// <summary>构造（会就地钳制配置的硬上限）。</summary>
    public LogAggregator(IOptions<LogToolOptions> options)
    {
        _options = options?.Value ?? new LogToolOptions();
        _options.ApplyHardCaps();
        _patterns = new LogPatterns(_options.LevelPattern, _options.TimestampPattern, _options.RegexMatchTimeoutMs);
    }

    /// <inheritdoc />
    public async Task<LogSummary> AggregateAsync(string filePath, CancellationToken ct = default)
    {
        var info = new FileInfo(filePath);
        if (!info.Exists)
            throw new FileNotFoundException($"日志文件不存在：{filePath}", filePath);

        var fileSize = info.Length;
        if (fileSize > _options.MaxFileSizeBytes)
        {
            return new LogSummary(
                filePath, fileSize, 0, true, "size", null, null,
                new Dictionary<LogSeverity, long>(), Array.Empty<ErrorSignature>(), 0);
        }

        var levelCounts = new Dictionary<LogSeverity, long>();
        var signatures = new Dictionary<string, SignatureAccumulator>(StringComparer.Ordinal);
        var otherErrorCount = 0L;
        DateTimeOffset? firstTimestamp = null;
        DateTimeOffset? lastTimestamp = null;
        var totalLines = 0L;
        var truncated = false;
        string? truncationReason = null;

        using var timeoutCts = new CancellationTokenSource(_options.MaxDurationMs);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

        try
        {
            var reader = new LogLineReader(filePath, _options.MaxLineBytes);
            await foreach (var line in reader.ReadAsync(linked.Token))
            {
                totalLines++;

                var level = _patterns.ParseLevel(line.Text);
                levelCounts[level] = levelCounts.TryGetValue(level, out var count) ? count + 1 : 1;

                if (_patterns.TryParseTimestamp(line.Text, out var timestamp))
                {
                    firstTimestamp ??= timestamp;
                    lastTimestamp = timestamp;
                }

                if (level is LogSeverity.Error or LogSeverity.Fatal)
                {
                    var template = LogSignatureNormalizer.Normalize(line.Text);
                    if (signatures.TryGetValue(template, out var accumulator))
                    {
                        accumulator.Add(line.LineNumber);
                    }
                    else if (signatures.Count < _options.MaxDistinctSignatures)
                    {
                        signatures[template] = new SignatureAccumulator(line.LineNumber, _options.SampleLinesPerSignature);
                    }
                    else
                    {
                        otherErrorCount++;
                    }
                }
            }
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !ct.IsCancellationRequested)
        {
            truncated = true;
            truncationReason = "timeout";
        }

        var topErrors = signatures
            .Select(kv => new ErrorSignature(kv.Key, kv.Value.Count, kv.Value.Sample))
            .OrderByDescending(e => e.Count)
            .ThenBy(e => e.Template, StringComparer.Ordinal)
            .Take(_options.TopN)
            .ToList();

        return new LogSummary(
            filePath, fileSize, totalLines, truncated, truncationReason,
            firstTimestamp, lastTimestamp, levelCounts, topErrors, otherErrorCount);
    }

    private sealed class SignatureAccumulator
    {
        private readonly int _sampleCap;
        private readonly List<long> _samples;

        public SignatureAccumulator(long firstLineNumber, int sampleCap)
        {
            _sampleCap = Math.Max(1, sampleCap);
            _samples = new List<long>(_sampleCap) { firstLineNumber };
        }

        public long Count { get; private set; } = 1;

        public IReadOnlyList<long> Sample => _samples;

        public void Add(long lineNumber)
        {
            Count++;
            if (_samples.Count < _sampleCap)
                _samples.Add(lineNumber);
        }
    }
}
