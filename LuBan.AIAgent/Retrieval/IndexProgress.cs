/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Retrieval
*文件名： IndexProgress
*版本号： V1.0.0.0
*唯一标识：2c1b0d24-7f7a-4a6e-9c31-5c8a4b2f7d10
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/21
*描述：索引进度快照
*
*=================================================
*修改标记
*修改时间：2026/9/21
*修改人： yswenli
*版本号： V1.0.0.0
*描述：索引进度快照
*
*****************************************************************************/
namespace LuBan.AIAgent.Retrieval;

/// <summary>
/// 索引阶段
/// </summary>
public enum IndexStage
{
    /// <summary>枚举待索引文件</summary>
    Scanning,
    /// <summary>提取、切块与嵌入</summary>
    Embedding,
    /// <summary>软删除失联文件</summary>
    Deleting,
    /// <summary>完成</summary>
    Done
}

/// <summary>
/// 索引进度快照
/// </summary>
/// <param name="Stage">当前阶段</param>
/// <param name="Done">已完成数量</param>
/// <param name="Total">总量（枚举完成后才有意义）</param>
/// <param name="CurrentFile">当前处理的文件路径</param>
public sealed record IndexProgress(IndexStage Stage, int Done, int Total, string? CurrentFile = null);