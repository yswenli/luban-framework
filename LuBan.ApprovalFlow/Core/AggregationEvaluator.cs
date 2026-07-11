namespace LuBan.ApprovalFlow.Core;

/// <summary>
/// 会签聚合评估器：根据不同的聚合策略评估会签结果。
/// 支持全部通过、任一通过、多数通过、百分比通过等多种聚合模式。
/// </summary>
public class AggregationEvaluator
{
    /// <summary>
    /// 评估会签聚合结果。
    /// </summary>
    /// <param name="aggregationType">聚合类型，如全部通过、任一通过、多数通过、百分比通过等。</param>
    /// <param name="approvedCount">已通过数量。</param>
    /// <param name="rejectedCount">已拒绝数量。</param>
    /// <param name="totalCount">总数量。</param>
    /// <param name="approvePercentage">百分比通过模式下的通过阈值（默认60%）。</param>
    /// <returns>聚合结果：已通过、已拒绝或等待中。</returns>
    public AggregationResult Evaluate(
        string aggregationType,
        int approvedCount,
        int rejectedCount,
        int totalCount,
        int? approvePercentage = null)
    {
        // 总数为零时返回等待状态
        if (totalCount == 0)
            return AggregationResult.Pending;

        switch (aggregationType)
        {
            case ConstAggregationType.AutoApprove:
                // 自动通过模式：节点启动时自动通过，无需审批人操作
                return AggregationResult.Approved;

            case ConstAggregationType.AllApprove:
                // 全部通过模式：有人拒绝则拒绝，全部通过才通过
                if (rejectedCount > 0) return AggregationResult.Rejected;
                if (approvedCount == totalCount) return AggregationResult.Approved;
                return AggregationResult.Pending;

            case ConstAggregationType.AnyApprove:
                // 任一通过模式：有人通过则通过，全部拒绝才拒绝
                if (approvedCount > 0) return AggregationResult.Approved;
                if (rejectedCount == totalCount) return AggregationResult.Rejected;
                return AggregationResult.Pending;

            case ConstAggregationType.MajorityApprove:
                // 多数通过模式：超过半数通过或拒绝则相应返回
                var majorityThreshold = totalCount / 2 + 1;
                if (approvedCount >= majorityThreshold) return AggregationResult.Approved;
                if (rejectedCount >= majorityThreshold) return AggregationResult.Rejected;
                return AggregationResult.Pending;

            case ConstAggregationType.PercentageApprove:
                // 百分比通过模式：达到指定百分比则通过
                var percentage = approvePercentage ?? 60;
                var requiredCount = (int)Math.Ceiling(totalCount * percentage / 100.0);
                if (approvedCount >= requiredCount) return AggregationResult.Approved;
                // 拒绝数量超过剩余可审批数量则拒绝
                if (rejectedCount > totalCount - requiredCount) return AggregationResult.Rejected;
                return AggregationResult.Pending;

            default:
                return AggregationResult.Pending;
        }
    }
}