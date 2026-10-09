/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Configuration
*文件名： LogToolOptions
*描述：大日志分析引擎与 log 工具组共用配置（引擎读取以注入的 IOptions 为准）
*****************************************************************************/
namespace LuBan.AIAgent.Configuration;

/// <summary>
/// 大日志分析配置。绑定到 <c>LuBanAgent:Tools:Log</c>，引擎与工具组共用同一对象。
/// <para>
/// 数值型护栏在 <see cref="ApplyHardCaps"/> 中做幂等 Math.Clamp，防止配置破掉内存护栏；
/// 该方法由 <c>LogAggregator</c> / <c>LogToolGroup</c> / <c>LogToolPlugin</c> 构造时调用。
/// </para>
/// </summary>
public class LogToolOptions
{
    /// <summary>文件大小硬上限（2GB）：配置超过此值被钳制到该值。</summary>
    public const long HardMaxFileSizeBytes = 2L * 1024 * 1024 * 1024;

    /// <summary>Top-N 硬上限。</summary>
    public const int HardMaxTopN = 100;

    /// <summary>不同错误签名数量硬上限。</summary>
    public const int HardMaxDistinctSignatures = 1000;

    /// <summary>单次切片行数硬上限。</summary>
    public const int HardMaxSliceLines = 5000;

    /// <summary>单行字节硬上限（1MB）：配置超过此值被钳制，防止巨行撑爆内存。</summary>
    public const int HardMaxLineBytes = 1024 * 1024;

    /// <summary>单行正则匹配超时硬上限（60 秒）：防止 ReDoS。</summary>
    public const int HardMaxRegexMatchTimeoutMs = 60_000;

    /// <summary>是否启用 log 工具组与附件自动聚合。</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>触发附件自动聚合的扩展名（不含点或含点均可，大小写不敏感）。</summary>
    public string[] AggregateExtensions { get; set; } = ["log"];

    /// <summary>单文件大小上限（字节）。默认 1GB，硬上限 2GB。</summary>
    public long MaxFileSizeBytes { get; set; } = 1024L * 1024 * 1024;

    /// <summary>单次聚合/检索时长上限（毫秒）。</summary>
    public int MaxDurationMs { get; set; } = 30_000;

    /// <summary>单行字节上限，超出仅用于匹配不累积（防无换行巨行）。</summary>
    public int MaxLineBytes { get; set; } = 65_536;

    /// <summary>概览保留的错误签名条数。硬上限 100。</summary>
    public int TopN { get; set; } = 20;

    /// <summary>签名表条目上限，超出归入 OtherErrorCount。硬上限 1000。</summary>
    public int MaxDistinctSignatures { get; set; } = 200;

    /// <summary>自定义级别正则（含命名组 <c>lvl</c>）；null 用内置探测。</summary>
    public string? LevelPattern { get; set; }

    /// <summary>自定义时间戳正则（含命名组 <c>ts</c>）；null 用内置探测。</summary>
    public string? TimestampPattern { get; set; }

    /// <summary>LogGrep 单次命中数上限。</summary>
    public int MaxMatches { get; set; } = 200;

    /// <summary>LogSlice 单次返回行数上限。硬上限 5000。</summary>
    public int MaxSliceLines { get; set; } = 2000;

    /// <summary>单次渲染结果字符上限。</summary>
    public int MaxResultChars { get; set; } = 8000;

    /// <summary>单行正则匹配超时（毫秒）。</summary>
    public int RegexMatchTimeoutMs { get; set; } = 5000;

    /// <summary>每个错误签名保留的样本行号数。</summary>
    public int SampleLinesPerSignature { get; set; } = 3;

    /// <summary>
    /// 幂等钳制：把数值型配置收口到硬上限/默认值区间内，防止配置破掉内存护栏。
    /// </summary>
    public void ApplyHardCaps()
    {
        if (MaxFileSizeBytes <= 0 || MaxFileSizeBytes > HardMaxFileSizeBytes)
            MaxFileSizeBytes = MaxFileSizeBytes <= 0 ? 1024L * 1024 * 1024 : HardMaxFileSizeBytes;
        TopN = TopN <= 0 ? 20 : Math.Min(TopN, HardMaxTopN);
        MaxDistinctSignatures = MaxDistinctSignatures <= 0 ? 200 : Math.Min(MaxDistinctSignatures, HardMaxDistinctSignatures);
        MaxSliceLines = MaxSliceLines <= 0 ? 2000 : Math.Min(MaxSliceLines, HardMaxSliceLines);
        MaxDurationMs = MaxDurationMs <= 0 ? 30_000 : MaxDurationMs;
        MaxLineBytes = MaxLineBytes <= 0 ? 65_536 : Math.Min(MaxLineBytes, HardMaxLineBytes);
        MaxMatches = MaxMatches <= 0 ? 200 : MaxMatches;
        MaxResultChars = MaxResultChars <= 0 ? 8000 : MaxResultChars;
        RegexMatchTimeoutMs = RegexMatchTimeoutMs <= 0 ? 5000 : Math.Min(RegexMatchTimeoutMs, HardMaxRegexMatchTimeoutMs);
        SampleLinesPerSignature = SampleLinesPerSignature <= 0 ? 3 : SampleLinesPerSignature;
    }

    /// <summary>按扩展名判断是否应自动聚合。</summary>
    public bool IsAggregateExtension(string filePath)
    {
        if (AggregateExtensions is null) return false;
        var ext = Path.GetExtension(filePath);
        if (string.IsNullOrEmpty(ext)) return false;
        var normalized = ext.TrimStart('.');
        foreach (var candidate in AggregateExtensions)
        {
            if (!string.IsNullOrWhiteSpace(candidate)
                && string.Equals(candidate.Trim().TrimStart('.'), normalized, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}