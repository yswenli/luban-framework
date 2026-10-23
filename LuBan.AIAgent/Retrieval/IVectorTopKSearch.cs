/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Retrieval
*文件名： IVectorTopKSearch
*版本号： V1.0.0.0
*唯一标识：向量存储 top-K 检索扩展接口
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*描述：向量存储的可选扩展：由实现方在库内完成 top-K 打分，避免调用方把整张向量表物化到内存。
*
*****************************************************************************/
namespace LuBan.AIAgent.Retrieval;

/// <summary>
/// 向量存储的可选扩展接口：在库内完成「分页扫描 + top-K 选择」，
/// 调用方不再需要把整张向量表读成内存列表。
/// </summary>
/// <remarks>
/// 实现该接口的存储应保证与 <see cref="VectorMath.Cosine"/> + 「分数降序、Id 升序」排序完全一致的结果。
/// 未实现时 <see cref="RetrievalService"/> 退回全量加载的兼容路径。
/// </remarks>
public interface IVectorTopKSearch
{
    /// <summary>
    /// 是否存在任何可参与检索的向量（用于空索引短路：库为空时无需加载嵌入模型）。
    /// </summary>
    /// <param name="pathPrefix">路径前缀过滤；为空表示不限。</param>
    /// <param name="language">语言过滤；为空表示不限。</param>
    /// <param name="workspaceId">目标工作区标识；为 null 时由实现方按当前工作区解析。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    Task<bool> HasVectorsAsync(string? pathPrefix = null, string? language = null, string? workspaceId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 取与查询向量最相似的 top-K 切块（按分数降序、Id 升序）。
    /// </summary>
    /// <param name="queryVector">查询向量。</param>
    /// <param name="topK">返回条数上限。</param>
    /// <param name="pathPrefix">路径前缀过滤；为空表示不限。</param>
    /// <param name="language">语言过滤；为空表示不限。</param>
    /// <param name="workspaceId">目标工作区标识；为 null 时由实现方按当前工作区解析。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    Task<IReadOnlyList<VectorHit>> SearchTopKAsync(float[] queryVector, int topK, string? pathPrefix = null, string? language = null, string? workspaceId = null, CancellationToken cancellationToken = default);
}