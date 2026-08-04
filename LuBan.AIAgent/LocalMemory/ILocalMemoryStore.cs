/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.LocalMemory
*文件名： ILocalMemoryStore
*版本号： V1.0.0.0
*唯一标识：a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/4
*描述：本地记忆持久化存储接口
*
*=================================================
*修改标记
*修改时间：2026/8/4
*修改人： yswenli
*版本号： V1.0.0.0
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
    /// 保存或更新记忆条目（包含向量字节）
    /// </summary>
    Task SaveAsync(MemoryEntry entry, byte[] vectorBytes, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据 ID 删除记忆条目
    /// </summary>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 列出记忆条目，按时间倒序
    /// </summary>
    Task<IReadOnlyList<MemoryEntry>> ListAsync(string? category = null, int limit = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// 加载所有记忆条目及其向量字节
    /// </summary>
    Task<IReadOnlyList<(MemoryEntry Entry, byte[] VectorBytes)>> LoadAllAsync(string? category = null, CancellationToken cancellationToken = default);
}
