namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 聊天客户端接口，负责与AI模型进行交互
/// </summary>
public interface IChatClient
{
    /// <summary>
    /// 异步完成聊天请求并返回完整响应
    /// </summary>
    /// <param name="request">聊天请求对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天响应对象</returns>
    Task<ChatResponse> CompleteAsync(ChatRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 异步流式处理聊天请求并返回实时更新
    /// </summary>
    /// <param name="request">聊天请求对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>流式聊天更新的异步枚举</returns>
    IAsyncEnumerable<StreamingChatUpdate> StreamAsync(ChatRequest request, CancellationToken cancellationToken = default);
}
