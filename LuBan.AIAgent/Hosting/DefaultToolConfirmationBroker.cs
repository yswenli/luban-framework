/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Hosting
*文件名： DefaultToolConfirmationBroker
*版本号： V1.0.0.0
*唯一标识：d0c7e5a9-8b31-4f26-9a75-3e1c6b4d8f20
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/29
*描述：进程内工具确认登记簿
*
*=================================================
*修改标记
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：进程内工具确认登记簿
*
*****************************************************************************/

using System.Collections.Concurrent;

using LuBan.AIAgent.Abstractions;

using Microsoft.Extensions.Options;

namespace LuBan.AIAgent.Hosting;

/// <summary>进程内确认登记簿。</summary>
public sealed class DefaultToolConfirmationBroker : IToolConfirmationBroker
{
    private sealed class Entry(PendingConfirmation pending)
    {
        public PendingConfirmation Pending { get; } = pending;
        public TaskCompletionSource<ConfirmResult> Completion { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private readonly ConcurrentDictionary<string, Entry> _entries = new(StringComparer.Ordinal);
    private readonly TimeSpan _timeout;

    /// <summary>创建 broker。</summary>
    /// <param name="options">Agent 配置（读 <c>LuBanAgent:Confirmation:TimeoutSeconds</c>）。</param>
    /// <param name="timeoutOverride">测试用超时覆盖；生产传 null。</param>
    public DefaultToolConfirmationBroker(
        IOptions<LuBanAgentOptions> options,
        TimeSpan? timeoutOverride = null)
    {
        var seconds = options.Value.Confirmation.TimeoutSeconds;
        TimeoutSeconds = seconds > 0 ? seconds : 60;
        _timeout = timeoutOverride ?? TimeSpan.FromSeconds(TimeoutSeconds);
    }

    /// <inheritdoc />
    public int TimeoutSeconds { get; }

    /// <inheritdoc />
    public PendingConfirmation Register(
        string sessionId, string toolName, IReadOnlyDictionary<string, object?> arguments)
    {
        var pending = new PendingConfirmation(Guid.NewGuid().ToString("N"), sessionId, toolName, arguments);
        _entries[pending.ConfirmationId] = new Entry(pending);
        return pending;
    }

    /// <inheritdoc />
    public async Task<ConfirmResult> WaitAsync(PendingConfirmation pending, CancellationToken ct)
    {
        if (!_entries.TryGetValue(pending.ConfirmationId, out var entry))
            return ConfirmResult.Deny;

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var delay = Task.Delay(_timeout, timeoutCts.Token);
            var winner = await Task.WhenAny(entry.Completion.Task, delay).ConfigureAwait(false);

            if (winner == entry.Completion.Task)
            {
                timeoutCts.Cancel();   // 停止计时器，避免 Timer 泄漏
                return await entry.Completion.Task.ConfigureAwait(false);
            }

            return ConfirmResult.Deny;
        }
        catch (OperationCanceledException)
        {
            return ConfirmResult.Deny;
        }
        finally
        {
            _entries.TryRemove(pending.ConfirmationId, out _);
        }
    }

    /// <inheritdoc />
    public bool TryResolve(string sessionId, string confirmationId, ConfirmResult result)
    {
        if (!_entries.TryGetValue(confirmationId, out var entry))
            return false;
        if (!string.Equals(entry.Pending.SessionId, sessionId, StringComparison.Ordinal))
            return false;

        return entry.Completion.TrySetResult(result);
    }
}