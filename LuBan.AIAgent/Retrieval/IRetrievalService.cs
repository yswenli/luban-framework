namespace LuBan.AIAgent.Retrieval;

/// <summary>
/// 语义检索服务接口
/// </summary>
public interface IRetrievalService
{
    /// <summary>
    /// 索引目录（增量）
    /// </summary>
    Task<IndexReport> IndexDirectoryAsync(string path, string? glob = null, bool force = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 索引一段文本内容（网页等虚拟来源，sourceName 如 web://example.com/page）
    /// </summary>
    Task<IndexReport> IndexContentAsync(string content, string language, string sourceName, CancellationToken cancellationToken = default);

    /// <summary>
    /// 语义搜索
    /// </summary>
    Task<IReadOnlyList<RetrievalResult>> SearchAsync(string query, int topK = 5, string? pathPrefix = null, string? language = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 索引统计
    /// </summary>
    Task<IndexStats> GetStatsAsync();
}
