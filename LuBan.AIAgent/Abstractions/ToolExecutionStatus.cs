namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 工具执行状态枚举，定义了工具执行的不同状态
/// </summary>
public enum ToolExecutionStatus
{
    /// <summary>
    /// 执行完成
    /// </summary>
    Completed = 0,
    
    /// <summary>
    /// 执行被拒绝
    /// </summary>
    Denied = 1,
    
    /// <summary>
    /// 等待审批
    /// </summary>
    AwaitingApproval = 2,
    
    /// <summary>
    /// 执行失败
    /// </summary>
    Failed = 3
}
