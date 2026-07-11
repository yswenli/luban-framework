using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using LuBan.AIAgent.Abstractions;

namespace LuBan.AIAgent.Core;

/// <summary>
/// 聊天客户端，用于管理和路由聊天请求到不同的模型提供者
/// </summary>
public class ChatClient : IChatClient
{
    private readonly Dictionary<string, IChatModelProvider> _providers = new();
    private readonly ILogger<ChatClient>? _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public ChatClient(ILogger<ChatClient>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// 注册聊天模型提供者
    /// </summary>
    /// <param name="provider">聊天模型提供者</param>
    public void RegisterProvider(IChatModelProvider provider)
    {
        _providers[provider.ProviderName.ToLowerInvariant()] = provider;
        _logger?.LogInformation("Registered provider: {ProviderName}", provider.ProviderName);
    }

    /// <summary>
    /// 异步完成聊天请求并返回完整响应
    /// </summary>
    /// <param name="request">聊天请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天响应</returns>
    public async Task<ChatResponse> CompleteAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        var (providerName, modelId) = ParseModelId(request.ModelId);
        _logger?.LogInformation("Routing request to provider {ProviderName} for model {ModelId}", providerName, modelId);
        
        if (!_providers.TryGetValue(providerName.ToLowerInvariant(), out var provider))
        {
            _logger?.LogError("Provider '{ProviderName}' not found", providerName);
            return new ChatResponse
            {
                IsSuccess = false,
                ErrorMessage = $"Provider '{providerName}' not found"
            };
        }

        var adjustedRequest = request with { ModelId = modelId };
        return await provider.CompleteAsync(adjustedRequest, cancellationToken);
    }

    /// <summary>
    /// 异步流式处理聊天请求并返回实时更新
    /// </summary>
    /// <param name="request">聊天请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>流式聊天更新的异步枚举</returns>
    public async IAsyncEnumerable<StreamingChatUpdate> StreamAsync(ChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var (providerName, modelId) = ParseModelId(request.ModelId);
        _logger?.LogInformation("Routing streaming request to provider {ProviderName} for model {ModelId}", providerName, modelId);
        
        if (!_providers.TryGetValue(providerName.ToLowerInvariant(), out var provider))
        {
            _logger?.LogError("Provider '{ProviderName}' not found for streaming", providerName);
            yield break;
        }

        var adjustedRequest = request with { ModelId = modelId };
        await foreach (var update in provider.StreamAsync(adjustedRequest, cancellationToken).WithCancellation(cancellationToken))
        {
            yield return update;
        }
    }

    /// <summary>
    /// 解析模型ID，提取提供者名称和模型ID
    /// </summary>
    /// <param name="modelId">模型ID，格式为"provider:model"或单独的模型名称</param>
    /// <returns>提供者名称和模型ID的元组</returns>
    private static (string ProviderName, string ModelId) ParseModelId(string modelId)
    {
        var parts = modelId.Split(':', 2);
        if (parts.Length == 2)
        {
            return (parts[0], parts[1]);
        }
        return ("openai", modelId);
    }
}
