namespace LuBan.AIAgent.LogAnalysis;

/// <summary>日志聚合器：单遍流式扫描，恒定内存。</summary>
public interface ILogAggregator
{
    /// <summary>聚合成概览。</summary>
    /// <param name="filePath">日志文件路径。</param>
    /// <param name="ct">外部取消令牌。</param>
    Task<LogSummary> AggregateAsync(string filePath, CancellationToken ct = default);
}
