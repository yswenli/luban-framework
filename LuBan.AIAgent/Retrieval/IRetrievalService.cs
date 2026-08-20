/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Retrieval
*文件名： IRetrievalService
*版本号： V1.0.0.0
*唯一标识：c533b83e-e56a-4927-99db-8afae0d6de46
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：检索服务接口
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：检索服务接口
*
*****************************************************************************/
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
