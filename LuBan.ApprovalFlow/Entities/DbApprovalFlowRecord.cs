/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm.Entities
*文件名： DbApprovalFlowRecord
*版本号： V1.0.0.0
*唯一标识：c7cadd4e-b44e-4ec3-b4b2-ed65f7b4ddf4
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/22 14:33:09
*描述：审批流流程记录
*
*=================================================
*修改标记
*修改时间：2025/10/22 14:33:09
*修改人： yswenli
*版本号： V1.0.0.0
*描述：审批流流程记录
*
*****************************************************************************/
namespace LuBan.ApprovalFlow.Entities;

/// <summary>
/// 审批流流程记录
/// </summary>
[SugarTable("db_approval_flow_record", "审批流流程记录")]
public class DbApprovalFlowRecord : EntityDataScoreBase
{
    /// <summary>
    /// 流程键
    /// </summary>
    [SugarColumn(ColumnDescription = "流程键")]
    public long ApprovalFlowId { get; set; }

    /// <summary>
    /// 流程结构
    /// </summary>
    [SugarColumn(ColumnDescription = "流程结构", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? FlowJson { get; set; }

    /// <summary>
    /// 流程结果
    /// </summary>
    [SugarColumn(ColumnDescription = "流程结果", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? FlowResult { get; set; }

    /// <summary>
    /// 流程版本
    /// </summary>
    [SugarColumn(ColumnDescription = "流程版本")]
    public int FlowVersion { get; set; }

    /// <summary>
    /// 流程编码
    /// </summary>
    [SugarColumn(ColumnDescription = "流程编码", Length = 32)]
    [MaxLength(32)]
    public string? FlowCode { get; set; }

    /// <summary>
    /// 当前节点ID
    /// </summary>
    [SugarColumn(ColumnDescription = "当前节点ID", Length = 32)]
    [MaxLength(32)]
    public string? CurrentNodeId { get; set; }

    /// <summary>
    /// 业务主键
    /// </summary>
    [SugarColumn(ColumnDescription = "业务主键", Length = 64)]
    [MaxLength(64)]
    public string? BusinessKey { get; set; }

    /// <summary>
    /// 发起人用户ID
    /// </summary>
    [SugarColumn(ColumnDescription = "发起人用户ID")]
    public long InitiatorUserId { get; set; }

    /// <summary>
    /// 发起人姓名
    /// </summary>
    [SugarColumn(ColumnDescription = "发起人姓名", Length = 32)]
    [MaxLength(32)]
    public string? InitiatorName { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    [SugarColumn(ColumnDescription = "开始时间")]
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// 完成时间
    /// </summary>
    [SugarColumn(ColumnDescription = "完成时间")]
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// 流程状态
    /// </summary>
    [SugarColumn(ColumnDescription = "流程状态", DefaultValue = "0")]
    public int Status { get; set; } = 0;

    /// <summary>
    /// 流程变量JSON
    /// </summary>
    [SugarColumn(ColumnDescription = "流程变量JSON", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? VariablesJson { get; set; }
}