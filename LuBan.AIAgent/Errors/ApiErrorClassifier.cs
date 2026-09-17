/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Errors
*文件名： ApiErrorClassifier
*唯一标识：LLM/API 错误分类器
*创建时间：2026/9/17
*描述：结构化优先（HTTP 状态码 + error.code）+ 文案关键字兜底的错误分类器
*
*****************************************************************************/
using System.ClientModel;
using System.Globalization;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace LuBan.AIAgent.Errors;

/// <summary>
/// LLM/API 错误分类器。
/// 识别顺序：结构化信息（<see cref="ClientResultException.Status"/> + 响应文案中的 error.code）优先，
/// 关键字兜底；未识别的错误统一返回 <see cref="AgentApiErrorCategory.Unknown"/> 与统一兜底文案。
/// </summary>
public static class ApiErrorClassifier
{
    private const string UnknownMessage = "调用失败（未识别错误），详情见日志。";

    private const string AuthenticationMessage =
        "API 认证失败：API Key 无效或已过期。请检查该 Provider 的 Key 配置后重试。";

    private const string PaymentRequiredMessage =
        "账户余额不足：请充值后再试。";

    private const string PermissionDeniedMessage =
        "API 权限不足：当前 Key 无权访问该模型。请更换 Key 或切换模型。";

    private const string ModelNotFoundMessage =
        "模型不存在或已下线：请检查模型名与 Provider 端点配置（可用 /model 查看），或切换到其他模型。";

    private const string BadRequestMessage =
        "请求被模型拒绝（参数或格式错误）：请精简输入后重试，或切换到其他模型。";

    private const string ContextLengthExceededMessage =
        "上下文超出模型长度上限：请用 /clear 清空对话、新建会话，或精简当前输入后重试。";

    private const string UnsupportedCapabilityMessage =
        "当前模型不支持该能力（如工具调用/多模态）：请切换到支持该能力的模型。";

    private const string RateLimitedMessage =
        "请求过于频繁（触发速率限制）：请稍后再试。";

    private const string ServerErrorMessage =
        "模型服务端错误：服务暂时不可用，请稍后再试。";

    private const string NetworkErrorMessage =
        "网络连接失败：无法访问模型服务，请检查网络或 Provider 端点配置后重试。";

    private const string TimeoutMessage =
        "请求超时：模型未在超时时间内响应，请稍后再试。";

    private const string StreamInterruptedMessage =
        "响应中断：连接在输出过程中断开，以上内容可能不完整。请重新提问。";

    private const string ContentFilteredMessage =
        "内容被风控拦截（非系统故障）：请调整措辞后重新提问。";

    private const string CanceledMessage = "任务已取消。";

    private static readonly Regex QuotaResetRegex = new(
        @"reset at\s*(?<time>(?:[0-9]{4}-)?[0-9]{2}-[0-9]{2}\s+[0-9]{2}:[0-9]{2}:[0-9]{2}\s*UTC)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// 分类异常。
    /// </summary>
    /// <param name="ex">原始异常。</param>
    /// <param name="canceledByHost">是否为宿主主动取消（Esc）；false 时 OperationCanceledException 视为请求超时。</param>
    /// <returns>分类结果。</returns>
    public static AgentApiErrorInfo Classify(Exception ex, bool canceledByHost = false)
        => Classify(ex, canceledByHost, depth: 0);

    private static AgentApiErrorInfo Classify(Exception ex, bool canceledByHost, int depth)
    {
        ArgumentNullException.ThrowIfNull(ex);

        // 已分类异常直通（幂等）：中间件包装后再分类（编排/压缩路径）不丢类别
        if (ex is AgentApiException agentApiException)
            return agentApiException.Error;

        // SDK 重试耗尽时会抛 AggregateException("Retry failed after N tries.")：取内层首个异常继续识别
        if (depth < 3 && ex is AggregateException aggregateException && aggregateException.InnerExceptions.Count > 0)
            return Classify(aggregateException.InnerExceptions[0], canceledByHost, depth + 1);

        if (ex is OperationCanceledException)
        {
            return canceledByHost
                ? new(AgentApiErrorCategory.Canceled, null, null, CanceledMessage, false)
                : new(AgentApiErrorCategory.Timeout, null, null, TimeoutMessage, true);
        }

        if (ex is TimeoutException)
            return new(AgentApiErrorCategory.Timeout, null, null, TimeoutMessage, true);

        if (ex is ClientResultException clientException)
        {
            // Status <= 0：未收到响应（传输层失败，如 DNS/连接/TLS 被 SDK 包装成 ClientResultException），
            // 先解内层异常；无内层时按网络错误处理（可重试）
            if (clientException.Status <= 0)
            {
                var inner = clientException.InnerException;
                if (depth < 3 && inner is not null && !ReferenceEquals(inner, clientException))
                    return Classify(inner, canceledByHost, depth + 1);
                return new(AgentApiErrorCategory.NetworkError, null, null, NetworkErrorMessage, true);
            }

            return ClassifyHttpStatus(clientException.Status, clientException.Message);
        }

        if (ex is HttpRequestException httpRequestException)
        {
            // 带 4xx/5xx 状态码的 HttpRequestException 按状态码分类，避免把 401 当网络错误重试
            if (httpRequestException.StatusCode is { } statusCode && (int)statusCode >= 400)
                return ClassifyHttpStatus((int)statusCode, httpRequestException.Message);
            return new(AgentApiErrorCategory.NetworkError, null, null, NetworkErrorMessage, true);
        }

        // IOException 覆盖 socket/流被重置等传输层 IO 故障；本地文件 IO 异常也会落此处（有意取舍：宁可多试一次）
        if (ex is SocketException or IOException)
            return new(AgentApiErrorCategory.NetworkError, null, null, NetworkErrorMessage, true);

        return new(AgentApiErrorCategory.Unknown, null, null, UnknownMessage, false);
    }

    /// <summary>
    /// 按 HTTP 状态码与错误文案分类（结构化入口，供单测直接覆盖矩阵）。
    /// </summary>
    /// <param name="status">HTTP 状态码。</param>
    /// <param name="message">服务端错误文案（含错误码字样时优先按码识别）。</param>
    /// <returns>分类结果。</returns>
    public static AgentApiErrorInfo ClassifyHttpStatus(int status, string? message)
    {
        var text = message ?? string.Empty;

        switch (status)
        {
            case 400:
                if (ContainsAny(text, "context_length_exceeded", "maximum context length", "too many tokens", "context length"))
                    return ContextLengthExceeded(status);
                if (ContainsAny(text, "content_filter", "content_policy_violation", "content policy", "flagged"))
                    return ContentFiltered(status);
                if (ContainsAny(text, "not support", "unsupported", "does not support"))
                    return UnsupportedCapability(status);
                return new(AgentApiErrorCategory.BadRequest, status, null, BadRequestMessage, false);

            case 401:
                return new(AgentApiErrorCategory.Authentication, status, "invalid_api_key", AuthenticationMessage, false);

            case 402:
                return new(AgentApiErrorCategory.PaymentRequired, status, null, PaymentRequiredMessage, false);

            case 403:
                if (ContainsAny(text, "insufficient_quota", "quota has been exhausted", "quota exhausted",
                        "exceeded your current quota", "billing"))
                    return QuotaExhausted(status, text);
                if (ContainsAny(text, "invalid_api_key", "incorrect api key"))
                    return new(AgentApiErrorCategory.Authentication, status, "invalid_api_key", AuthenticationMessage, false);
                return new(AgentApiErrorCategory.PermissionDenied, status, null, PermissionDeniedMessage, false);

            case 404:
            case 410:
                return new(AgentApiErrorCategory.ModelNotFound, status, "model_not_found", ModelNotFoundMessage, false);

            case 408:
                return new(AgentApiErrorCategory.Timeout, status, null, TimeoutMessage, true);

            case 413:
                return ContextLengthExceeded(status);

            case 422:
                // 422 既用于「能力不支持」，也被网关/FastAPI 代理用于参数校验失败：先看关键字，默认按参数错误
                return ContainsAny(text, "not support", "unsupported", "does not support")
                    ? UnsupportedCapability(status)
                    : new(AgentApiErrorCategory.BadRequest, status, null, BadRequestMessage, false);

            case 429:
                // 注意：不要用 "billing" 判定配额——OpenAI 限流文案常带 billing 链接，会误判为配额耗尽（不可重试）
                return ContainsAny(text, "insufficient_quota", "quota has been exhausted", "quota exhausted",
                        "exceeded your current quota")
                    ? QuotaExhausted(status, text)
                    : new(AgentApiErrorCategory.RateLimited, status, "rate_limit_exceeded", RateLimitedMessage, true);

            case 500:
            case 502:
            case 503:
            case 504:
                return new(AgentApiErrorCategory.ServerError, status, null, ServerErrorMessage, true);

            default:
                if (status >= 500)
                    return new(AgentApiErrorCategory.ServerError, status, null, ServerErrorMessage, true);
                if (status >= 400)
                    return new(AgentApiErrorCategory.BadRequest, status, null, BadRequestMessage, false);
                return new(AgentApiErrorCategory.Unknown, status, null, UnknownMessage, false);
        }
    }

    /// <summary>
    /// 构造「响应中断」分类结果（流式已输出内容后失败，不可重试）。
    /// </summary>
    /// <param name="status">底层错误状态码（可空）。</param>
    /// <param name="errorCode">底层错误码（可空）。</param>
    /// <returns>分类结果。</returns>
    public static AgentApiErrorInfo CreateStreamInterrupted(int? status = null, string? errorCode = null)
        => new(AgentApiErrorCategory.StreamInterrupted, status, errorCode, StreamInterruptedMessage, false);

    /// <summary>
    /// 计算第 N 次重试前的等待时长：优先遵循 Retry-After，否则指数退避 + ±20% 抖动；
    /// 结果统一受 <see cref="ApiRetryOptions.MaxDelayMs"/> 上限收口。
    /// </summary>
    /// <param name="attempt">即将进行的重试序号（1 表示第一次重试）。</param>
    /// <param name="options">重试配置。</param>
    /// <param name="retryAfter">服务端 Retry-After（可空）。</param>
    /// <returns>等待时长。</returns>
    public static TimeSpan ComputeRetryDelay(int attempt, ApiRetryOptions options, TimeSpan? retryAfter = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        var cap = TimeSpan.FromMilliseconds(Math.Max(1, options.MaxDelayMs));

        if (retryAfter is { } after && after > TimeSpan.Zero)
            return after > cap ? cap : after;

        var baseMs = Math.Max(1, options.BaseDelayMs);
        var factor = options.BackoffFactor <= 0 ? 1 : options.BackoffFactor;
        var ms = baseMs * Math.Pow(factor, Math.Max(0, attempt - 1));

        // 畸形配置（超大 BaseDelayMs × BackoffFactor）会让 Math.Pow 溢出为 Infinity，兜底回退基础延迟
        if (!double.IsFinite(ms) || ms <= 0)
            ms = Math.Min(baseMs, cap.TotalMilliseconds);

        // ±20% 抖动：避免多会话同时重试形成请求尖峰
        var jitter = 1 + (Random.Shared.NextDouble() * 0.4 - 0.2);
        var result = ms * jitter;
        if (!double.IsFinite(result) || result <= 0)
            return TimeSpan.FromMilliseconds(Math.Min(baseMs, cap.TotalMilliseconds));

        // 先在 double 空间收口再转换：result 可能超出 TimeSpan 可表示范围，直接转换会抛 OverflowException
        var cappedMs = Math.Min(result, cap.TotalMilliseconds);
        return TimeSpan.FromMilliseconds(cappedMs);
    }

    /// <summary>
    /// 尝试从响应头提取 Retry-After。
    /// </summary>
    /// <param name="ex">原始异常。</param>
    /// <param name="delay">解析出的等待时长。</param>
    /// <returns>是否解析成功。</returns>
    public static bool TryGetRetryAfter(Exception ex, out TimeSpan delay)
    {
        delay = default;
        if (ex is not ClientResultException clientException)
            return false;

        var headers = clientException.GetRawResponse()?.Headers;
        if (headers is null || !headers.TryGetValue("Retry-After", out var value) || string.IsNullOrWhiteSpace(value))
            return false;

        if (int.TryParse(value, out var seconds) && seconds > 0)
        {
            delay = TimeSpan.FromSeconds(seconds);
            return true;
        }

        if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var when))
        {
            delay = when - DateTimeOffset.UtcNow;
            return delay > TimeSpan.Zero;
        }

        return false;
    }

    /// <summary>
    /// 获取类别的中文短标签（用于重试进度提示）。
    /// </summary>
    /// <param name="category">错误类别。</param>
    /// <returns>中文短标签。</returns>
    public static string GetCategoryLabel(AgentApiErrorCategory category) => category switch
    {
        AgentApiErrorCategory.Authentication => "认证失败",
        AgentApiErrorCategory.PaymentRequired => "余额不足",
        AgentApiErrorCategory.PermissionDenied => "权限不足",
        AgentApiErrorCategory.ModelNotFound => "模型不存在",
        AgentApiErrorCategory.BadRequest => "请求错误",
        AgentApiErrorCategory.ContextLengthExceeded => "上下文超长",
        AgentApiErrorCategory.UnsupportedCapability => "能力不支持",
        AgentApiErrorCategory.RateLimited => "限流",
        AgentApiErrorCategory.QuotaExhausted => "配额耗尽",
        AgentApiErrorCategory.ServerError => "服务端错误",
        AgentApiErrorCategory.NetworkError => "网络错误",
        AgentApiErrorCategory.Timeout => "请求超时",
        AgentApiErrorCategory.StreamInterrupted => "响应中断",
        AgentApiErrorCategory.ContentFiltered => "内容拦截",
        AgentApiErrorCategory.Canceled => "已取消",
        _ => "未识别错误"
    };

    private static AgentApiErrorInfo QuotaExhausted(int status, string message)
    {
        var match = QuotaResetRegex.Match(message);
        var tail = match.Success
            ? $"配额将于 {match.Groups["time"].Value.Trim()} 重置。"
            : "请等待配额刷新。";
        var text = $"API 配额已用尽：当前套餐额度已耗尽，{tail}可切换 Provider/模型或升级套餐后重试。";
        return new(AgentApiErrorCategory.QuotaExhausted, status, "insufficient_quota", text, false);
    }

    private static AgentApiErrorInfo ContextLengthExceeded(int? status)
        => new(AgentApiErrorCategory.ContextLengthExceeded, status, "context_length_exceeded", ContextLengthExceededMessage, false);

    private static AgentApiErrorInfo UnsupportedCapability(int? status)
        => new(AgentApiErrorCategory.UnsupportedCapability, status, null, UnsupportedCapabilityMessage, false);

    private static AgentApiErrorInfo ContentFiltered(int? status)
        => new(AgentApiErrorCategory.ContentFiltered, status, "content_filter", ContentFilteredMessage, false);

    private static bool ContainsAny(string text, params string[] keywords)
        => keywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));
}