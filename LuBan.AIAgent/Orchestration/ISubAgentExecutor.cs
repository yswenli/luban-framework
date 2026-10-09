/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Orchestration
*文件名： ISubAgentExecutor
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/8
*描述：子代理执行器抽象。DagScheduler 经此接缝执行节点，屏蔽底层是直接创建子代理还是经 A2A 进程内传输
*
*****************************************************************************/
using LuBan.AIAgent.Orchestration.Models;

namespace LuBan.AIAgent.Orchestration;

/// <summary>
/// 子代理执行器抽象。默认实现为 <see cref="SubAgentFactory"/>（直接创建子代理）；
/// 启用 A2A 后可由进程内传输实现替换，DagScheduler 无需感知差异。
/// 产出框架原生的 <see cref="SubAgentExecutionEvent"/> 流，不泄漏任何 A2A/SDK 类型。
/// </summary>
public interface ISubAgentExecutor
{
    /// <summary>
    /// 按规格执行子代理，流式产出执行事件。
    /// 事件流以 <see cref="SubAgentEventKind.Started"/> 开始，以
    /// <see cref="SubAgentEventKind.Completed"/> 或 <see cref="SubAgentEventKind.Failed"/> 结束。
    /// </summary>
    /// <param name="spec">子代理规格。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>执行事件流。</returns>
    IAsyncEnumerable<SubAgentExecutionEvent> ExecuteAsync(SubAgentSpec spec, CancellationToken ct = default);
}