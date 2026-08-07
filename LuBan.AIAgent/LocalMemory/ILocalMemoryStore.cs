/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.LocalMemory
*文件名： ILocalMemoryStore
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：本地记忆持久化存储接口
*
*****************************************************************************/
namespace LuBan.AIAgent.LocalMemory;

/// <summary>
/// 本地记忆持久化存储接口
/// </summary>
public interface ILocalMemoryStore
{
    /// <summary>
    /// 按 (WorkspaceId, ContentHash) 去重 upsert；命中则更新并返回原条目（保留原 Id）
    /// </summary>
    Task<MemoryEntry> UpsertAsync(MemoryEntry entry, byte[] vectorBytes, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据 ID 删除记忆条目
    /// </summary>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 列出记忆条目，按时间倒序；workspaceId 语义：仅显示该工作区 + 全局(WorkspaceId IS NULL)行
    /// </summary>
    Task<IReadOnlyList<MemoryEntry>> ListAsync(string? category = null, string? workspaceId = null, int limit = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// 加载记忆条目及向量；includeAllWorkspaces=true 时不做工作区过滤（用于倒排索引构建）
    /// </summary>
    Task<IReadOnlyList<(MemoryEntry Entry, byte[] VectorBytes)>> LoadAllAsync(string? category = null, string? workspaceId = null, bool includeAllWorkspaces = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 按 ID 批量加载（配合倒排索引预筛），应用工作区可见性过滤
    /// </summary>
    Task<IReadOnlyList<(MemoryEntry Entry, byte[] VectorBytes)>> LoadByIdsAsync(IEnumerable<string> ids, string? workspaceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 物理删除全部已过期条目，返回删除行数
    /// </summary>
    Task<int> DeleteExpiredAsync(CancellationToken cancellationToken = default);
}
