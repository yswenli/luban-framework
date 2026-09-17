/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： AgentApiErrorUnitTest
*唯一标识：LLM/API 错误分类与重试单测
*创建时间：2026/9/17
*描述：ApiErrorClassifier 分类矩阵与退避计算单元测试（纯单元，不访问网络）
*
*****************************************************************************/
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Errors;

using Microsoft.Extensions.AI;

using System.ClientModel;
using System.Net;

namespace LuBan.XTestProject;

[TestClass]
public class AgentApiErrorUnitTest
{
    [TestMethod]
    public void ClassifyHttpStatus_401_IsAuthentication_NotRetryable()
    {
        var info = ApiErrorClassifier.ClassifyHttpStatus(401, "Incorrect API key provided: sk-xxx");

        Assert.AreEqual(AgentApiErrorCategory.Authentication, info.Category);
        Assert.IsFalse(info.IsRetryable);
    }

    [TestMethod]
    public void ClassifyHttpStatus_402_IsPaymentRequired_NotRetryable()
    {
        var info = ApiErrorClassifier.ClassifyHttpStatus(402, "insufficient balance");

        Assert.AreEqual(AgentApiErrorCategory.PaymentRequired, info.Category);
        Assert.IsFalse(info.IsRetryable);
    }

    [TestMethod]
    public void ClassifyHttpStatus_403_IsPermissionDenied_NotRetryable()
    {
        var info = ApiErrorClassifier.ClassifyHttpStatus(403, "you do not have access to model");

        Assert.AreEqual(AgentApiErrorCategory.PermissionDenied, info.Category);
        Assert.IsFalse(info.IsRetryable);
    }

    [TestMethod]
    public void ClassifyHttpStatus_404_IsModelNotFound_NotRetryable()
    {
        var info = ApiErrorClassifier.ClassifyHttpStatus(404, "The model `gpt-x` does not exist");

        Assert.AreEqual(AgentApiErrorCategory.ModelNotFound, info.Category);
        Assert.IsFalse(info.IsRetryable);
        StringAssert.Contains(info.FriendlyMessage, "模型");
    }

    [TestMethod]
    public void ClassifyHttpStatus_400_ContextLength_IsContextLengthExceeded()
    {
        var info = ApiErrorClassifier.ClassifyHttpStatus(400,
            "This model's maximum context length is 8192 tokens. error code: context_length_exceeded");

        Assert.AreEqual(AgentApiErrorCategory.ContextLengthExceeded, info.Category);
        Assert.IsFalse(info.IsRetryable);
    }

    [TestMethod]
    public void ClassifyHttpStatus_400_ContentFilter_IsContentFiltered()
    {
        var info = ApiErrorClassifier.ClassifyHttpStatus(400,
            "The response was filtered due to content_filter");

        Assert.AreEqual(AgentApiErrorCategory.ContentFiltered, info.Category);
        Assert.IsFalse(info.IsRetryable);
    }

    [TestMethod]
    public void ClassifyHttpStatus_400_Unsupported_IsUnsupportedCapability()
    {
        var info = ApiErrorClassifier.ClassifyHttpStatus(400,
            "tools is not supported by this model");

        Assert.AreEqual(AgentApiErrorCategory.UnsupportedCapability, info.Category);
        Assert.IsFalse(info.IsRetryable);
    }

    [TestMethod]
    public void ClassifyHttpStatus_429_RateLimit_IsRetryable()
    {
        var info = ApiErrorClassifier.ClassifyHttpStatus(429, "rate_limit_exceeded: too many requests");

        Assert.AreEqual(AgentApiErrorCategory.RateLimited, info.Category);
        Assert.IsTrue(info.IsRetryable);
    }

    [TestMethod]
    public void ClassifyHttpStatus_429_InsufficientQuota_IsQuotaExhausted_WithResetTime()
    {
        var info = ApiErrorClassifier.ClassifyHttpStatus(429,
            "HTTP 429 (insufficient_quota: insufficient_quota)\n\nYour token-plan 1-week quota has been exhausted. The quota will reset at 09-20 09:46:00 UTC.");

        Assert.AreEqual(AgentApiErrorCategory.QuotaExhausted, info.Category);
        Assert.IsFalse(info.IsRetryable);
        StringAssert.Contains(info.FriendlyMessage, "09-20 09:46:00 UTC");
    }

    [TestMethod]
    public void ClassifyHttpStatus_503_IsServerError_Retryable()
    {
        var info = ApiErrorClassifier.ClassifyHttpStatus(503, "Service Unavailable");

        Assert.AreEqual(AgentApiErrorCategory.ServerError, info.Category);
        Assert.IsTrue(info.IsRetryable);
    }

    [TestMethod]
    public void Classify_HttpRequestException_IsNetworkError_Retryable()
    {
        var ex = new HttpRequestException("connection refused");

        var info = ApiErrorClassifier.Classify(ex);

        Assert.AreEqual(AgentApiErrorCategory.NetworkError, info.Category);
        Assert.IsTrue(info.IsRetryable);
    }

    [TestMethod]
    public void Classify_HttpRequestException_WithStatusCode_RoutesToStatusClassification()
    {
        var ex = new HttpRequestException("unauthorized", null, HttpStatusCode.Unauthorized);

        var info = ApiErrorClassifier.Classify(ex);

        Assert.AreEqual(AgentApiErrorCategory.Authentication, info.Category);
        Assert.IsFalse(info.IsRetryable);
    }

    [TestMethod]
    public void Classify_ClientResultException_NoResponse_UnwrapsToNetworkError()
    {
        // SDK 传输层把 HttpRequestException 包成 ClientResultException（未收到响应时 Status == 0）
        var ex = new ClientResultException("connection refused", null!, new HttpRequestException("connection refused"));

        var info = ApiErrorClassifier.Classify(ex);

        Assert.AreEqual(AgentApiErrorCategory.NetworkError, info.Category);
        Assert.IsTrue(info.IsRetryable);
    }

    [TestMethod]
    public void Classify_ClientResultException_NoResponseNoInner_IsNetworkError()
    {
        var ex = new ClientResultException("no response", null!, null!);

        var info = ApiErrorClassifier.Classify(ex);

        Assert.AreEqual(AgentApiErrorCategory.NetworkError, info.Category);
        Assert.IsTrue(info.IsRetryable);
    }

    [TestMethod]
    public void Classify_AggregateException_UnwrapsInner()
    {
        // SDK 重试耗尽：多次尝试都抛异常时抛 AggregateException
        var ex = new AggregateException("Retry failed after 4 tries.",
            new HttpRequestException("connection refused"), new TaskCanceledException());

        var info = ApiErrorClassifier.Classify(ex);

        Assert.AreEqual(AgentApiErrorCategory.NetworkError, info.Category);
        Assert.IsTrue(info.IsRetryable);
    }

    [TestMethod]
    public void Classify_AgentApiException_PassesThrough()
    {
        var original = new AgentApiErrorInfo(AgentApiErrorCategory.QuotaExhausted, 429, "insufficient_quota", "配额已用尽", false);
        var ex = new AgentApiException(original, new InvalidOperationException("x"));

        var info = ApiErrorClassifier.Classify(ex);

        Assert.AreSame(original, info);
    }

    [TestMethod]
    public void ClassifyHttpStatus_429_BillingWithoutQuotaPhrase_IsRateLimited_Retryable()
    {
        var info = ApiErrorClassifier.ClassifyHttpStatus(429,
            "Rate limit reached. Visit https://platform.openai.com/account/billing to check your limit and billing details.");

        Assert.AreEqual(AgentApiErrorCategory.RateLimited, info.Category);
        Assert.IsTrue(info.IsRetryable);
    }

    [TestMethod]
    public void ClassifyHttpStatus_403_ModelAccessDenied_IsPermissionDenied()
    {
        var info = ApiErrorClassifier.ClassifyHttpStatus(403,
            "Your API key does not have permission to use this model.");

        Assert.AreEqual(AgentApiErrorCategory.PermissionDenied, info.Category);
        Assert.IsFalse(info.IsRetryable);
    }

    [TestMethod]
    public void ClassifyHttpStatus_422_ValidationError_IsBadRequest()
    {
        var info = ApiErrorClassifier.ClassifyHttpStatus(422,
            "body.messages: field required");

        Assert.AreEqual(AgentApiErrorCategory.BadRequest, info.Category);
        Assert.IsFalse(info.IsRetryable);
    }

    [TestMethod]
    public void ClassifyHttpStatus_429_QuotaWithFourDigitYear_ParsesResetTime()
    {
        var info = ApiErrorClassifier.ClassifyHttpStatus(429,
            "quota has been exhausted. The quota will reset at 2026-09-20 09:46:00 UTC.");

        Assert.AreEqual(AgentApiErrorCategory.QuotaExhausted, info.Category);
        StringAssert.Contains(info.FriendlyMessage, "2026-09-20 09:46:00 UTC");
    }

    [TestMethod]
    public void ComputeRetryDelay_CapsRetryAfter()
    {
        var options = new ApiRetryOptions { MaxDelayMs = 30000 };

        // 服务端可能返回超大 Retry-After（如 30 天），必须被 MaxDelayMs 收口
        var delay = ApiErrorClassifier.ComputeRetryDelay(1, options, TimeSpan.FromDays(30));

        Assert.AreEqual(30d, delay.TotalSeconds, 0.001);
    }

    [TestMethod]
    public void ComputeRetryDelay_MalformedConfig_DoesNotThrow()
    {
        var options = new ApiRetryOptions { BaseDelayMs = int.MaxValue, BackoffFactor = 1000, MaxDelayMs = 30000 };

        var delay = ApiErrorClassifier.ComputeRetryDelay(5, options);

        Assert.IsTrue(delay > TimeSpan.Zero && delay <= TimeSpan.FromSeconds(30), $"delay={delay}");
    }

    [TestMethod]
    public void Classify_OperationCanceled_ByHost_IsCanceled_NotRetryable()
    {
        var info = ApiErrorClassifier.Classify(new OperationCanceledException(), canceledByHost: true);

        Assert.AreEqual(AgentApiErrorCategory.Canceled, info.Category);
        Assert.IsFalse(info.IsRetryable);
    }

    [TestMethod]
    public void Classify_OperationCanceled_NotByHost_IsTimeout_Retryable()
    {
        var info = ApiErrorClassifier.Classify(new OperationCanceledException(), canceledByHost: false);

        Assert.AreEqual(AgentApiErrorCategory.Timeout, info.Category);
        Assert.IsTrue(info.IsRetryable);
    }

    [TestMethod]
    public void Classify_Unknown_ReturnsUnifiedFallback()
    {
        var info = ApiErrorClassifier.Classify(new InvalidOperationException("something odd"));

        Assert.AreEqual(AgentApiErrorCategory.Unknown, info.Category);
        Assert.IsFalse(info.IsRetryable);
        StringAssert.Contains(info.FriendlyMessage, "未识别错误");
    }

    [TestMethod]
    public void ComputeRetryDelay_ExponentialWithJitter()
    {
        var options = new ApiRetryOptions { BaseDelayMs = 1000, BackoffFactor = 2 };

        var first = ApiErrorClassifier.ComputeRetryDelay(1, options);
        var second = ApiErrorClassifier.ComputeRetryDelay(2, options);

        Assert.IsTrue(first.TotalMilliseconds >= 800 && first.TotalMilliseconds <= 1200, $"first={first.TotalMilliseconds}");
        Assert.IsTrue(second.TotalMilliseconds >= 1600 && second.TotalMilliseconds <= 2400, $"second={second.TotalMilliseconds}");
    }

    [TestMethod]
    public void ComputeRetryDelay_RetryAfter_Wins()
    {
        var options = new ApiRetryOptions { BaseDelayMs = 1000, BackoffFactor = 2 };

        var delay = ApiErrorClassifier.ComputeRetryDelay(3, options, TimeSpan.FromSeconds(5));

        Assert.AreEqual(5d, delay.TotalSeconds, 0.001);
    }

    [TestMethod]
    public void GetCategoryLabel_ReturnsChineseLabel()
    {
        Assert.AreEqual("限流", ApiErrorClassifier.GetCategoryLabel(AgentApiErrorCategory.RateLimited));
        Assert.AreEqual("未识别错误", ApiErrorClassifier.GetCategoryLabel(AgentApiErrorCategory.Unknown));
    }

    [TestMethod]
    public async Task Resilient_RetriesTransient_ThenSucceeds()
    {
        var inner = new FlakyChatClient(new HttpRequestException("boom", null, HttpStatusCode.ServiceUnavailable), failTimes: 2);
        var options = new LuBanAgentOptions
        {
            ApiRetry = new ApiRetryOptions { MaxAttempts = 3, BaseDelayMs = 1, BackoffFactor = 1 }
        };
        var notices = new List<AgentRetryNotice>();
        options.OnApiRetry = notices.Add;

        var client = new ResilientChatClient(inner, options);
        var response = await client.GetResponseAsync(new[] { new ChatMessage(ChatRole.User, "hi") });

        Assert.AreEqual(3, inner.Calls);
        Assert.AreEqual(2, notices.Count);
        Assert.AreEqual(1, notices[0].Attempt);
        Assert.AreEqual(3, notices[0].MaxAttempts);
        Assert.AreEqual("ok", response.Text);
    }

    [TestMethod]
    public async Task Resilient_Exhausted_ThrowsAgentApiException()
    {
        var inner = new FlakyChatClient(new HttpRequestException("boom", null, HttpStatusCode.ServiceUnavailable), failTimes: 99);
        var options = new LuBanAgentOptions
        {
            ApiRetry = new ApiRetryOptions { MaxAttempts = 3, BaseDelayMs = 1, BackoffFactor = 1 }
        };

        var client = new ResilientChatClient(inner, options);

        var thrown = await Assert.ThrowsExactlyAsync<AgentApiException>(() =>
            client.GetResponseAsync(new[] { new ChatMessage(ChatRole.User, "hi") }));

        Assert.AreEqual(AgentApiErrorCategory.ServerError, thrown.Error.Category);
        Assert.AreEqual(3, inner.Calls);
    }

    [TestMethod]
    public async Task Resilient_NonRetryable_ThrowsImmediately()
    {
        var inner = new FlakyChatClient(new InvalidOperationException("bad config"), failTimes: 99);
        var options = new LuBanAgentOptions
        {
            ApiRetry = new ApiRetryOptions { MaxAttempts = 3, BaseDelayMs = 1, BackoffFactor = 1 }
        };

        var client = new ResilientChatClient(inner, options);

        var thrown = await Assert.ThrowsExactlyAsync<AgentApiException>(() =>
            client.GetResponseAsync(new[] { new ChatMessage(ChatRole.User, "hi") }));

        Assert.AreEqual(AgentApiErrorCategory.Unknown, thrown.Error.Category);
        Assert.AreEqual(1, inner.Calls);
    }

    [TestMethod]
    public async Task Resilient_Disabled_DoesNotRetry()
    {
        var inner = new FlakyChatClient(new HttpRequestException("boom", null, HttpStatusCode.ServiceUnavailable), failTimes: 99);
        var options = new LuBanAgentOptions
        {
            ApiRetry = new ApiRetryOptions { Enabled = false, MaxAttempts = 3, BaseDelayMs = 1, BackoffFactor = 1 }
        };

        var client = new ResilientChatClient(inner, options);

        await Assert.ThrowsExactlyAsync<AgentApiException>(() =>
            client.GetResponseAsync(new[] { new ChatMessage(ChatRole.User, "hi") }));

        Assert.AreEqual(1, inner.Calls);
    }

    [TestMethod]
    public async Task Resilient_Streaming_RetriesBeforeFirstToken()
    {
        var inner = new ScriptedStreamClient();
        inner.Enqueue(text: null, fail: new HttpRequestException("boom", null, HttpStatusCode.ServiceUnavailable));
        inner.Enqueue(text: "hello", fail: null);
        var options = new LuBanAgentOptions
        {
            ApiRetry = new ApiRetryOptions { MaxAttempts = 3, BaseDelayMs = 1, BackoffFactor = 1 }
        };

        var client = new ResilientChatClient(inner, options);
        var collected = new List<string>();
        await foreach (var update in client.GetStreamingResponseAsync(new[] { new ChatMessage(ChatRole.User, "hi") }))
        {
            collected.Add(update.Text ?? string.Empty);
        }

        Assert.AreEqual(2, inner.Calls);
        Assert.AreEqual("hello", string.Concat(collected));
    }

    [TestMethod]
    public async Task Resilient_Streaming_AfterFirstToken_ThrowsStreamInterrupted()
    {
        var inner = new ScriptedStreamClient();
        inner.Enqueue(text: "part", fail: new HttpRequestException("connection reset"));
        var options = new LuBanAgentOptions
        {
            ApiRetry = new ApiRetryOptions { MaxAttempts = 3, BaseDelayMs = 1, BackoffFactor = 1 }
        };

        var client = new ResilientChatClient(inner, options);
        var collected = new List<string>();
        var thrown = await Assert.ThrowsExactlyAsync<AgentApiException>(async () =>
        {
            await foreach (var update in client.GetStreamingResponseAsync(new[] { new ChatMessage(ChatRole.User, "hi") }))
            {
                collected.Add(update.Text ?? string.Empty);
            }
        });

        Assert.AreEqual(AgentApiErrorCategory.StreamInterrupted, thrown.Error.Category);
        Assert.AreEqual(1, inner.Calls);
        Assert.AreEqual("part", string.Concat(collected));
    }

    [TestMethod]
    public async Task Resilient_Streaming_HostCanceled_DoesNotRetry()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var inner = new ScriptedStreamClient();
        inner.Enqueue(text: null, fail: new OperationCanceledException(cts.Token));
        var options = new LuBanAgentOptions
        {
            ApiRetry = new ApiRetryOptions { MaxAttempts = 3, BaseDelayMs = 1, BackoffFactor = 1 }
        };

        var client = new ResilientChatClient(inner, options);

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var _ in client.GetStreamingResponseAsync(new[] { new ChatMessage(ChatRole.User, "hi") }, cancellationToken: cts.Token))
            {
            }
        });

        Assert.AreEqual(1, inner.Calls);
    }

    [TestMethod]
    public void ChatClientResilience_Wrap_IsIdempotent()
    {
        var raw = new FlakyChatClient(new InvalidOperationException("x"), failTimes: 0);
        var options = new LuBanAgentOptions();

        var once = ChatClientResilience.Wrap(raw, options);
        var twice = ChatClientResilience.Wrap(once, options);

        Assert.AreSame(once, twice);
    }

    [TestMethod]
    public async Task Resilient_Streaming_ExhaustedBeforeFirstToken_ThrowsOriginalCategory()
    {
        var inner = new ScriptedStreamClient();
        inner.Enqueue(text: null, fail: new HttpRequestException("boom", null, HttpStatusCode.ServiceUnavailable));
        inner.Enqueue(text: null, fail: new HttpRequestException("boom", null, HttpStatusCode.ServiceUnavailable));
        inner.Enqueue(text: null, fail: new HttpRequestException("boom", null, HttpStatusCode.ServiceUnavailable));
        var options = new LuBanAgentOptions
        {
            ApiRetry = new ApiRetryOptions { MaxAttempts = 3, BaseDelayMs = 1, BackoffFactor = 1 }
        };

        var client = new ResilientChatClient(inner, options);

        var thrown = await Assert.ThrowsExactlyAsync<AgentApiException>(async () =>
        {
            await foreach (var _ in client.GetStreamingResponseAsync(new[] { new ChatMessage(ChatRole.User, "hi") }))
            {
            }
        });

        Assert.AreEqual(AgentApiErrorCategory.ServerError, thrown.Error.Category);
        Assert.AreEqual(3, inner.Calls);
    }

    [TestMethod]
    public async Task Resilient_OnApiRetry_ThrowingCallback_DoesNotBreakRetry()
    {
        var inner = new FlakyChatClient(new HttpRequestException("boom", null, HttpStatusCode.ServiceUnavailable), failTimes: 1);
        var options = new LuBanAgentOptions
        {
            ApiRetry = new ApiRetryOptions { MaxAttempts = 3, BaseDelayMs = 1, BackoffFactor = 1 }
        };
        options.OnApiRetry = _ => throw new InvalidOperationException("host callback failed");

        var client = new ResilientChatClient(inner, options);
        var response = await client.GetResponseAsync(new[] { new ChatMessage(ChatRole.User, "hi") });

        Assert.AreEqual(2, inner.Calls);
        Assert.AreEqual("ok", response.Text);
    }

    [TestMethod]
    public async Task Resilient_BackoffCanceled_ThrowsOperationCanceled()
    {
        var inner = new FlakyChatClient(new HttpRequestException("boom", null, HttpStatusCode.ServiceUnavailable), failTimes: 99);
        var options = new LuBanAgentOptions
        {
            ApiRetry = new ApiRetryOptions { MaxAttempts = 3, BaseDelayMs = 5000, BackoffFactor = 1 }
        };

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
        var client = new ResilientChatClient(inner, options);

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            client.GetResponseAsync(new[] { new ChatMessage(ChatRole.User, "hi") }, cancellationToken: cts.Token));

        Assert.AreEqual(1, inner.Calls);
    }
}

/// <summary>
/// 前 N 次调用失败、之后返回固定响应的聊天客户端（非流式测试用）。
/// </summary>
internal sealed class FlakyChatClient : IChatClient
{
    private readonly Exception _exception;
    private readonly int _failTimes;

    public int Calls { get; private set; }

    public FlakyChatClient(Exception exception, int failTimes)
    {
        _exception = exception;
        _failTimes = failTimes;
    }

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        Calls++;
        if (Calls <= _failTimes)
            return Task.FromException<ChatResponse>(_exception);
        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "ok")));
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public object? GetService(Type serviceType, object? key = null) => null;

    public void Dispose()
    {
    }
}

/// <summary>
/// 按脚本逐次返回「文本 + 是否失败」的流式聊天客户端（流式测试用）。
/// </summary>
internal sealed class ScriptedStreamClient : IChatClient
{
    private readonly Queue<(string? Text, Exception? Fail)> _steps = new();

    public int Calls { get; private set; }

    public void Enqueue(string? text, Exception? fail) => _steps.Enqueue((text, fail));

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        Calls++;
        var step = _steps.Count > 0 ? _steps.Dequeue() : (Text: (string?)null, Fail: (Exception?)null);

        await Task.Yield();

        if (step.Text is not null)
            yield return new ChatResponseUpdate(ChatRole.Assistant, step.Text);

        if (step.Fail is not null)
            throw step.Fail;
    }

    public object? GetService(Type serviceType, object? key = null) => null;

    public void Dispose()
    {
    }
}