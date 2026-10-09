namespace LuBan.AIAgent.LogAnalysis;

/// <summary>日志严重级别（Trace..Fatal 顺序由低到高；Unknown 表示未识别，不参与排序）。</summary>
public enum LogSeverity
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