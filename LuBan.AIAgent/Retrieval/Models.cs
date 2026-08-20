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
    public string FilePath { get; set; } = "";
    public int ChunkIndex { get; set; }
    public int StartLine { get; set; }
    public int EndLine { get; set; }
    public string ChunkType { get; set; } = "";
    public string? SymbolName { get; set; }
    public string Language { get; set; } = "";
    public string Content { get; set; } = "";
}

/// <summary>
/// 检索结果
/// </summary>
public class RetrievalResult
{
    public long ChunkId { get; set; }
    public string FilePath { get; set; } = "";
    public int StartLine { get; set; }
    public int EndLine { get; set; }
    public string ChunkType { get; set; } = "";
    public string? SymbolName { get; set; }
    public string Content { get; set; } = "";
    public double Score { get; set; }
}

/// <summary>
/// 向量条目
/// </summary>
public class VectorEntry
{
    public long ChunkId { get; set; }
    public float[] Vector { get; set; } = Array.Empty<float>();
}

/// <summary>
/// 已索引文件
/// </summary>
public class IndexedFile
{
    public long Id { get; set; }
    public string FilePath { get; set; } = "";
    public string FileHash { get; set; } = "";
    public string Language { get; set; } = "";
}

/// <summary>
/// 已存储切块（增量比对用）
/// </summary>
public class StoredChunk
{
    public long Id { get; set; }
    public int ChunkIndex { get; set; }
    public string ContentHash { get; set; } = "";
    public float[] Vector { get; set; } = Array.Empty<float>();
}

/// <summary>
/// 切块与向量对（写入用）
/// </summary>
public class ChunkVectorPair
{
    public CodeChunk Chunk { get; set; } = new();
    public float[] Vector { get; set; } = Array.Empty<float>();
}

/// <summary>
/// 索引报告
/// </summary>
public class IndexReport
{
    public int ScannedFiles { get; set; }
    public int NewFiles { get; set; }
    public int UpdatedFiles { get; set; }
    public int SkippedFiles { get; set; }
    public int DeletedFiles { get; set; }
    public int TotalChunks { get; set; }
    public int EmbeddedChunks { get; set; }
    public int ReusedChunks { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// 索引统计
/// </summary>
public class IndexStats
{
    public int TotalFiles { get; set; }
    public int TotalChunks { get; set; }
    public string? ModelId { get; set; }
    public int VectorDimension { get; set; }
}

/// <summary>
/// 存储统计
/// </summary>
public class StoreStats
{
    public int FileCount { get; set; }
    public int ChunkCount { get; set; }
    public string? ModelId { get; set; }
    public int Dimension { get; set; }
}
