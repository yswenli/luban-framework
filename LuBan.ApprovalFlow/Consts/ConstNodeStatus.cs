namespace LuBan.ApprovalFlow.Consts;

/// <summary>
/// 节点状态常量，用于定义流程节点的状态
/// </summary>
public class ConstNodeStatus
{
    /// <summary>
    /// 未开始
    /// </summary>
    public const string NotStarted = "未开始";

    /// <summary>
    /// 待处理
    /// </summary>
    public const string Pending = "待处理";

    /// <summary>
    /// 处理中
    /// </summary>
    public const string Processing = "处理中";

    /// <summary>
    /// 已审批通过
    /// </summary>
    public const string Approved = "已审批";

    /// <summary>
    /// 已拒绝
    /// </summary>
    public const string Rejected = "已拒绝";

    /// <summary>
    /// 已退回
    /// </summary>
    public const string Returned = "已退回";

    /// <summary>
    /// 已取消
    /// </summary>
    public const string Cancelled = "已取消";

    /// <summary>
    /// 已跳过
    /// </summary>
    public const string Skipped = "已跳过";

    /// <summary>
    /// 已撤回
    /// </summary>
    public const string Withdrawn = "已撤回";
}