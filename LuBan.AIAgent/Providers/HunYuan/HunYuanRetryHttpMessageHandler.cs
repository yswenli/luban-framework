using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace LuBan.AIAgent.Providers.HunYuan;

/// <summary>
/// HunYuan 重试 HTTP 消息处理器，用于处理 HunYuan 请求的重试逻辑
/// </summary>
public class HunYuanRetryHttpMessageHandler : DelegatingHandler
{
    /// <summary>
    /// HunYuan 配置选项
    /// </summary>
    private readonly HunYuanOptions _options;
    
    /// <summary>
    /// 日志记录器
    /// </summary>
    private readonly ILogger<HunYuanRetryHttpMessageHandler>? _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options">HunYuan 配置选项</param>
    /// <param name="logger">日志记录器</param>
    public HunYuanRetryHttpMessageHandler(HunYuanOptions options, ILogger<HunYuanRetryHttpMessageHandler>? logger = null)
    {
        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// 异步发送 HTTP 请求
    /// </summary>
    /// <param name="request">HTTP 请求消息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>HTTP 响应消息</returns>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage? response = null;
        int retryAttempt = 0;

        while (true)
        {
            try
            {
                using var timeoutCts = new CancellationTokenSource(_options.RequestTimeout);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);

                var clonedRequest = await CloneRequest(request);
                response = await base.SendAsync(clonedRequest, linkedCts.Token);

                if (IsTransientFailure(response.StatusCode))
                {
                    if (retryAttempt < _options.MaxRetryCount)
                    {
                        retryAttempt++;
                        var delay = CalculateDelay(retryAttempt);
                        _logger?.LogWarning(
                            "Request failed with status code {StatusCode}. Retrying attempt {RetryAttempt}/{MaxRetryCount} after {Delay}ms",
                            response.StatusCode, retryAttempt, _options.MaxRetryCount, delay.TotalMilliseconds);
                        
                        await Task.Delay(delay, cancellationToken);
                        continue;
                    }
                    _logger?.LogError(
                        "Request failed with status code {StatusCode} after {MaxRetryCount} retries",
                        response.StatusCode, _options.MaxRetryCount);
                }

                return response;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                if (retryAttempt < _options.MaxRetryCount)
                {
                    retryAttempt++;
                    var delay = CalculateDelay(retryAttempt);
                    _logger?.LogWarning(
                        "Request timed out. Retrying attempt {RetryAttempt}/{MaxRetryCount} after {Delay}ms",
                        retryAttempt, _options.MaxRetryCount, delay.TotalMilliseconds);
                    
                    await Task.Delay(delay, cancellationToken);
                    continue;
                }
                _logger?.LogError("Request timed out after {MaxRetryCount} retries", _options.MaxRetryCount);
                throw;
            }
            catch (HttpRequestException ex)
            {
                if (retryAttempt < _options.MaxRetryCount)
                {
                    retryAttempt++;
                    var delay = CalculateDelay(retryAttempt);
                    _logger?.LogWarning(
                        ex, "Network error occurred. Retrying attempt {RetryAttempt}/{MaxRetryCount} after {Delay}ms",
                        retryAttempt, _options.MaxRetryCount, delay.TotalMilliseconds);
                    
                    await Task.Delay(delay, cancellationToken);
                    continue;
                }
                _logger?.LogError(ex, "Network error occurred after {MaxRetryCount} retries", _options.MaxRetryCount);
                throw;
            }
        }
    }

    /// <summary>
    /// 克隆 HTTP 请求
    /// </summary>
    /// <param name="request">原始请求</param>
    /// <returns>克隆的请求</returns>
    private static async Task<HttpRequestMessage> CloneRequest(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri!);

        // 复制请求头
        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // 复制内容
        if (request.Content != null)
        {
            var contentBytes = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(contentBytes);

            // 复制内容头
            if (request.Content.Headers != null)
            {
                foreach (var header in request.Content.Headers)
                {
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        return clone;
    }

    /// <summary>
    /// 检查是否为暂时性失败
    /// </summary>
    /// <param name="statusCode">HTTP 状态码</param>
    /// <returns>是否为暂时性失败</returns>
    private static bool IsTransientFailure(HttpStatusCode statusCode)
    {
        return statusCode == HttpStatusCode.TooManyRequests ||
               (int)statusCode >= 500 && (int)statusCode < 600;
    }

    /// <summary>
    /// 计算重试延迟
    /// </summary>
    /// <param name="retryAttempt">重试次数</param>
    /// <returns>重试延迟</returns>
    private TimeSpan CalculateDelay(int retryAttempt)
    {
        var delay = TimeSpan.FromMilliseconds(
            _options.InitialRetryDelay.TotalMilliseconds * Math.Pow(2, retryAttempt - 1));
        return delay;
    }
}
