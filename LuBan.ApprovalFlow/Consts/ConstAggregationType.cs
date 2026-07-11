namespace LuBan.ApprovalFlow.Consts;

/// <summary>
/// 聚合类型常量，用于定义多审批人时的聚合策略
/// </summary>
public class ConstAggregationType
{
    /// <summary>
    /// 自动通过，节点启动时自动通过，无需审批人操作
    /// </summary>
    public const string AutoApprove = "auto_approve";

    /// <summary>
    /// 全部通过，所有审批人都需同意
    /// </summary>
    public const string AllApprove = "all_approve";

    /// <summary>
    /// 任一通过，任意一人同意即可
    /// </summary>
    public const string AnyApprove = "any_approve";

    /// <summary>
    /// 多数通过，超过半数同意即可
    /// </summary>
    public const string MajorityApprove = "majority_approve";

    /// <summary>
    /// 百分比通过，按设定百分比同意即可
    /// </summary>
    public const string PercentageApprove = "percentage_approve";

    /// <summary>
    /// 自定义聚合规则
    /// </summary>
    public const string Custom = "custom";
}