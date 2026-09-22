/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Retrieval
*文件名： IVectorStore
*版本号： V1.0.0.0
*唯一标识：d6d835ea-d6ab-4e4e-bf32-3aa52a307cce
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：向量存储接口
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：向量存储接口
*
*****************************************************************************/
namespace LuBan.AIAgent.Retrieval;

/// <summary>
/// 向量存储接口（在库中定义，在 ConsoleApp 中实现）
/// </summary>
public interface IVectorStore
{
    /// <summary>
    /// 获取已索引文件（可按路径前缀过滤）
    /// </summary>
    /// <param name="pathPrefix">路径前缀过滤；为空表示不限。</param>
    /// <param name="workspaceId">目标工作区标识；为 null 时由实现方按当前工作区解析。</param>
    Task<IReadOnlyList<IndexedFile>> GetFilesAsync(string? pathPrefix = null, string? workspaceId = null);

    /// <summary>
    /// 新增或更新文件台账，返回文件 Id
    /// </summary>
    /// <param name="filePath">文件路径。</param>
    /// <param name="fileHash">文件内容哈希。</param>
    /// <param name="language">语言标识。</param>
    /// <param name="chunkCount">切块数量。</param>
    /// <param name="workspaceId">目标工作区标识；为 null 时由实现方按当前工作区解析。</param>
    Task<long> UpsertFileAsync(string filePath, string fileHash, string language, int chunkCount, string? workspaceId = null);

    /// <summary>
    /// 软删除文件及其全部切块
    /// </summary>
    /// <param name="fileId">文件台账 Id。</param>
    /// <param name="workspaceId">目标工作区标识；为 null 时由实现方按当前工作区解析。</param>
    Task SoftDeleteFileAsync(long fileId, string? workspaceId = null);

    /// <summary>
    /// 获取文件已存储的切块（增量比对用）
    /// </summary>
    /// <param name="fileId">文件台账 Id。</param>
    /// <param name="workspaceId">目标工作区标识；为 null 时由实现方按当前工作区解析。</param>
    Task<IReadOnlyList<StoredChunk>> GetFileChunksAsync(long fileId, string? workspaceId = null);

    /// <summary>
    /// 整体替换文件的全部切块。ContentHash 由实现方计算（SHA256 hex of UTF8 content）
    /// </summary>
    /// <param name="fileId">文件台账 Id。</param>
    /// <param name="modelId">嵌入模型标识。</param>
    /// <param name="chunks">切块与向量集合。</param>
    /// <param name="workspaceId">目标工作区标识；为 null 时由实现方按当前工作区解析。</param>
    Task ReplaceFileChunksAsync(long fileId, string modelId, IReadOnlyList<ChunkVectorPair> chunks, string? workspaceId = null);

    /// <summary>
    /// 加载向量（可按路径前缀/语言预过滤）
    /// </summary>
    /// <param name="pathPrefix">路径前缀过滤；为空表示不限。</param>
    /// <param name="language">语言过滤；为空表示不限。</param>
    /// <param name="maxResults">返回上限；实现方需保证返回顺序稳定（按 Id 升序）。</param>
    /// <param name="workspaceId">目标工作区标识；为 null 时由实现方按当前工作区解析。</param>
    Task<IReadOnlyList<VectorEntry>> LoadVectorsAsync(string? pathPrefix = null, string? language = null, int maxResults = int.MaxValue, string? workspaceId = null);

    /// <summary>
    /// 按 Id 取切块内容（含文件路径/语言）
    /// </summary>
    /// <param name="chunkIds">切块 Id 集合。</param>
    /// <param name="workspaceId">目标工作区标识；为 null 时由实现方按当前工作区解析。</param>
    Task<Dictionary<long, CodeChunk>> GetChunksAsync(IReadOnlyList<long> chunkIds, string? workspaceId = null);

    /// <summary>
    /// 存储统计
    /// </summary>
    /// <param name="workspaceId">目标工作区标识；为 null 时由实现方按当前工作区解析。</param>
    Task<StoreStats> GetStatsAsync(string? workspaceId = null);
}
