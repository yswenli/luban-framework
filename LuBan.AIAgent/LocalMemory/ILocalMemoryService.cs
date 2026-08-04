/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.LocalMemory
*文件名： ILocalMemoryService
*版本号： V1.0.0.0
*唯一标识：a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/4
*描述：本地长期记忆服务接口
*
*=================================================
*修改标记
*修改时间：2026/8/4
*修改人： yswenli
*版本号： V1.0.0.0
*描述：本地长期记忆服务接口
*
*****************************************************************************/
namespace LuBan.AIAgent.LocalMemory;

/// <summary>
/// 本地长期记忆服务接口
/// </summary>
public interface ILocalMemoryService
{
    /// <summary>
    /// 保存一条记忆
    /// </summary>
    /// <param name="content">记忆内容</param>
    /// <param name="category">记忆类别，如 fact、preference、todo</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>保存的记忆条目</returns>
    Task<MemoryEntry> SaveAsync(string content, string category = "general", CancellationToken cancellationToken = default);

    /// <summary>
    /// 基于语义相似度搜索记忆
    /// </summary>
    /// <param name="query">查询文本</param>
    /// <param name="category">可选类别过滤</param>
    /// <param name="topK">返回条数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>相似度降序排列的记忆结果</returns>
    Task<IReadOnlyList<MemorySearchResult>> SearchAsync(string query, string? category = null, int topK = 5, CancellationToken cancellationToken = default);

    /// <summary>
    /// 列出记忆条目
    /// </summary>
    Task<IReadOnlyList<MemoryEntry>> ListAsync(string? category = null, int limit = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除指定记忆
    /// </summary>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
