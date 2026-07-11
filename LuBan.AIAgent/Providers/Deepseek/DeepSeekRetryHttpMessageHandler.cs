using System.Net;
using Microsoft.Extensions.Logging;

namespace LuBan.AIAgent.Providers.DeepSeek;

/// <summary>
/// DeepSeek 重试 HTTP 消息处理器，用于处理 DeepSeek 请求的重试逻辑
/// </summary>
public class DeepSeekRetryHttpMessageHandler : DelegatingHandler
{
    /// <summary>
    /// DeepSeek 配置选项
    /// </summary>
    private readonly DeepSeekOptions _options;
    
    /// <summary>
    /// 日志记录器
    /// </summary>
    private readonly ILogger<DeepSeekRetryHttpMessageHandler>? _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options">DeepSeek 配置选项</param>
    /// <param name="logger">日志记录器</param>
    public DeepSeekRetryHttpMessageHandler(DeepSeekOptions options, ILogger<DeepSeekRetryHttpMessageHandler>? logger = null)
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
        byte[]? requestContent = null;
        string? contentType = null;

        // 保存请求内容，以便重试时重新创建请求
        if (request.Content != null)
        {
            requestContent = await request.Content.ReadAsByteArrayAsync(cancellationToken);
            contentType = request.Content.Headers.ContentType?.ToString();
        }

        while (true)
        {
            // 为每次尝试创建新的请求对象
            var currentRequest = CloneRequest(request, requestContent, contentType);
            
            try
            {
                // 设置认证头
                currentRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiKey);
                
                using var timeoutCts = new CancellationTokenSource(_options.RequestTimeout);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);

                response = await base.SendAsync(currentRequest, linkedCts.Token);

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
    /// 克隆 HTTP 请求消息
    /// </summary>
    /// <param name="request">原始请求</param>
    /// <param name="contentBytes">请求内容字节数组</param>
    /// <param name="contentType">内容类型</param>
    /// <returns>克隆的请求</returns>
    private static HttpRequestMessage CloneRequest(HttpRequestMessage request, byte[]? contentBytes, string? contentType)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version
        };

        // 复制头信息
        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // 复制内容
        if (contentBytes != null)
        {
            clone.Content = new ByteArrayContent(contentBytes);
            if (!string.IsNullOrEmpty(contentType))
            {
                try
                {
                    // 尝试解析完整的Content-Type字符串
                    clone.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(contentType);
                }
                catch
                {
                    // 如果解析失败，使用默认的application/json
                    clone.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
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