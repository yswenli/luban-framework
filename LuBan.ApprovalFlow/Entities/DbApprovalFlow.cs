/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm.Entities
*文件名： DbApprovalFlow
*版本号： V1.0.0.0
*唯一标识：3752d7c6-9c1d-4074-9f48-e3047d41c669
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/22 14:31:39
*描述：审批流程信息表
*
*=================================================
*修改标记
*修改时间：2025/10/22 14:31:39
*修改人： yswenli
*版本号： V1.0.0.0
*描述：审批流程信息表
*
*****************************************************************************/
namespace LuBan.ApprovalFlow.Entities;

/// <summary>
/// 审批流程信息表
/// </summary>
[SugarTable("db_approval_flow", "审批流程信息表")]
public class DbApprovalFlow : EntityeDataScoreBase
{
    /// <summary>
    /// 编号
    /// </summary>
    [SugarColumn(ColumnDescription = "编号", Length = 32)]
    [MaxLength(32)]
    public string? Code { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [SugarColumn(ColumnDescription = "名称", Length = 32)]
    [MaxLength(32)]
    public string Name { get; set; }

    /// <summary>
    /// 流程
    /// </summary>
    [SugarColumn(ColumnDescription = "流程", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? FlowJson { get; set; }

    /// <summary>
    /// 表单定义JSON
    /// </summary>
    [SugarColumn(ColumnDescription = "表单定义JSON", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? FormJson { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public int? Status { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = 256)]
    [MaxLength(256)]
    public string? Remark { get; set; }

    /// <summary>
    /// 版本号
    /// </summary>
    [SugarColumn(ColumnDescription = "版本号", DefaultValue = "1")]
    public int Version { get; set; } = 1;

    /// <summary>
    /// 是否当前版本
    /// </summary>
    [SugarColumn(ColumnDescription = "是否当前版本", DefaultValue = "1")]
    public bool IsCurrent { get; set; } = true;

    /// <summary>
    /// 激活时间
    /// </summary>
    [SugarColumn(ColumnDescription = "激活时间")]
    public DateTime? ActivatedAt { get; set; }

    /// <summary>
    /// 激活人ID
    /// </summary>
    [SugarColumn(ColumnDescription = "激活人ID")]
    public long? ActivatedBy { get; set; }

    /// <summary>
    /// 变更日志
    /// </summary>
    [SugarColumn(ColumnDescription = "变更日志", Length = 512)]
    [MaxLength(512)]
    public string? ChangeLog { get; set; }

    /// <summary>
    /// 全局变量JSON
    /// </summary>
    [SugarColumn(ColumnDescription = "全局变量JSON", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? GlobalVariablesJson { get; set; }

    /// <summary>
    /// 事件配置JSON
    /// </summary>
    [SugarColumn(ColumnDescription = "事件配置JSON", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? EventsJson { get; set; }
}