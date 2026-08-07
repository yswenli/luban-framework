/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Orchestration
*文件名： IOrchestrator
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：编排器接口，作为对外统一入口
*
*****************************************************************************/
using LuBan.AIAgent.Orchestration.Models;

namespace LuBan.AIAgent.Orchestration;

/// <summary>
/// 编排器接口，作为对外统一入口，串联 Planner、Scheduler 与结果聚合。
/// </summary>
public interface IOrchestrator
{
    /// <summary>
    /// 执行复合任务编排。
    /// </summary>
    /// <param name="task">用户的复合任务描述。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>编排结果。</returns>
    Task<OrchestrationResult> RunAsync(string task, CancellationToken cancellationToken = default);

    /// <summary>
    /// 执行预计算的任务图谱（跳过规划阶段）。
    /// </summary>
    /// <param name="graph">预计算的任务图谱。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>编排结果。</returns>
    Task<OrchestrationResult> RunAsync(TaskGraph graph, CancellationToken cancellationToken = default);

    /// <summary>
    /// 流式执行，实时推送节点进度事件。
    /// </summary>
    /// <param name="task">用户的复合任务描述。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>进度事件流。</returns>
    IAsyncEnumerable<OrchestrationProgress> RunStreamingAsync(
        string task,
        CancellationToken cancellationToken = default);
}
