using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Core;
using Microsoft.Extensions.Logging;

namespace LuBan.AIAgent.Providers.Qwen;

/// <summary>
/// Qwen 聊天模型提供者，用于与 Qwen API 进行交互
/// </summary>
public class QwenChatModelProvider : OpenAICompatibleProviderBase
{
    /// <summary>
    /// 提供者名称
    /// </summary>
    public override string ProviderName => "qwen";
    
    /// <summary>
    /// Qwen 配置选项
    /// </summary>
    private readonly QwenOptions _options;

    /// <summary>
    /// 初始化 QwenChatModelProvider 实例
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="httpClient">HTTP 客户端</param>
    /// <param name="options">Qwen 配置选项</param>
    /// <param name="logger">日志记录器</param>
    public QwenChatModelProvider(HttpClient httpClient, Microsoft.Extensions.Options.IOptions<QwenOptions> options, ILogger<QwenChatModelProvider>? logger = null)
        : base(httpClient, logger)
    {
        _options = options.Value;
    }
    
    /// <summary>
    /// 获取基础URL
    /// </summary>
    /// <returns>基础URL</returns>
    protected override string GetBaseUrl()
    {
        return _options.BaseUrl ?? "https://dashscope.aliyuncs.com/compatible-mode/v1/";
    }

    /// <summary>
    /// 创建提供者请求
    /// </summary>
    /// <param name="request">聊天请求</param>
    /// <param name="stream">是否流式</param>
    /// <returns>提供者请求对象</returns>
    protected override object CreateProviderRequest(ChatRequest request, bool stream)
        => CreateBaseRequest(request, stream, includeModel: true);

    /// <summary>
    /// 构建相对 URL
    /// </summary>
    /// <param name="modelOrDeployment">模型或部署</param>
    /// <returns>相对 URL</returns>
    protected override string BuildRelativeUrl(string modelOrDeployment)
        => "chat/completions";

    /// <summary>
    /// 获取无效响应消息
    /// </summary>
    /// <returns>无效响应消息</returns>
    protected override string GetInvalidResponseMessage()
        => "Qwen 返回了无效响应";
}