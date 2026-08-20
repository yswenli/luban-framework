/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.LocalMemory
*文件名： MemoryEntry
*版本号： V1.0.0.0
*唯一标识：a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/4
*描述：本地长期记忆条目
*
*=================================================
*修改标记
*修改时间：2026/8/4
*修改人： yswenli
*版本号： V1.0.0.0
*描述：本地长期记忆条目
*
*****************************************************************************/
namespace LuBan.AIAgent.LocalMemory;

/// <summary>
/// 本地长期记忆条目
/// </summary>
public class MemoryEntry
{
    /// <summary>
    /// 记忆 ID
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 记忆内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 记忆类别，如 fact、preference、todo、project 等
    /// </summary>
    public string Category { get; set; } = "general";

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 向量维度（仅用于校验）
    /// </summary>
    public int VectorDimension { get; set; }

    /// <summary>
    /// 工作区 ID（NULL=全局，跨工作区可见）
    /// </summary>
    public string? WorkspaceId { get; set; }

    /// <summary>
    /// 规范化内容的 SHA256（用于去重）
    /// </summary>
    public string ContentHash { get; set; } = string.Empty;

    /// <summary>
    /// 过期时间（NULL=永不过期）
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// 搜索结果记忆条目，包含相似度分数
/// </summary>
public class MemorySearchResult : MemoryEntry
{
    /// <summary>
    /// 与查询的相似度分数（余弦相似度）
    /// </summary>
    public double Score { get; set; }
}
