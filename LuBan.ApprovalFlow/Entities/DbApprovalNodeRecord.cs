namespace LuBan.ApprovalFlow.Entities;

[SugarTable("db_approval_node_record", "审批节点记录")]
public class DbApprovalNodeRecord : EntityDataScoreBase
{
    /// <summary>
    /// 流程记录ID
    /// </summary>
    [SugarColumn(ColumnDescription = "流程记录ID")]
    public long RecordId { get; set; }

    /// <summary>
    /// 节点ID
    /// </summary>
    [SugarColumn(ColumnDescription = "节点ID", Length = 32)]
    [MaxLength(32)]
    public string NodeId { get; set; }

    /// <summary>
    /// 节点名称
    /// </summary>
    [SugarColumn(ColumnDescription = "节点名称", Length = 64)]
    [MaxLength(64)]
    public string? NodeName { get; set; }

    /// <summary>
    /// 节点类型
    /// </summary>
    [SugarColumn(ColumnDescription = "节点类型", Length = 16)]
    [MaxLength(16)]
    public string NodeType { get; set; }

    /// <summary>
    /// 节点状态
    /// </summary>
    [SugarColumn(ColumnDescription = "节点状态", DefaultValue = "0")]
    public int Status { get; set; } = 0;

    /// <summary>
    /// 进入时间
    /// </summary>
    [SugarColumn(ColumnDescription = "进入时间")]
    public DateTime? EnteredAt { get; set; }

    /// <summary>
    /// 离开时间
    /// </summary>
    [SugarColumn(ColumnDescription = "离开时间")]
    public DateTime? LeftAt { get; set; }

    /// <summary>
    /// 处理时长(秒)
    /// </summary>
    [SugarColumn(ColumnDescription = "处理时长(秒)")]
    public int? DurationSeconds { get; set; }

    /// <summary>
    /// 节点配置JSON
    /// </summary>
    [SugarColumn(ColumnDescription = "节点配置JSON", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? NodeConfigJson { get; set; }

    /// <summary>
    /// 节点结果JSON
    /// </summary>
    [SugarColumn(ColumnDescription = "节点结果JSON", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? NodeResultJson { get; set; }

    /// <summary>
    /// 已审批人数（会签）
    /// </summary>
    [SugarColumn(ColumnDescription = "已审批人数", DefaultValue = "0")]
    public int ApprovedCount { get; set; } = 0;

    /// <summary>
    /// 已拒绝人数（会签）
    /// </summary>
    [SugarColumn(ColumnDescription = "已拒绝人数", DefaultValue = "0")]
    public int RejectedCount { get; set; } = 0;

    /// <summary>
    /// 总审批人数（会签）
    /// </summary>
    [SugarColumn(ColumnDescription = "总审批人数", DefaultValue = "0")]
    public int TotalCount { get; set; } = 0;

    /// <summary>
    /// 聚合类型（会签）
    /// </summary>
    [SugarColumn(ColumnDescription = "聚合类型", Length = 32)]
    [MaxLength(32)]
    public string? AggregationType { get; set; }

    /// <summary>
    /// 聚合结果（会签）
    /// </summary>
    [SugarColumn(ColumnDescription = "聚合结果", Length = 16)]
    [MaxLength(16)]
    public string? AggregationResult { get; set; }
}