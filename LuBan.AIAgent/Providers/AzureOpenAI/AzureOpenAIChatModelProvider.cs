using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Core;
using Microsoft.Extensions.Logging;

namespace LuBan.AIAgent.Providers.AzureOpenAI;

/// <summary>
/// Azure OpenAI 聊天模型提供者，用于与 Azure OpenAI API 进行交互
/// </summary>
public class AzureOpenAIChatModelProvider : OpenAICompatibleProviderBase
{
    /// <summary>
    /// 提供者名称
    /// </summary>
    public override string ProviderName => "azure-openai";

    /// <summary>
    /// Azure OpenAI 配置选项
    /// </summary>
    private readonly AzureOpenAIOptions _options;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="httpClient">HTTP 客户端</param>
    /// <param name="options">Azure OpenAI 配置选项</param>
    /// <param name="logger">日志记录器</param>
    public AzureOpenAIChatModelProvider(HttpClient httpClient, Microsoft.Extensions.Options.IOptions<AzureOpenAIOptions> options, ILogger<AzureOpenAIChatModelProvider>? logger = null)
        : base(httpClient, logger)
    {
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.Endpoint))
        {
            throw new ArgumentException("Azure OpenAI endpoint is required.", nameof(options));
        }
    }

    /// <summary>
    /// 创建提供者请求
    /// </summary>
    /// <param name="request">聊天请求</param>
    /// <param name="stream">是否流式响应</param>
    /// <returns>提供者请求对象</returns>
    protected override object CreateProviderRequest(ChatRequest request, bool stream)
        => CreateBaseRequest(request, stream, includeModel: false);

    /// <summary>
    /// 构建相对 URL
    /// </summary>
    /// <param name="modelOrDeployment">模型或部署名称</param>
    /// <returns>相对 URL</returns>
    protected override string BuildRelativeUrl(string modelOrDeployment)
        => $"openai/deployments/{Uri.EscapeDataString(modelOrDeployment)}/chat/completions?api-version={Uri.EscapeDataString(_options.ApiVersion)}";

    /// <summary>
    /// 获取无效响应消息
    /// </summary>
    /// <returns>无效响应消息</returns>
    protected override string GetInvalidResponseMessage()
        => "Invalid response from Azure OpenAI";

    /// <summary>
    /// 获取基础URL
    /// </summary>
    /// <returns>基础URL</returns>
    protected override string GetBaseUrl()
        => EnsureTrailingSlash(_options.Endpoint);

    /// <summary>
    /// 异步获取该提供者支持的模型列表
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>模型信息列表</returns>
    /// <exception cref="NotSupportedException">Azure OpenAI 不支持获取模型列表</exception>
    public override Task<IReadOnlyList<ModelInfo>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Azure OpenAI 不支持获取模型列表");
    }

    /// <summary>
    /// 确保 URL 以斜杠结尾
    /// </summary>
    /// <param name="endpoint">端点 URL</param>
    /// <returns>以斜杠结尾的 URL</returns>
    private static string EnsureTrailingSlash(string endpoint)
        => endpoint.EndsWith('/') ? endpoint : endpoint + "/";
}
