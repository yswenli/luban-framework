namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 需要审批属性，用于标记工具需要审批
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class NeedApprovalAttribute : Attribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="approvalMode">审批模式，默认为每次执行都需要审批</param>
    public NeedApprovalAttribute(ToolApprovalMode approvalMode = ToolApprovalMode.PerExecution)
    {
        ApprovalMode = approvalMode;
    }

    /// <summary>
    /// 审批模式
    /// </summary>
    public ToolApprovalMode ApprovalMode { get; }
}
