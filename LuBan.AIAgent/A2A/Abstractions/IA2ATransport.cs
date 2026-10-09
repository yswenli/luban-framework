/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.A2A.Abstractions
*文件名： IA2ATransport
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/8
*描述：A2A 传输抽象，统一进程内与远程调用
*
*****************************************************************************/
using LuBan.AIAgent.A2A.Models;

namespace LuBan.AIAgent.A2A.Abstractions;

/// <summary>
/// A2A 传输抽象。统一「进程内直连」与「远程 HTTP/JSON-RPC」两种传输方式，
/// 使上层（子代理执行器、UI 桥接）无需感知具体实现。
/// </summary>
public interface IA2ATransport
{
    /// <summary>
    /// 发送消息并等待任务终态（非流式）。
    /// </summary>
    /// <param name="agentName">目标 Agent 名称。</param>
    /// <param name="message">消息。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>任务终态。</returns>
    Task<A2ATask> SendAsync(string agentName, A2AMessage message, CancellationToken ct = default);

    /// <summary>
    /// 发送消息并流式接收事件。
    /// </summary>
    /// <param name="agentName">目标 Agent 名称。</param>
    /// <param name="message">消息。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>A2A 流式事件。</returns>
    IAsyncEnumerable<A2AStreamEvent> SendStreamingAsync(string agentName, A2AMessage message, CancellationToken ct = default);

    /// <summary>
    /// 获取任务状态。
    /// </summary>
    /// <param name="agentName">目标 Agent 名称。</param>
    /// <param name="taskId">任务标识。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>任务；不存在时为 null。</returns>
    Task<A2ATask?> GetTaskAsync(string agentName, string taskId, CancellationToken ct = default);

    /// <summary>
    /// 列出目标 Agent 的任务。
    /// </summary>
    /// <param name="agentName">目标 Agent 名称。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>任务列表。</returns>
    Task<IReadOnlyList<A2ATask>> ListTasksAsync(string agentName, CancellationToken ct = default);

    /// <summary>
    /// 取消任务。
    /// </summary>
    /// <param name="agentName">目标 Agent 名称。</param>
    /// <param name="taskId">任务标识。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>取消后的任务；不存在时为 null。</returns>
    Task<A2ATask?> CancelAsync(string agentName, string taskId, CancellationToken ct = default);

    /// <summary>
    /// 订阅指定任务的后续事件（用于断线重连）。
    /// </summary>
    /// <param name="agentName">目标 Agent 名称。</param>
    /// <param name="taskId">任务标识。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>A2A 流式事件。</returns>
    IAsyncEnumerable<A2AStreamEvent> SubscribeAsync(string agentName, string taskId, CancellationToken ct = default);
}