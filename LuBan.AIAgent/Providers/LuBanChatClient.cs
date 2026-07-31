/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Providers
*文件名： LuBanChatClient
*版本号： V1.0.0.0
*唯一标识：1cabdfb3-a008-4287-8f43-e1b805ec28de
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：LuBan 聊天客户端
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：LuBan 聊天客户端
*
*****************************************************************************/
namespace LuBan.AIAgent.Providers;

/// <summary>
/// LuBan 聊天客户端，根据模型 ID 路由到不同的 Provider
/// </summary>
public class LuBanChatClient : IChatClient
{
    private readonly Dictionary<string, IChatClient> _providers;
    private readonly string _defaultProvider;
    private int _disposedInt;

    /// <summary>
    /// 创建 LuBanChatClient 实例
    /// </summary>
    /// <param name="providers">Provider 字典</param>
    /// <param name="defaultProvider">默认 Provider 名称</param>
    public LuBanChatClient(
        IEnumerable<KeyValuePair<string, IChatClient>> providers,
        string defaultProvider = "openai")
    {
        _providers = providers?.ToDictionary(p => p.Key.ToLowerInvariant(), p => p.Value)
            ?? new Dictionary<string, IChatClient>();
        _defaultProvider = defaultProvider.ToLowerInvariant();
    }

    /// <summary>
    /// 获取聊天响应
    /// </summary>
    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var provider = GetProvider(options?.ModelId);
        return await provider.GetResponseAsync(messages, options, cancellationToken);
    }

    /// <summary>
    /// 获取流式聊天响应
    /// </summary>
    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var provider = GetProvider(options?.ModelId);
        await foreach (var update in provider.GetStreamingResponseAsync(messages, options, cancellationToken))
        {
            yield return update;
        }
    }

    /// <summary>
    /// 获取服务
    /// </summary>
    public object? GetService(Type serviceType, object? key = null)
    {
        foreach (var provider in _providers.Values)
        {
            var service = provider.GetService(serviceType, key);
            if (service != null)
                return service;
        }
        return null;
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposedInt, 1) == 0)
        {
            foreach (var provider in _providers.Values)
            {
                try { provider.Dispose(); } catch { }
            }
            _providers.Clear();
        }
        GC.SuppressFinalize(this);
    }

    private IChatClient GetProvider(string? modelId)
    {
        var providerName = _defaultProvider;
        
        if (!string.IsNullOrEmpty(modelId))
        {
            var parts = modelId.Split(':', 2);
            if (parts.Length == 2)
            {
                providerName = parts[0].ToLowerInvariant();
            }
        }

        if (_providers.TryGetValue(providerName, out var provider))
        {
            return provider;
        }

        throw new InvalidOperationException($"Provider '{providerName}' not found");
    }
}
