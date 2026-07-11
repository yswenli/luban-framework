using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Core;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace LuBan.AIAgent.Providers.ERNIE;

/// <summary>
/// ERNIE 聊天模型提供者，用于与百度 ERNIE API 进行交互
/// </summary>
public class ERNIEChatModelProvider : OpenAICompatibleProviderBase
{
    /// <summary>
    /// 提供者名称
    /// </summary>
    public override string ProviderName => "ernie";

    /// <summary>
    /// ERNIE 配置选项
    /// </summary>
    private readonly ERNIEOptions _options;
    
    /// <summary>
    /// HTTP 客户端
    /// </summary>
    private readonly HttpClient _httpClient;
    
    /// <summary>
    /// 访问令牌
    /// </summary>
    private string _accessToken = string.Empty;
    
    /// <summary>
    /// 令牌过期时间
    /// </summary>
    private DateTime _tokenExpiry = DateTime.MinValue;
    
    /// <summary>
    /// 令牌锁
    /// </summary>
    private readonly object _tokenLock = new object();

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="httpClient">HTTP 客户端</param>
    /// <param name="options">ERNIE 配置选项</param>
    /// <param name="logger">日志记录器</param>
    public ERNIEChatModelProvider(HttpClient httpClient, Microsoft.Extensions.Options.IOptions<ERNIEOptions> options, ILogger<ERNIEChatModelProvider>? logger = null)
        : base(httpClient, logger)
    {
        _httpClient = httpClient;
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.Endpoint))
        {
            throw new ArgumentException("ERNIE endpoint is required.", nameof(options));
        }

        if (string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            throw new ArgumentException("ERNIE SecretKey is required.", nameof(options));
        }

        if (string.IsNullOrWhiteSpace(_options.TokenEndpoint))
        {
            throw new ArgumentException("ERNIE TokenEndpoint is required.", nameof(options));
        }
    }

    /// <summary>
    /// 异步完成聊天请求
    /// </summary>
    /// <param name="request">聊天请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天响应</returns>
    public new async Task<ChatResponse> CompleteAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureAccessTokenAsync(cancellationToken);
        return await base.CompleteAsync(request, cancellationToken);
    }

    /// <summary>
    /// 异步流式聊天请求
    /// </summary>
    /// <param name="request">聊天请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>流式聊天更新的异步枚举</returns>
    public new async IAsyncEnumerable<StreamingChatUpdate> StreamAsync(ChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await EnsureAccessTokenAsync(cancellationToken);
        await foreach (var update in base.StreamAsync(request, cancellationToken))
        {
            yield return update;
        }
    }

    /// <summary>
    /// 创建提供者请求
    /// </summary>
    /// <param name="request">聊天请求</param>
    /// <param name="stream">是否流式响应</param>
    /// <returns>提供者请求对象</returns>
    protected override object CreateProviderRequest(ChatRequest request, bool stream)
        => CreateBaseRequest(request, stream, includeModel: true);

    /// <summary>
    /// 构建相对 URL
    /// </summary>
    /// <param name="modelOrDeployment">模型或部署名称</param>
    /// <returns>相对 URL</returns>
    protected override string BuildRelativeUrl(string modelOrDeployment)
        => $"chat/completions";

    /// <summary>
    /// 获取无效响应消息
    /// </summary>
    /// <returns>无效响应消息</returns>
    protected override string GetInvalidResponseMessage()
        => "Invalid response from ERNIE";

    /// <summary>
    /// 获取基础URL
    /// </summary>
    /// <returns>基础URL</returns>
    protected override string GetBaseUrl()
        => EnsureTrailingSlash(_options.Endpoint);

    /// <summary>
    /// 确保访问令牌有效
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>任务</returns>
    private async Task EnsureAccessTokenAsync(CancellationToken cancellationToken)
    {
        lock (_tokenLock)
        {
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry)
            {
                // 令牌有效，直接返回
                return;
            }
        }

        // 令牌无效或过期，获取新令牌
        var newToken = await GetAccessTokenAsync(cancellationToken);
        var expiry = DateTime.UtcNow.AddHours(1); // 假设令牌有效期为1小时

        lock (_tokenLock)
        {
            _accessToken = newToken;
            _tokenExpiry = expiry;
            // 更新 HTTP 客户端的授权头
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");
        }
    }

    /// <summary>
    /// 获取访问令牌
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>访问令牌</returns>
    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        using var client = new HttpClient();
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", _options.SecretKey.Split('.')[0]), // 假设 SecretKey 格式为 client_id.client_secret
            new KeyValuePair<string, string>("client_secret", _options.SecretKey.Split('.')[1])
        });

        var response = await client.PostAsync(_options.TokenEndpoint, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseJson);

        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
        {
            throw new Exception("Failed to get access token from ERNIE");
        }

        return tokenResponse.AccessToken;
    }

    /// <summary>
    /// 确保 URL 以斜杠结尾
    /// </summary>
    /// <param name="endpoint">端点 URL</param>
    /// <returns>以斜杠结尾的 URL</returns>
    private static string EnsureTrailingSlash(string endpoint)
        => endpoint.EndsWith('/') ? endpoint : endpoint + "/";

    /// <summary>
    /// 令牌响应
    /// </summary>
    private class TokenResponse
    {
        /// <summary>
        /// 访问令牌
        /// </summary>
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// 令牌类型
        /// </summary>
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;

        /// <summary>
        /// 过期时间（秒）
        /// </summary>
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}