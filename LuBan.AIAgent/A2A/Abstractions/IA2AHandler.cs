/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.A2A.Abstractions
*文件名： IA2AHandler
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/8
*描述：A2A 处理句柄抽象，不泄漏 SDK 类型，由 agent 侧映射到 SDK IAgentHandler
*
*****************************************************************************/
using LuBan.AIAgent.A2A.Models;

namespace LuBan.AIAgent.A2A.Abstractions;

/// <summary>
/// A2A 处理句柄。框架侧句柄抽象，代表「一个可接收 A2A 消息并产出流式事件的实体」。
/// 由 <c>LubanAgentA2A</c> 映射为 SDK <c>IAgentHandler</c>。
/// </summary>
public interface IA2AHandler
{
    /// <summary>
    /// 处理一条入站消息，流式产出 A2A 事件。
    /// </summary>
    /// <param name="message">入站消息。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>A2A 流式事件。</returns>
    IAsyncEnumerable<A2AStreamEvent> ExecuteAsync(A2AMessage message, CancellationToken ct = default);

    /// <summary>
    /// 取消指定任务。
    /// </summary>
    /// <param name="taskId">任务标识。</param>
    /// <param name="ct">取消令牌。</param>
    Task CancelAsync(string taskId, CancellationToken ct = default);
}