namespace LuBan.ApprovalFlow.Consts;

/// <summary>
/// 操作类型常量，用于定义审批任务可执行的操作
/// </summary>
public class ConstActionType
{
    /// <summary>
    /// 同意审批
    /// </summary>
    public const string Approve = "approve";

    /// <summary>
    /// 拒绝审批
    /// </summary>
    public const string Reject = "reject";

    /// <summary>
    /// 退回到指定节点
    /// </summary>
    public const string Return = "return";

    /// <summary>
    /// 取消审批流程
    /// </summary>
    public const string Cancel = "cancel";

    /// <summary>
    /// 撤回已提交的审批
    /// </summary>
    public const string Withdraw = "withdraw";

    /// <summary>
    /// 转办给其他人处理
    /// </summary>
    public const string Transfer = "transfer";

    /// <summary>
    /// 委托给代理人处理
    /// </summary>
    public const string Delegate = "delegate";
}