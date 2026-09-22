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
    /// <param name="path">目录绝对路径。</param>
    /// <param name="glob">文件匹配模式，多个以分号分隔；为空时匹配全部。</param>
    /// <param name="force">为 true 时忽略哈希未变化直接重建切块。</param>
    /// <param name="progress">进度回调；可为 null。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <param name="workspaceId">目标工作区标识；为 null 时由实现方按当前工作区解析。</param>
    Task<IndexReport> IndexDirectoryAsync(string path, string? glob = null, bool force = false,
        IProgress<IndexProgress>? progress = null, CancellationToken cancellationToken = default, string? workspaceId = null);

    /// <summary>
    /// 索引/刷新单个文件（复用单文件提取 + 切块 + Upsert 逻辑）。
    /// </summary>
    /// <param name="path">文件绝对路径。</param>
    /// <param name="force">为 true 时忽略哈希未变化直接重建切块。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <param name="workspaceId">目标工作区标识；为 null 时由实现方按当前工作区解析。</param>
    Task<IndexReport> IndexFileAsync(string path, bool force = false, CancellationToken cancellationToken = default, string? workspaceId = null);

    /// <summary>
    /// 按来源名（即索引时传入的 <c>FilePath</c>）软删除该文件的全部切块。
    /// </summary>
    /// <param name="sourceName">来源名，通常为该文件的绝对路径。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <param name="workspaceId">目标工作区标识；为 null 时由实现方按当前工作区解析。</param>
    Task RemoveAsync(string sourceName, CancellationToken cancellationToken = default, string? workspaceId = null);

    /// <summary>
    /// 索引一段文本内容（网页等虚拟来源，sourceName 如 web://example.com/page）
    /// </summary>
    /// <param name="content">文本内容。</param>
    /// <param name="language">语言标识。</param>
    /// <param name="sourceName">来源名。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <param name="workspaceId">目标工作区标识；为 null 时由实现方按当前工作区解析。</param>
    Task<IndexReport> IndexContentAsync(string content, string language, string sourceName, CancellationToken cancellationToken = default, string? workspaceId = null);

    /// <summary>
    /// 语义搜索
    /// </summary>
    /// <param name="query">查询文本。</param>
    /// <param name="topK">返回条数。</param>
    /// <param name="pathPrefix">路径前缀过滤；为空表示不限。</param>
    /// <param name="language">语言过滤；为空表示不限。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <param name="workspaceId">目标工作区标识；为 null 时由实现方按当前工作区解析。</param>
    Task<IReadOnlyList<RetrievalResult>> SearchAsync(string query, int topK = 5, string? pathPrefix = null, string? language = null,
        CancellationToken cancellationToken = default, string? workspaceId = null);

    /// <summary>
    /// 索引统计
    /// </summary>
    /// <param name="workspaceId">目标工作区标识；为 null 时由实现方按当前工作区解析。</param>
    Task<IndexStats> GetStatsAsync(string? workspaceId = null);
}
