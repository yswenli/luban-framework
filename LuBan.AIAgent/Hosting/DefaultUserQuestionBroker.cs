/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*命名空间：LuBan.AIAgent.Hosting
*文件名： DefaultUserQuestionBroker
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/11/24
*描述：进程内用户提问登记簿
*
*****************************************************************************/

namespace LuBan.AIAgent.Hosting;

/// <summary>进程内提问登记簿。</summary>
public sealed class DefaultUserQuestionBroker : IUserQuestionBroker
{
    private sealed class Entry(PendingQuestion pending)
    {
        public PendingQuestion Pending { get; } = pending;

        public TaskCompletionSource<UserQuestionResult> Completion { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private readonly ConcurrentDictionary<string, Entry> _entries = new(StringComparer.Ordinal);
    private readonly TimeSpan _timeout;

    /// <summary>创建 broker。</summary>
    /// <param name="options">Agent 配置（读 <c>LuBanAgent:Interaction:TimeoutSeconds</c>，未配置时回退 <c>LuBanAgent:Confirmation:TimeoutSeconds</c>）。</param>
    /// <param name="timeoutOverride">测试用超时覆盖；生产传 null。</param>
    public DefaultUserQuestionBroker(
        IOptions<LuBanAgentOptions> options,
        TimeSpan? timeoutOverride = null)
    {
        // 提问要逐题点选/输入，比工具确认更耗时；未单独配置时仍按确认超时兜底
        var seconds = options.Value.Interaction.TimeoutSeconds;
        if (seconds <= 0)
            seconds = options.Value.Confirmation.TimeoutSeconds;
        TimeoutSeconds = seconds > 0 ? seconds : 60;
        _timeout = timeoutOverride ?? TimeSpan.FromSeconds(TimeoutSeconds);
    }

    /// <inheritdoc />
    public int TimeoutSeconds { get; }

    /// <inheritdoc />
    public PendingQuestion Register(string sessionId, IReadOnlyList<UserQuestion> questions)
    {
        var pending = new PendingQuestion(Guid.NewGuid().ToString("N"), sessionId, questions, TimeoutSeconds);
        _entries[pending.QuestionId] = new Entry(pending);
        return pending;
    }

    /// <inheritdoc />
    public async Task<UserQuestionResult> WaitAsync(PendingQuestion pending, CancellationToken ct)
    {
        if (!_entries.TryGetValue(pending.QuestionId, out var entry))
            return UserQuestionResult.Cancelled("该提问已失效。");

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

            // 超时按「用户未作答」处理：不算拒绝、不失败节点，只让模型据现有信息继续
            return UserQuestionResult.Cancelled("等待用户回答超时，未获得答案。请基于现有信息继续。");
        }
        catch (OperationCanceledException)
        {
            return UserQuestionResult.Cancelled();
        }
        finally
        {
            _entries.TryRemove(pending.QuestionId, out _);
        }
    }

    /// <inheritdoc />
    public bool TryResolve(string sessionId, string questionId, IReadOnlyList<UserQuestionAnswer> answers)
    {
        if (answers is null)
            return false;
        if (!_entries.TryGetValue(questionId, out var entry))
            return false;
        if (!string.Equals(entry.Pending.SessionId, sessionId, StringComparison.Ordinal))
            return false;

        // 按问题正文回链：每题都必须在，且不能出现本次提问之外的问题（防止前端漏答/错答后模型拿到残缺输入）
        var expected = entry.Pending.Questions.Select(q => q.Question).ToHashSet(StringComparer.Ordinal);
        var answered = new HashSet<string>(StringComparer.Ordinal);
        foreach (var answer in answers)
        {
            if (string.IsNullOrWhiteSpace(answer.Question) || !expected.Contains(answer.Question))
                return false;
            if (!answered.Add(answer.Question))
                return false;
        }

        if (answered.Count != expected.Count)
            return false;

        return entry.Completion.TrySetResult(UserQuestionResult.Answered(answers));
    }

    /// <inheritdoc />
    public bool TryCancel(string sessionId, string questionId)
    {
        if (!_entries.TryGetValue(questionId, out var entry))
            return false;
        if (!string.Equals(entry.Pending.SessionId, sessionId, StringComparison.Ordinal))
            return false;

        return entry.Completion.TrySetResult(UserQuestionResult.Cancelled());
    }
}