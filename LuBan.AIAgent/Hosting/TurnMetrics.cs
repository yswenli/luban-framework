/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \\ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/ \  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Hosting
*描述：单回合用量与耗时采集
*
*****************************************************************************/
namespace LuBan.AIAgent.Hosting;

/// <summary>
/// 单回合（一次用户输入到助手回复完成）的用量与耗时采集器。
/// </summary>
/// <remarks>
/// 会话级 scope 内注册，由 <see cref="AgentHost"/> 在回合开始/流式过程中写入，
/// 由 <c>SessionChatHistoryProvider</c> 在落库时读取，用于给助手消息写入真实 token 用量与耗时。
/// 同一会话的回合串行执行（见 <see cref="SessionRuntime"/>），故单实例复用不会串扰。
/// </remarks>
public sealed class TurnMetrics
{
    private readonly object _gate = new();
    private long _startTimestamp;
    private long _elapsedMs;
    private long _inputTokens;
    private long _outputTokens;
    private long _totalTokens;
    private bool _hasUsage;

    /// <summary>当前思考区间起点；<see cref="DateTimeOffset.MinValue"/> 表示没有进行中的思考区间。</summary>
    private DateTimeOffset _thinkingStartedAt = DateTimeOffset.MinValue;

    /// <summary>当前思考区间内最后一帧增量的时刻。</summary>
    private DateTimeOffset _thinkingLastAt = DateTimeOffset.MinValue;

    /// <summary>按发生顺序落定的思考区间耗时（毫秒）。</summary>
    private readonly List<int> _thinkingDurationsMs = [];

    /// <summary>工具调用起点，按调用标识记录。</summary>
    private readonly Dictionary<string, DateTimeOffset> _toolStartedAt = new(StringComparer.Ordinal);

    /// <summary>工具调用耗时（毫秒），按调用标识记录；同一调用重试时以最后一次为准。</summary>
    private readonly Dictionary<string, int> _toolDurationsMs = new(StringComparer.Ordinal);

    /// <summary>是否为取消（用户中断）结束：取消时耗时照记，但用量可能不完整。</summary>
    public bool IsCancelled { get; private set; }

    /// <summary>开始新一轮：重置用量并记录起点。</summary>
    public void BeginTurn()
    {
        lock (_gate)
        {
            _startTimestamp = Stopwatch.GetTimestamp();
            _elapsedMs = 0;
            _inputTokens = 0;
            _outputTokens = 0;
            _totalTokens = 0;
            _hasUsage = false;
            IsCancelled = false;
            _thinkingStartedAt = DateTimeOffset.MinValue;
            _thinkingLastAt = DateTimeOffset.MinValue;
            _thinkingDurationsMs.Clear();
            _toolStartedAt.Clear();
            _toolDurationsMs.Clear();
        }
    }

    /// <summary>
    /// 记录一次 LLM 往返的用量。
    /// </summary>
    /// <param name="details">用量明细；整体为 null 时忽略。</param>
    /// <remarks>
    /// 一轮应答可能包含多次 LLM 往返（工具调用使上下文循环），故为累加而非覆盖。
    /// 优先采用 <see cref="UsageDetails.TotalTokenCount"/>；缺失时用输入+输出之和。
    /// </remarks>
    public void RecordUsage(UsageDetails? details)
    {
        if (details is null) return;

        lock (_gate)
        {
            var input = details.InputTokenCount ?? 0;
            var output = details.OutputTokenCount ?? 0;
            var total = details.TotalTokenCount ?? (input + output);

            _inputTokens += input;
            _outputTokens += output;
            _totalTokens += total;
            if (total > 0 || input > 0 || output > 0) _hasUsage = true;
        }
    }

    /// <summary>结束本轮计时并落定耗时。</summary>
    /// <param name="cancelled">是否因取消而结束。</param>
    public void EndTurn(bool cancelled = false)
    {
        lock (_gate)
        {
            // 回合以思考收尾时，先落定这段思考的耗时
            CloseThinkingLocked();

            // 只在尚未落定（EndTurn 被重复调用/未 BeginTurn）时计算，避免把落库回调耗时算进去
            if (_elapsedMs == 0 && _startTimestamp != 0)
                _elapsedMs = (long)Stopwatch.GetElapsedTime(_startTimestamp).TotalMilliseconds;
            IsCancelled = cancelled;
        }
    }

    /// <summary>记录一帧思考增量：首帧开启区间，后续帧刷新末帧时刻。</summary>
    /// <param name="at">该帧的产生时刻。</param>
    public void RecordThinking(DateTimeOffset at)
    {
        lock (_gate)
        {
            if (_thinkingStartedAt == DateTimeOffset.MinValue)
                _thinkingStartedAt = at;
            _thinkingLastAt = at;
        }
    }

    /// <summary>结束当前思考区间（遇到正文、工具调用或回合结束）；无进行中的区间时无副作用。</summary>
    public void EndThinking()
    {
        lock (_gate)
        {
            CloseThinkingLocked();
        }
    }

    /// <summary>记录工具调用开始时刻。</summary>
    /// <param name="callId">调用标识。</param>
    /// <param name="at">开始时刻。</param>
    public void RecordToolStart(string callId, DateTimeOffset at)
    {
        if (string.IsNullOrEmpty(callId)) return;

        lock (_gate)
        {
            _toolStartedAt[callId] = at;
        }
    }

    /// <summary>记录工具调用结束时刻并落定耗时；没有对应开始记录时忽略。</summary>
    /// <param name="callId">调用标识。</param>
    /// <param name="at">结束时刻。</param>
    public void RecordToolEnd(string callId, DateTimeOffset at)
    {
        if (string.IsNullOrEmpty(callId)) return;

        lock (_gate)
        {
            if (!_toolStartedAt.Remove(callId, out var startedAt)) return;
            _toolDurationsMs[callId] = (int)Math.Max(0, (at - startedAt).TotalMilliseconds);
        }
    }

    /// <summary>本轮按发生顺序落定的思考区间耗时（毫秒）。</summary>
    /// <remarks>
    /// 还包含尚未落定的末段：回合以思考收尾时，历史提供者读到的时刻往往早于 <see cref="EndTurn"/>，
    /// 不补这一段就会丢掉最后一个思考块的耗时。
    /// </remarks>
    public IReadOnlyList<int> ThinkingDurationsMs
    {
        get
        {
            lock (_gate)
            {
                var durations = new List<int>(_thinkingDurationsMs);
                if (_thinkingStartedAt != DateTimeOffset.MinValue)
                    durations.Add((int)Math.Max(0, (_thinkingLastAt - _thinkingStartedAt).TotalMilliseconds));
                return durations;
            }
        }
    }

    /// <summary>取指定工具调用的耗时（毫秒）；未落定或没有记录时返回 <c>null</c>。</summary>
    /// <param name="callId">调用标识。</param>
    public int? GetToolDurationMs(string callId)
    {
        if (string.IsNullOrEmpty(callId)) return null;

        lock (_gate)
        {
            return _toolDurationsMs.TryGetValue(callId, out var ms) ? ms : null;
        }
    }

    /// <summary>落定进行中的思考区间；调用方需持有 <see cref="_gate"/>。</summary>
    private void CloseThinkingLocked()
    {
        if (_thinkingStartedAt == DateTimeOffset.MinValue) return;

        _thinkingDurationsMs.Add((int)Math.Max(0, (_thinkingLastAt - _thinkingStartedAt).TotalMilliseconds));
        _thinkingStartedAt = DateTimeOffset.MinValue;
        _thinkingLastAt = DateTimeOffset.MinValue;
    }

    /// <summary>本轮是否采集到了真实用量。</summary>
    public bool HasUsage
    {
        get { lock (_gate) return _hasUsage; }
    }

    /// <summary>本轮累计输入 token；未采集到用量时为 0。</summary>
    public long InputTokens
    {
        get { lock (_gate) return _inputTokens; }
    }

    /// <summary>本轮累计输出 token；未采集到用量时为 0。</summary>
    public long OutputTokens
    {
        get { lock (_gate) return _outputTokens; }
    }

    /// <summary>本轮累计总 token；未采集到用量时为 0。</summary>
    public long TotalTokens
    {
        get { lock (_gate) return _hasUsage ? _totalTokens : 0; }
    }

    /// <summary>本轮耗时（毫秒）；未采集到时为 0。</summary>
    public int ElapsedMs
    {
        get
        {
            lock (_gate)
            {
                // 兜底：调用方未显式 EndTurn（如异常路径）时按当前时刻计算
                if (_elapsedMs > 0) return (int)_elapsedMs;
                if (_startTimestamp == 0) return 0;
                return (int)Stopwatch.GetElapsedTime(_startTimestamp).TotalMilliseconds;
            }
        }
    }
}