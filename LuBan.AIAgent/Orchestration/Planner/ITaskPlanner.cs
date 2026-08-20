/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Orchestration.Planner
*文件名： ITaskPlanner
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：任务规划器接口，将自然语言任务转换为 TaskGraph
*
*****************************************************************************/
using LuBan.AIAgent.Orchestration.Models;

namespace LuBan.AIAgent.Orchestration.Planner;

/// <summary>
/// 任务规划器接口，将自然语言任务转换为 TaskGraph。
/// </summary>
public interface ITaskPlanner
{
    /// <summary>
    /// 将自然语言任务转换为 TaskGraph。
    /// </summary>
    /// <param name="task">用户任务描述。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>TaskGraph 实例；模板未命中时返回 null（由 CompositeTaskPlanner 回退）。</returns>
    Task<TaskGraph?> PlanAsync(string task, CancellationToken ct = default);

    /// <summary>
    /// 分析执行失败的关键节点，决定是否重规划并生成修正节点。
    /// </summary>
    /// <param name="context">反思上下文。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>反思结果，包含分析和修正建议。</returns>
    Task<ReflectionResult> ReflectAsync(ReplanContext context, CancellationToken ct = default);
}
