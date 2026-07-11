namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 工具执行门控接口，用于评估工具执行的审批请求
/// </summary>
public interface IToolExecutionGate
{
    /// <summary>
    /// 异步评估工具审批请求
    /// </summary>
    /// <param name="request">工具审批请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>工具审批决策</returns>
    Task<ToolApprovalDecision> EvaluateAsync(ToolApprovalRequest request, CancellationToken cancellationToken = default);
}
