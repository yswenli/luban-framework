/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Retrieval
*文件名： Models
*版本号： V1.0.0.0
*唯一标识：2d38cd82-826e-47f2-9e59-24f88b0e69c2
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：检索相关数据模型
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：检索相关数据模型
*
*****************************************************************************/
namespace LuBan.AIAgent.Retrieval;

/// <summary>
/// 文本切块
/// </summary>
public class CodeChunk
{
    /// <summary>
    /// 切块来源文件相对索引根目录的路径。
    /// </summary>
    public string FilePath { get; set; } = "";
    /// <summary>
    /// 切块在该文件内的序号，从 0 开始递增。
    /// </summary>
    public int ChunkIndex { get; set; }
    /// <summary>
    /// 切块在源文件中的起始行号。
    /// </summary>
    public int StartLine { get; set; }
    /// <summary>
    /// 切块在源文件中的结束行号，包含该行。
    /// </summary>
    public int EndLine { get; set; }
    /// <summary>
    /// 切分粒度类型，如函数、类型、段落。
    /// </summary>
    public string ChunkType { get; set; } = "";
    /// <summary>
    /// 切块对应的符号名（函数或类型名），无符号时为 null。
    /// </summary>
    public string? SymbolName { get; set; }
    /// <summary>
    /// 源文件的语言标识，如 csharp、python。
    /// </summary>
    public string Language { get; set; } = "";
    /// <summary>
    /// 切块的原始文本内容，用于嵌入与展示。
    /// </summary>
    public string Content { get; set; } = "";
}

/// <summary>
/// 检索结果
/// </summary>
public class RetrievalResult
{
    /// <summary>
    /// 命中切块在索引库中的唯一标识。
    /// </summary>
    public long ChunkId { get; set; }
    /// <summary>
    /// 命中切块所属文件的相对路径。
    /// </summary>
    public string FilePath { get; set; } = "";
    /// <summary>
    /// 命中切块在原文件中的起始行号。
    /// </summary>
    public int StartLine { get; set; }
    /// <summary>
    /// 命中切块在原文件中的结束行号，包含该行。
    /// </summary>
    public int EndLine { get; set; }
    /// <summary>
    /// 命中切块的切分粒度类型。
    /// </summary>
    public string ChunkType { get; set; } = "";
    /// <summary>
    /// 命中切块对应的符号名，无符号时为 null。
    /// </summary>
    public string? SymbolName { get; set; }
    /// <summary>
    /// 命中切块的文本内容。
    /// </summary>
    public string Content { get; set; } = "";
    /// <summary>
    /// 该条命中与查询的相关性得分，越大越相关。
    /// </summary>
    public double Score { get; set; }
}

/// <summary>
/// 向量条目
/// </summary>
public class VectorEntry
{
    /// <summary>
    /// 该向量所属切块的标识。
    /// </summary>
    public long ChunkId { get; set; }
    /// <summary>
    /// 切块对应的嵌入向量数值。
    /// </summary>
    public float[] Vector { get; set; } = Array.Empty<float>();
}

/// <summary>
/// 向量检索命中（存储侧 top-K 结果）
/// </summary>
/// <param name="ChunkId">切块 Id</param>
/// <param name="Score">余弦相似度</param>
public sealed record VectorHit(long ChunkId, double Score);

/// <summary>
/// 已索引文件
/// </summary>
public class IndexedFile
{
    /// <summary>
    /// 已索引文件在文件表中的主键标识。
    /// </summary>
    public long Id { get; set; }
    /// <summary>
    /// 已索引文件相对索引根目录的路径。
    /// </summary>
    public string FilePath { get; set; } = "";
    /// <summary>
    /// 文件内容哈希，用于判断是否需要重建索引。
    /// </summary>
    public string FileHash { get; set; } = "";
    /// <summary>
    /// 文件语言标识，决定切分策略。
    /// </summary>
    public string Language { get; set; } = "";
}

/// <summary>
/// 已存储切块（增量比对用）
/// </summary>
public class StoredChunk
{
    /// <summary>
    /// 已存储切块在切块表中的主键标识。
    /// </summary>
    public long Id { get; set; }
    /// <summary>
    /// 该切块在其文件内的序号。
    /// </summary>
    public int ChunkIndex { get; set; }
    /// <summary>
    /// 切块内容哈希，内容未变时可复用已有向量。
    /// </summary>
    public string ContentHash { get; set; } = "";
    /// <summary>
    /// 该切块已存储的嵌入向量。
    /// </summary>
    public float[] Vector { get; set; } = Array.Empty<float>();
}

/// <summary>
/// 切块与向量对（写入用）
/// </summary>
public class ChunkVectorPair
{
    /// <summary>
    /// 待写入索引库的切块对象。
    /// </summary>
    public CodeChunk Chunk { get; set; } = new();
    /// <summary>
    /// 与切块配对写入的嵌入向量。
    /// </summary>
    public float[] Vector { get; set; } = Array.Empty<float>();
}

/// <summary>
/// 索引报告
/// </summary>
public class IndexReport
{
    /// <summary>
    /// 本次索引扫描到的文件总数。
    /// </summary>
    public int ScannedFiles { get; set; }
    /// <summary>
    /// 首次入库并完成向量化的文件数。
    /// </summary>
    public int NewFiles { get; set; }
    /// <summary>
    /// 内容变化并重建索引的文件数。
    /// </summary>
    public int UpdatedFiles { get; set; }
    /// <summary>
    /// 哈希未变而跳过的文件数。
    /// </summary>
    public int SkippedFiles { get; set; }
    /// <summary>
    /// 已从索引中移除的失效文件数。
    /// </summary>
    public int DeletedFiles { get; set; }
    /// <summary>
    /// 本次索引涉及的切块总数。
    /// </summary>
    public int TotalChunks { get; set; }
    /// <summary>
    /// 本次新计算向量的切块数。
    /// </summary>
    public int EmbeddedChunks { get; set; }
    /// <summary>
    /// 复用已有向量而无需重算的切块数。
    /// </summary>
    public int ReusedChunks { get; set; }
    /// <summary>
    /// 索引过程中失败项的错误说明列表。
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// 警告（非失败但需提示，例如 .xlsx 提取超出单文件字符上限导致内容截断）
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// 索引统计
/// </summary>
public class IndexStats
{
    /// <summary>
    /// 索引库中的文件总数。
    /// </summary>
    public int TotalFiles { get; set; }
    /// <summary>
    /// 索引库中的切块总数。
    /// </summary>
    public int TotalChunks { get; set; }
    /// <summary>
    /// 生成现有向量所用的嵌入模型标识。
    /// </summary>
    public string? ModelId { get; set; }
    /// <summary>
    /// 索引向量的维度。
    /// </summary>
    public int VectorDimension { get; set; }
}

/// <summary>
/// 存储统计
/// </summary>
public class StoreStats
{
    /// <summary>
    /// 存储中的文件记录条数。
    /// </summary>
    public int FileCount { get; set; }
    /// <summary>
    /// 存储中的切块记录条数。
    /// </summary>
    public int ChunkCount { get; set; }
    /// <summary>
    /// 写入这些向量时使用的嵌入模型标识。
    /// </summary>
    public string? ModelId { get; set; }
    /// <summary>
    /// 存储中向量的维度。
    /// </summary>
    public int Dimension { get; set; }
}
