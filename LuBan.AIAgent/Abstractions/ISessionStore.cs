namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 会话存储接口，用于存储和管理对话状态
/// </summary>
public interface ISessionStore
{
    /// <summary>
    /// 根据会话ID异步获取对话状态
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>对话状态，如果不存在则返回null</returns>
    Task<ConversationState?> GetAsync(string sessionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 异步保存对话状态
    /// </summary>
    /// <param name="state">对话状态</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>任务</returns>
    Task SaveAsync(ConversationState state, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 根据会话ID异步删除对话状态
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>任务</returns>
    Task DeleteAsync(string sessionId, CancellationToken cancellationToken = default);
}
