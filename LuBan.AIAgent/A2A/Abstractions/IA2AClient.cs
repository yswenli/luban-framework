/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.A2A.Abstractions
*文件名： IA2AClient
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/8
*描述：A2A 远程客户端抽象，用于以客户端角色调用远端 Agent
*
*****************************************************************************/
using LuBan.AIAgent.A2A.Models;

namespace LuBan.AIAgent.A2A.Abstractions;

/// <summary>
/// A2A 远程客户端抽象。代表一个已配置的远端 Agent 连接，
/// 由 <c>RemoteSubAgentExecutor</c> 使用（客户端角色调用远端服务端）。
/// </summary>
public interface IA2AClient
{
    /// <summary>
    /// 获取客户端逻辑名称（用于注册为本地 SubAgent 角色，如 "remote-analyst"）。
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 获取远端端点基址。
    /// </summary>
    string Endpoint { get; }

    /// <summary>
    /// 拉取并解析远端 AgentCard。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    /// <returns>远端描述符；不可达时为 null。</returns>
    Task<A2AAgentDescriptor?> ResolveCardAsync(CancellationToken ct = default);

    /// <summary>
    /// 发送消息并等待任务终态（非流式）。
    /// </summary>
    /// <param name="message">消息。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>任务终态。</returns>
    Task<A2ATask> SendAsync(A2AMessage message, CancellationToken ct = default);

    /// <summary>
    /// 发送消息并流式接收事件。
    /// </summary>
    /// <param name="message">消息。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>A2A 流式事件。</returns>
    IAsyncEnumerable<A2AStreamEvent> SendStreamingAsync(A2AMessage message, CancellationToken ct = default);

    /// <summary>
    /// 获取任务状态。
    /// </summary>
    /// <param name="taskId">任务标识。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>任务；不存在时为 null。</returns>
    Task<A2ATask?> GetTaskAsync(string taskId, CancellationToken ct = default);

    /// <summary>
    /// 取消任务。
    /// </summary>
    /// <param name="taskId">任务标识。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>取消后的任务；不存在时为 null。</returns>
    Task<A2ATask?> CancelAsync(string taskId, CancellationToken ct = default);
}