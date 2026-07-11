namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 审批感知工具接口，用于标识工具的审批模式
/// </summary>
public interface IApprovalAwareTool
{
    /// <summary>
    /// 工具审批模式
    /// </summary>
    ToolApprovalMode ApprovalMode { get; }
}
