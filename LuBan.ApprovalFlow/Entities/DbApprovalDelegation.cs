/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Entities
*文件名： DbApprovalDelegation.cs
*版本号： V1.0.0.0
*唯一标识：f46df2e1-83cd-4f60-9c00-c4383cd36093
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：DbApprovalDelegation 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：DbApprovalDelegation 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Entities;

[SugarTable("db_approval_delegation", "审批委托配置")]
public class DbApprovalDelegation : EntityDataScoreBase
{
    /// <summary>
    /// 委托人ID
    /// </summary>
    [SugarColumn(ColumnDescription = "委托人ID")]
    public long DelegatorId { get; set; }

    /// <summary>
    /// 托人姓名
    /// </summary>
    [SugarColumn(ColumnDescription = "委托人姓名", Length = 32)]
    [MaxLength(32)]
    public string? DelegatorName { get; set; }

    /// <summary>
    /// 受托人ID
    /// </summary>
    [SugarColumn(ColumnDescription = "受托人ID")]
    public long DelegateeId { get; set; }

    /// <summary>
    /// 受托人姓名
    /// </summary>
    [SugarColumn(ColumnDescription = "受托人姓名", Length = 32)]
    [MaxLength(32)]
    public string? DelegateeName { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    [SugarColumn(ColumnDescription = "开始时间")]
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    [SugarColumn(ColumnDescription = "结束时间")]
    public DateTime EndTime { get; set; }

    /// <summary>
    /// 是否生效
    /// </summary>
    [SugarColumn(ColumnDescription = "是否生效", DefaultValue = "1")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 流程范围(空表示全部)
    /// </summary>
    [SugarColumn(ColumnDescription = "流程范围", Length = 256)]
    [MaxLength(256)]
    public string? FlowCodes { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [SugarColumn(ColumnDescription = "创建时间")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 取消时间
    /// </summary>
    [SugarColumn(ColumnDescription = "取消时间")]
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// 取消人ID
    /// </summary>
    [SugarColumn(ColumnDescription = "取消人ID")]
    public long? CancelledBy { get; set; }
}