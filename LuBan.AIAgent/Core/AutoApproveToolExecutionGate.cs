using LuBan.AIAgent.Abstractions;

namespace LuBan.AIAgent.Core;

/// <summary>
/// 自动批准工具执行门控，会自动批准所有的工具执行请求
/// </summary>
public sealed class AutoApproveToolExecutionGate : IToolExecutionGate
{
    /// <summary>
    /// 异步评估工具审批请求，总是返回批准的决策
    /// </summary>
    /// <param name="request">工具审批请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>批准的工具审批决策</returns>
    public Task<ToolApprovalDecision> EvaluateAsync(ToolApprovalRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(ToolApprovalDecision.ApprovedDecision(request.Id));
}
