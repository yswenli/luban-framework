using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Core;
using Microsoft.Extensions.Logging;

namespace LuBan.AIAgent.Providers.Doubao;

/// <summary>
/// Doubao聊天模型提供者，用于与Doubao API进行交互
/// </summary>
public class DoubaoChatModelProvider : OpenAICompatibleProviderBase
{
    /// <summary>
    /// 提供者名称
    /// </summary>
    public override string ProviderName => "doubao";

    /// <summary>
    /// Doubao 配置选项
    /// </summary>
    private readonly DoubaoOptions _options;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="httpClient">HTTP 客户端</param>
    /// <param name="options">Doubao 配置选项</param>
    /// <param name="logger">日志记录器</param>
    public DoubaoChatModelProvider(HttpClient httpClient, Microsoft.Extensions.Options.IOptions<DoubaoOptions> options, ILogger<DoubaoChatModelProvider>? logger = null)
        : base(httpClient, logger)
    {
        _options = options.Value;
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
    /// 构建相对URL
    /// </summary>
    /// <param name="modelOrDeployment">模型或部署</param>
    /// <returns>相对URL</returns>
    protected override string BuildRelativeUrl(string modelOrDeployment)
        => "chat/completions";

    /// <summary>
    /// 获取无效响应消息
    /// </summary>
    /// <returns>无效响应消息</returns>
    protected override string GetInvalidResponseMessage()
        => "Doubao返回了无效响应";

    /// <summary>
    /// 获取基础URL
    /// </summary>
    /// <returns>基础URL</returns>
    protected override string GetBaseUrl()
        => _options.BaseUrl ?? "https://ark.cn-beijing.volces.com/api/v3/";
}