/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： FlowExecutionStats
*版本号： V1.0.0.0
*唯一标识：e6473019-8737-4b54-b296-72b5f454b1e0
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/30 19:01:51
*描述：流程运行统计信息
*
*=================================================
*修改标记
*修改时间：2025/10/30 19:01:51
*修改人： yswenli
*版本号： V1.0.0.0
*描述：流程运行统计信息
*
*****************************************************************************/
namespace LuBan.ApprovalFlow.Models;


/// <summary>
/// 流程运行统计信息
/// </summary>
public sealed class FlowExecutionStats
{
    /// <summary>流程 Key。</summary>
    public string Key { get; init; } = string.Empty;
    /// <summary>记录总数。</summary>
    public int TotalRecords { get; init; }
    /// <summary>已结束（含拒绝）记录数。</summary>
    public int FinishedRecords { get; init; }
    /// <summary>异常记录数。</summary>
    public int ExceptionRecords { get; init; }
    /// <summary>运行中记录数。</summary>
    public int RunningRecords => TotalRecords - FinishedRecords - ExceptionRecords;
    /// <summary>最近一次更新的时间。</summary>
    public DateTime? LastUpdated { get; init; }
    /// <summary>最近一次更新的记录ID。</summary>
    public long? LastRecordId { get; init; }
}