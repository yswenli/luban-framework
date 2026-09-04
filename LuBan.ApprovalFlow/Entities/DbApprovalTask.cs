/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Entities
*文件名： DbApprovalTask.cs
*版本号： V1.0.0.0
*唯一标识：3f3adad3-868d-494a-a6a6-17f0f1d8bf14
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：DbApprovalTask 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：DbApprovalTask 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Entities;

[SugarTable("db_approval_task", "审批待办任务")]
public class DbApprovalTask : EntityDataScoreBase
{
    /// <summary>
    /// 流程记录ID
    /// </summary>
    [SugarColumn(ColumnDescription = "流程记录ID")]
    public long RecordId { get; set; }

    /// <summary>
    /// 节点记录ID
    /// </summary>
    [SugarColumn(ColumnDescription = "节点记录ID")]
    public long NodeRecordId { get; set; }

    /// <summary>
    /// 节点ID
    /// </summary>
    [SugarColumn(ColumnDescription = "节点ID", Length = 32)]
    [MaxLength(32)]
    public string NodeId { get; set; }

    /// <summary>
    /// 任务状态
    /// </summary>
    [SugarColumn(ColumnDescription = "任务状态", DefaultValue = "0")]
    public int Status { get; set; } = 0;

    /// <summary>
    /// 处理人用户ID
    /// </summary>
    [SugarColumn(ColumnDescription = "处理人用户ID")]
    public long AssigneeUserId { get; set; }

    /// <summary>
    /// 处理人姓名
    /// </summary>
    [SugarColumn(ColumnDescription = "处理人姓名", Length = 32)]
    [MaxLength(32)]
    public string? AssigneeName { get; set; }

    /// <summary>
    /// 原处理人ID(转办时)
    /// </summary>
    [SugarColumn(ColumnDescription = "原处理人ID(转办时)")]
    public long? OriginalAssigneeId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [SugarColumn(ColumnDescription = "创建时间")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 完成时间
    /// </summary>
    [SugarColumn(ColumnDescription = "完成时间")]
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// 是否已读
    /// </summary>
    [SugarColumn(ColumnDescription = "是否已读", DefaultValue = "0")]
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// 是否委托任务
    /// </summary>
    [SugarColumn(ColumnDescription = "是否委托任务", DefaultValue = "0")]
    public bool IsDelegated { get; set; } = false;

    /// <summary>
    /// 委托人ID
    /// </summary>
    [SugarColumn(ColumnDescription = "委托人ID")]
    public long? DelegatorId { get; set; }
}