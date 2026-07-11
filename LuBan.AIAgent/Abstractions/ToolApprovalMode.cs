namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 工具审批模式枚举，定义了工具执行的审批方式
/// </summary>
public enum ToolApprovalMode
{
    /// <summary>
    /// 不需要审批
    /// </summary>
    None = 0,
    
    /// <summary>
    /// 每次执行都需要审批
    /// </summary>
    PerExecution = 1
}
