using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Core;
using Microsoft.Extensions.Logging;

namespace LuBan.AIAgent.Providers.OpenAI;

/// <summary>
/// OpenAI聊天模型提供者，用于与OpenAI API进行交互
/// </summary>
public class OpenAIChatModelProvider : OpenAICompatibleProviderBase
{
    /// <summary>
    /// 提供者名称
    /// </summary>
    public override string ProviderName => "openai";

    /// <summary>
    /// OpenAI配置选项
    /// </summary>
    private readonly OpenAIOptions _options;

    /// <summary>
    /// 初始化OpenAIChatModelProvider实例
    /// </summary>
    /// <param name="httpClient">HTTP客户端</param>
    /// <param name="options">OpenAI选项</param>
    /// <param name="logger">日志记录器</param>
    public OpenAIChatModelProvider(
        HttpClient httpClient,
        Microsoft.Extensions.Options.IOptions<OpenAIOptions> options,
        ILogger<OpenAIChatModelProvider>? logger = null)
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
        => "OpenAI返回了无效响应";

    /// <summary>
    /// 获取基础URL
    /// </summary>
    /// <returns>基础URL</returns>
    protected override string GetBaseUrl()
        => _options.BaseUrl ?? "https://api.openai.com/v1/";
}
