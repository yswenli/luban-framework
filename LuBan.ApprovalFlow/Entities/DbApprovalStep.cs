namespace LuBan.ApprovalFlow.Entities;

[SugarTable("db_approval_step", "审批步骤记录")]
public class DbApprovalStep : EntityDataScoreBase
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
    /// 步骤序号
    /// </summary>
    [SugarColumn(ColumnDescription = "步骤序号")]
    public int StepNo { get; set; }

    /// <summary>
    /// 操作类型
    /// </summary>
    [SugarColumn(ColumnDescription = "操作类型", Length = 16)]
    [MaxLength(16)]
    public string ActionType { get; set; }

    /// <summary>
    /// 操作人用户ID
    /// </summary>
    [SugarColumn(ColumnDescription = "操作人用户ID")]
    public long OperatorUserId { get; set; }

    /// <summary>
    /// 操作人姓名
    /// </summary>
    [SugarColumn(ColumnDescription = "操作人姓名", Length = 32)]
    [MaxLength(32)]
    public string? OperatorName { get; set; }

    /// <summary>
    /// 操作时间
    /// </summary>
    [SugarColumn(ColumnDescription = "操作时间")]
    public DateTime OperatedAt { get; set; }

    /// <summary>
    /// 审批意见
    /// </summary>
    [SugarColumn(ColumnDescription = "审批意见", Length = 512)]
    [MaxLength(512)]
    public string? Comment { get; set; }

    /// <summary>
    /// 携带数据JSON
    /// </summary>
    [SugarColumn(ColumnDescription = "携带数据JSON", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? DataJson { get; set; }
}