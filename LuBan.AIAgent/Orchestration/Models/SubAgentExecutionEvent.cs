/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Orchestration.Models
*文件名： SubAgentExecutionEvent
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/8
*描述：子代理执行事件，作为 ISubAgentExecutor 的流式产出，框架原生、不泄漏任何 A2A/SDK 类型
*
*****************************************************************************/
namespace LuBan.AIAgent.Orchestration.Models;

/// <summary>
/// 子代理执行过程中的事件种类。事件流以 <see cref="Started"/> 开始，
/// 以 <see cref="Completed"/> 或 <see cref="Failed"/> 结束。
/// </summary>
public enum SubAgentEventKind
{
    /// <summary>子代理已创建，<see cref="SubAgentExecutionEvent.SessionId"/> 可用。</summary>
    Started = 0,

    /// <summary>思考增量，<see cref="SubAgentExecutionEvent.Text"/> 为增量文本。</summary>
    Thinking = 1,

    /// <summary>工具调用，<see cref="SubAgentExecutionEvent.Text"/> 为工具名，<see cref="SubAgentExecutionEvent.Detail"/> 为参数摘要。</summary>
    ToolCall = 2,

    /// <summary>工具结果，<see cref="SubAgentExecutionEvent.Text"/> 为结果摘要（失败时为错误信息）。</summary>
    ToolResult = 3,

    /// <summary>正文增量，<see cref="SubAgentExecutionEvent.Text"/> 为增量文本。</summary>
    Text = 4,

    /// <summary>执行完成，<see cref="SubAgentExecutionEvent.Text"/> 为节点最终输出。</summary>
    Completed = 5,

    /// <summary>执行失败，<see cref="SubAgentExecutionEvent.Text"/> 为错误信息。</summary>
    Failed = 6
}

/// <summary>
/// 子代理执行事件。由 <see cref="ISubAgentExecutor"/> 逐条产出，<see cref="DagScheduler"/>
/// 消费后转发为 <see cref="OrchestrationProgress"/> 的节点活动与结果。
/// 仅携带框架原生数据类型，不泄漏任何 A2A/SDK 类型。
/// </summary>
/// <param name="Kind">事件种类。</param>
/// <param name="SessionId">子代理会话标识；<see cref="SubAgentEventKind.Started"/> 起可用。</param>
/// <param name="Text">文本载荷，含义随 <paramref name="Kind"/> 而定。</param>
/// <param name="Detail">附加说明（如工具调用参数摘要）。</param>
public readonly record struct SubAgentExecutionEvent(
    SubAgentEventKind Kind,
    string? SessionId,
    string? Text,
    string? Detail);