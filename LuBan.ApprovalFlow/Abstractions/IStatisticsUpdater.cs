/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Abstractions
*文件名： IStatisticsUpdater.cs
*版本号： V1.0.0.0
*唯一标识：a7d68cd5-cb45-45b1-8d4d-9f52316d3244
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：IStatisticsUpdater 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：IStatisticsUpdater 类
*
*****************************************************************************/

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