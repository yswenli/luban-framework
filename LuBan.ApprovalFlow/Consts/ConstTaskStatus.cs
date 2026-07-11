namespace LuBan.ApprovalFlow.Consts;

/// <summary>
/// 任务状态常量，用于定义审批任务的状态
/// </summary>
public class ConstTaskStatus
{
    /// <summary>
    /// 待处理
    /// </summary>
    public const string Pending = "待处理";

    /// <summary>
    /// 处理中
    /// </summary>
    public const string Processing = "处理中";

    /// <summary>
    /// 已完成
    /// </summary>
    public const string Completed = "已完成";

    /// <summary>
    /// 已转办
    /// </summary>
    public const string Transferred = "已转办";

    /// <summary>
    /// 已委托
    /// </summary>
    public const string Delegated = "已委托";

    /// <summary>
    /// 已取消
    /// </summary>
    public const string Cancelled = "已取消";
}