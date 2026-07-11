namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 聊天会话接口，用于管理聊天历史和发送消息
/// </summary>
public interface IChatSession
{
    /// <summary>
    /// 聊天历史记录
    /// </summary>
    IReadOnlyList<ChatMessage> History { get; }
    
    /// <summary>
    /// 添加消息到历史记录
    /// </summary>
    /// <param name="message">聊天消息</param>
    void AddMessage(ChatMessage message);
    
    /// <summary>
    /// 清除聊天历史记录
    /// </summary>
    void ClearHistory();
    
    /// <summary>
    /// 异步发送消息并返回响应
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="options">聊天选项</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天响应</returns>
    Task<ChatResponse> SendAsync(string message, ChatOptions? options = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 异步发送消息并返回聊天回合结果
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="options">聊天选项</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天回合结果</returns>
    Task<ChatTurnResult> SendTurnAsync(string message, ChatOptions? options = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 异步继续聊天回合
    /// </summary>
    /// <param name="options">聊天选项</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天回合结果</returns>
    Task<ChatTurnResult> ContinueAsync(ChatOptions? options = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 异步流式发送消息并返回实时更新
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="options">聊天选项</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天回合流更新的异步枚举</returns>
    IAsyncEnumerable<ChatTurnStreamUpdate> StreamTurnAsync(string message, ChatOptions? options = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 异步流式继续聊天回合并返回实时更新
    /// </summary>
    /// <param name="options">聊天选项</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天回合流更新的异步枚举</returns>
    IAsyncEnumerable<ChatTurnStreamUpdate> ContinueStreamAsync(ChatOptions? options = null, CancellationToken cancellationToken = default);
}
