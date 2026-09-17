/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Errors
*文件名： ResilientChatClient
*唯一标识：LLM 调用容错重试中间件
*创建时间：2026/9/17
*描述：对瞬时错误按配置自动退避重试，最终失败抛出携带分类结果的 AgentApiException
*
*****************************************************************************/
namespace LuBan.AIAgent.Errors;

/// <summary>
/// LLM 调用容错中间件：对瞬时错误（限流/5xx/网络/超时）按配置自动退避重试，
/// 最终失败时抛出 <see cref="AgentApiException"/>。
/// 流式调用仅在首个 token 产出前重试；已产出内容后中断不再重试，抛「响应中断」。
/// 必须包裹在 <c>FunctionInvokingChatClient</c> 之内，避免工具轮次整体回滚造成重复副作用。
/// </summary>
public class ResilientChatClient : IChatClient
{
    private readonly IChatClient _inner;
    private readonly LuBanAgentOptions _options;
    private const int MaxConsecutiveEnumeratorOutOfRange = 3;

    /// <summary>
    /// 创建容错客户端。
    /// </summary>
    /// <param name="inner">被包装的聊天客户端。</param>
    /// <param name="options">Agent 配置（读取 <see cref="LuBanAgentOptions.ApiRetry"/> 与 <see cref="LuBanAgentOptions.OnApiRetry"/>）。</param>
    public ResilientChatClient(IChatClient inner, LuBanAgentOptions options)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc/>
    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var list = messages as IList<ChatMessage> ?? messages.ToList();
        var retryOptions = _options.ApiRetry ?? new ApiRetryOptions();
        var maxAttempts = retryOptions.Enabled ? Math.Max(1, retryOptions.MaxAttempts) : 1;

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await _inner.GetResponseAsync(list, options, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var info = ApiErrorClassifier.Classify(ex, cancellationToken.IsCancellationRequested);
                if (info.Category == AgentApiErrorCategory.Canceled)
                    throw;

                if (attempt >= maxAttempts || !info.IsRetryable)
                {
                    Logger.Error("LLM 调用失败", ex, info.Category.ToString());
                    throw new AgentApiException(info, ex);
                }

                var delay = GetDelay(attempt, ex, retryOptions);
                Notify(info, attempt, maxAttempts, delay);
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var list = messages as IList<ChatMessage> ?? messages.ToList();
        var retryOptions = _options.ApiRetry ?? new ApiRetryOptions();
        var maxAttempts = retryOptions.Enabled ? Math.Max(1, retryOptions.MaxAttempts) : 1;

        for (var attempt = 1; ; attempt++)
        {
            var yieldedAny = false;
            var completed = false;
            var consecutiveEnumeratorOutOfRange = 0;
            Exception? failure = null;

            await using (var enumerator = _inner
                .GetStreamingResponseAsync(list, options, cancellationToken)
                .GetAsyncEnumerator(cancellationToken))
            {
                while (true)
                {
                    try
                    {
                        if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                            completed = true;
                    }
                    catch (ArgumentOutOfRangeException ex) when (!cancellationToken.IsCancellationRequested)
                    {
                        // 参照 SanitizingChatClient 的既有行为：个别 provider 的流枚举器会抛越界异常；
                        // 但连续越界说明流已损坏，限次后按失败处理，避免无产出热自旋
                        if (++consecutiveEnumeratorOutOfRange <= MaxConsecutiveEnumeratorOutOfRange)
                            continue;
                        failure = ex;
                    }
                    catch (Exception ex)
                    {
                        failure = ex;
                    }

                    if (failure is not null || completed)
                        break;

                    consecutiveEnumeratorOutOfRange = 0;
                    yieldedAny = true;
                    yield return enumerator.Current;
                }
            }

            if (completed || failure is null)
                yield break;

            var info = ApiErrorClassifier.Classify(failure, cancellationToken.IsCancellationRequested);
            if (info.Category == AgentApiErrorCategory.Canceled)
                throw failure;

            if (yieldedAny)
            {
                Logger.Error("LLM 流式中断", failure, info.Category.ToString());
                throw new AgentApiException(ApiErrorClassifier.CreateStreamInterrupted(info.StatusCode, info.ErrorCode), failure);
            }

            if (attempt >= maxAttempts || !info.IsRetryable)
            {
                Logger.Error("LLM 调用失败", failure, info.Category.ToString());
                throw new AgentApiException(info, failure);
            }

            var delay = GetDelay(attempt, failure, retryOptions);
            Notify(info, attempt, maxAttempts, delay);
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public object? GetService(Type serviceType, object? key = null)
        => _inner.GetService(serviceType, key);

    /// <inheritdoc/>
    public void Dispose()
    {
        _inner.Dispose();
        GC.SuppressFinalize(this);
    }

    private TimeSpan GetDelay(int attempt, Exception ex, ApiRetryOptions retryOptions)
        => ApiErrorClassifier.TryGetRetryAfter(ex, out var retryAfter)
            ? ApiErrorClassifier.ComputeRetryDelay(attempt, retryOptions, retryAfter)
            : ApiErrorClassifier.ComputeRetryDelay(attempt, retryOptions);

    private void Notify(AgentApiErrorInfo info, int attempt, int maxAttempts, TimeSpan delay)
    {
        Logger.Warn($"LLM 调用失败（{info.Category}），{delay.TotalSeconds:F0}s 后重试（第 {attempt} 次重试，最多 {maxAttempts - 1} 次）");
        try
        {
            _options.OnApiRetry?.Invoke(new AgentRetryNotice(info.Category, attempt, maxAttempts, delay));
        }
        catch (Exception ex)
        {
            Logger.Warn("重试通知回调异常", ex);
        }
    }
}