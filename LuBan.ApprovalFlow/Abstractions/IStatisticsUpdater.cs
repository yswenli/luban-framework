namespace LuBan.ApprovalFlow.Abstractions;

/// <summary>
/// 统计更新器接口，用于更新审批流程相关的统计数据
/// </summary>
public interface IStatisticsUpdater
{
    /// <summary>
    /// 更新发起人统计数据
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="category">流程分类</param>
    /// <param name="status">流程状态</param>
    Task UpdateInitiatorStatsAsync(long userId, string category, string status);

    /// <summary>
    /// 更新审批人统计数据
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="action">审批动作</param>
    Task UpdateApproverStatsAsync(long userId, string action);

    /// <summary>
    /// 任务创建时的统计更新
    /// </summary>
    /// <param name="userId">用户ID</param>
    Task OnTaskCreatedAsync(long userId);

    /// <summary>
    /// 任务转办时的统计更新
    /// </summary>
    /// <param name="fromUserId">原处理人ID</param>
    /// <param name="toUserId">新处理人ID</param>
    Task OnTaskTransferredAsync(long fromUserId, long toUserId);
}