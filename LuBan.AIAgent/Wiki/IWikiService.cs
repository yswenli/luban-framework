/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Wiki
*文件名： IWikiService
*版本号： V1.0.0.0
*唯一标识：3f9a1c7e-5b2d-4e88-a6c4-1d8f7b2e9a50
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/20
*描述：wiki 知识库服务接口
*
*****************************************************************************/
using LuBan.AIAgent.Retrieval;

namespace LuBan.AIAgent.Wiki;

/// <summary>
/// wiki 知识库服务：确定性维护 wiki/ 的路径布局、index.md、log.md、链接校验与增量重索引。
/// </summary>
public interface IWikiService
{
    /// <summary>读取 index.md。</summary>
    Task<WikiIndex> ReadIndexAsync(CancellationToken cancellationToken = default);

    /// <summary>读取页面（相对 wiki 根的路径）。</summary>
    Task<WikiPage> ReadPageAsync(string relativePath, CancellationToken cancellationToken = default);

    /// <summary>写入/更新页面：落盘 + index upsert + log 追加 + 单页增量重索引。</summary>
    Task SavePageAsync(WikiPage page, CancellationToken cancellationToken = default);

    /// <summary>删除页面：删文件 + index 移除 + log 追加 + 向量软删除。</summary>
    Task DeletePageAsync(string relativePath, CancellationToken cancellationToken = default);

    /// <summary>搜索 wiki（默认限定 wiki 目录），includeRaw 时追加工作区根召回。</summary>
    Task<IReadOnlyList<RetrievalResult>> SearchAsync(string query, int topK = 8, bool includeRaw = false, CancellationToken cancellationToken = default);

    /// <summary>逐页重建 wiki 向量索引（排除 log.md），并清理消失页面。</summary>
    Task<IndexReport> RebuildIndexAsync(CancellationToken cancellationToken = default);

    /// <summary>客观项 lint。</summary>
    Task<WikiLintReport> LintAsync(CancellationToken cancellationToken = default);

    /// <summary>文件系统口径统计。</summary>
    Task<WikiStats> GetStatsAsync(CancellationToken cancellationToken = default);
}