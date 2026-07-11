namespace LuBan.ApprovalFlow.Entities;

[SugarTable("db_approval_statistics", "审批统计")]
public class DbApprovalStatistics : EntityeDataScoreBase
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