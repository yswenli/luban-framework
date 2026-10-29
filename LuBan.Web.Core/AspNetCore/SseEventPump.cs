/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Web.Core.AspNetCore
*文件名： SseEventPump
*版本号： V1.0.0.0
*唯一标识：b9d4e2a6-58c1-4f30-8a97-6d3b1e05c7f2
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/29
*描述：把事件帧序列泵到 SSE 响应，空闲时发注释心跳
*
*=================================================
*修改标记
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：把事件帧序列泵到 SSE 响应，空闲时发注释心跳
*
*****************************************************************************/

using LuBan.AIAgent.Protocol;

namespace LuBan.Web.Core.AspNetCore;

/// <summary>
/// 把事件帧序列泵到 SSE 响应，并在空闲超过阈值时发注释心跳。
/// </summary>
public static class SseEventPump
{
    /// <summary>泵送直到帧序列结束。</summary>
    /// <param name="frames">事件帧序列。</param>
    /// <param name="writer">SSE 写入器。</param>
    /// <param name="heartbeatInterval">空闲心跳间隔；小于等于零表示不心跳。</param>
    /// <param name="ct">取消令牌（客户端断开）。</param>
    /// <remarks>
    /// 同一时刻只允许一个挂起的 <c>MoveNextAsync</c>：心跳不能并发推进异步迭代器，
    /// 否则会抛「迭代器已在运行」。故心跳只写注释，然后继续等同一个 MoveNext 任务。
    /// </remarks>
    public static async Task PumpAsync(
        IAsyncEnumerable<AgentEventFrame> frames,
        SseEventWriter writer,
        TimeSpan heartbeatInterval,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(frames);
        ArgumentNullException.ThrowIfNull(writer);

        var enumerator = frames.GetAsyncEnumerator(ct);
        Task<bool>? pending = null;
        try
        {
            while (true)
            {
                // 复用同一个 MoveNext：心跳轮次不重新发起，避免并发推进迭代器
                pending ??= enumerator.MoveNextAsync().AsTask();

                if (heartbeatInterval > TimeSpan.Zero)
                {
                    using var delayCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    var delay = Task.Delay(heartbeatInterval, delayCts.Token);
                    var winner = await Task.WhenAny(pending, delay).ConfigureAwait(false);

                    if (winner != pending)
                    {
                        await delayCts.CancelAsync().ConfigureAwait(false);
                        ct.ThrowIfCancellationRequested();
                        await writer.WriteCommentAsync("ping", ct).ConfigureAwait(false);
                        continue;
                    }

                    await delayCts.CancelAsync().ConfigureAwait(false);
                }

                if (!await pending.ConfigureAwait(false))
                    break;

                pending = null;
                var frame = enumerator.Current;
                await writer.WriteEventAsync(frame.Name, frame.Payload, ct).ConfigureAwait(false);
            }
        }
        finally
        {
            if (pending is not null)
            {
                // 取消/异常退出时把挂起的 MoveNext 收干净，避免迭代器悬挂
                try { await pending.ConfigureAwait(false); }
                catch { /* 收尾异常不上抛：调用方已在处理自身异常 */ }
            }

            await enumerator.DisposeAsync().ConfigureAwait(false);
        }
    }
}