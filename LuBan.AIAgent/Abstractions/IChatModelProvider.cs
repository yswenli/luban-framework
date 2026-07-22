namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 聊天模型提供者接口，用于与不同的AI模型进行交互
/// </summary>
public interface IChatModelProvider
{
    /// <summary>
    /// 提供者名称
    /// </summary>
    string ProviderName { get; }
    
    /// <summary>
    /// 异步完成聊天请求并返回完整响应
    /// </summary>
    /// <param name="request">聊天请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天响应</returns>
    Task<ChatResponse> CompleteAsync(ChatRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 异步流式处理聊天请求并返回实时更新
    /// </summary>
    /// <param name="request">聊天请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>流式聊天更新的异步枚举</returns>
    IAsyncEnumerable<StreamingChatUpdate> StreamAsync(ChatRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步获取该提供者支持的模型列表
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>模型信息列表</returns>
    /// <exception cref="NotSupportedException">当提供者不支持获取模型列表时抛出</exception>
    Task<IReadOnlyList<ModelInfo>> GetModelsAsync(CancellationToken cancellationToken = default);
}
