using System.Collections.Concurrent;
using LuBan.AIAgent.Abstractions;
using Microsoft.Extensions.Logging;

namespace LuBan.AIAgent.Core;

/// <summary>
/// 内存会话存储，将会话状态存储在内存中
/// </summary>
public class InMemorySessionStore : ISessionStore
{
    private readonly ConcurrentDictionary<string, ConversationState> _states = new(StringComparer.Ordinal);
    private readonly ILogger<InMemorySessionStore>? _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public InMemorySessionStore(ILogger<InMemorySessionStore>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// 异步获取会话状态
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>会话状态，若不存在则返回null</returns>
    public Task<ConversationState?> GetAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        _states.TryGetValue(sessionId, out var state);
        _logger?.LogDebug("SessionStore GetAsync. SessionId={SessionId}, Found={Found}", sessionId, state != null);
        return Task.FromResult(state);
    }

    /// <summary>
    /// 异步保存会话状态
    /// </summary>
    /// <param name="state">会话状态</param>
    /// <param name="cancellationToken">取消令牌</param>
    public Task SaveAsync(ConversationState state, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (string.IsNullOrWhiteSpace(state.SessionId))
        {
            throw new ArgumentException("SessionId is required", nameof(state));
        }

        _states[state.SessionId] = state;
        _logger?.LogDebug(
            "SessionStore SaveAsync. SessionId={SessionId}, HistoryCount={HistoryCount}, ActiveSkill={ActiveSkill}, TotalSessions={TotalSessions}",
            state.SessionId,
            state.History?.Count ?? 0,
            state.ActiveSkill,
            _states.Count);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 异步删除会话状态
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    public Task DeleteAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        _states.TryRemove(sessionId, out _);
        _logger?.LogDebug("SessionStore DeleteAsync. SessionId={SessionId}, TotalSessions={TotalSessions}", sessionId, _states.Count);
        return Task.CompletedTask;
    }
}
