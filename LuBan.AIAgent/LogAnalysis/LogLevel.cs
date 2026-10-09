namespace LuBan.AIAgent.LogAnalysis;

/// <summary>日志级别（枚举顺序即严重度由低到高）。</summary>
public enum LogLevel
{
    /// <summary>跟踪。</summary>
    Trace,
    /// <summary>调试。</summary>
    Debug,
    /// <summary>信息。</summary>
    Info,
    /// <summary>警告。</summary>
    Warn,
    /// <summary>错误。</summary>
    Error,
    /// <summary>致命。</summary>
    Fatal,
    /// <summary>未知（未匹配到级别）。</summary>
    Unknown
}
