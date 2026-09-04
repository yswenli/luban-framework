/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Entities
*文件名： DbApprovalStatistics.cs
*版本号： V1.0.0.0
*唯一标识：f577e8aa-bda3-449c-b1a5-d227b071bae7
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：DbApprovalStatistics 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：DbApprovalStatistics 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Entities;

[SugarTable("db_approval_statistics", "审批统计")]
public class DbApprovalStatistics : EntityDataScoreBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [SugarColumn(ColumnDescription = "用户ID")]
    public long UserId { get; set; }

    /// <summary>
    /// 发起流程数
    /// </summary>
    [SugarColumn(ColumnDescription = "发起流程数", DefaultValue = "0")]
    public int InitiatedCount { get; set; } = 0;

    /// <summary>
    /// 审批通过数
    /// </summary>
    [SugarColumn(ColumnDescription = "审批通过数", DefaultValue = "0")]
    public int ApprovedCount { get; set; } = 0;

    /// <summary>
    /// 审批拒绝数
    /// </summary>
    [SugarColumn(ColumnDescription = "审批拒绝数", DefaultValue = "0")]
    public int RejectedCount { get; set; } = 0;

    /// <summary>
    /// 转办次数
    /// </summary>
    [SugarColumn(ColumnDescription = "转办次数", DefaultValue = "0")]
    public int TransferredCount { get; set; } = 0;

    /// <summary>
    /// 委托次数
    /// </summary>
    [SugarColumn(ColumnDescription = "委托次数", DefaultValue = "0")]
    public int DelegatedCount { get; set; } = 0;

    /// <summary>
    /// 更新时间
    /// </summary>
    [SugarColumn(ColumnDescription = "更新时间")]
    public DateTime? UpdatedAt { get; set; }
}