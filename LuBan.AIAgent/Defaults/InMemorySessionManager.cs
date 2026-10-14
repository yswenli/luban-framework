/***************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Defaults
*文件名： InMemorySessionManager
*创建时间：2026/10/14
*描述：会话管理的内存实现（进程内，不持久化），供最小宿主开箱使用
*
*****************************************************************************/
namespace LuBan.AIAgent.Defaults;

/// <summary>
/// 会话管理的内存实现（进程内，不持久化）。
/// <para>
/// 语义对齐 SQLite 实现：会话与消息存于进程内字典；摘要消息（role = "summary"）
/// 仅计入 token、不计入消息数；活跃消息为未被标记压缩的消息（含摘要）。
/// </para>
/// <para>
/// 默认以 Singleton 注册（<c>AddLuBanAgentDefaults</c>），使同一进程内跨作用域的
/// 会话历史保持连续。需要持久化 / 多用户隔离请注册自定义
/// <see cref="ISessionManager"/>（<c>TryAdd</c> 语义允许覆盖默认注册）。
/// </para>
/// </summary>
public sealed class InMemorySessionManager : ISessionManager
{
    private readonly Lock _gate = new();
    private readonly Dictionary<string, SessionRecord> _sessions = new(StringComparer.Ordinal);
    private long _nextMessageId;

    /// <inheritdoc />
    public SessionInfo? CurrentSession { get; private set; }

    /// <inheritdoc />
    public Task<SessionInfo> CreateSessionAsync(string? title = null, string? workspaceId = null)
    {
        var info = new SessionInfo
        {
            SessionId = Guid.NewGuid().ToString("N"),
            Title = title ?? "新对话",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        lock (_gate)
        {
            _sessions[info.SessionId] = new SessionRecord(info, workspaceId);
            CurrentSession = info;
        }

        return Task.FromResult(info);
    }

    /// <inheritdoc />
    public Task<SessionInfo?> GetSessionAsync(string sessionId)
    {
        lock (_gate)
        {
            return Task.FromResult(_sessions.TryGetValue(sessionId, out var record) ? record.Info : null);
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<SessionInfo>> GetUserSessionsAsync()
    {
        lock (_gate)
        {
            var list = _sessions.Values
                .Select(r => r.Info)
                .OrderByDescending(s => s.CreatedAt)
                .ToList();
            return Task.FromResult<IEnumerable<SessionInfo>>(list);
        }
    }

    /// <inheritdoc />
    public Task UpdateSessionTitleAsync(string sessionId, string title)
    {
        lock (_gate)
        {
            if (_sessions.TryGetValue(sessionId, out var record))
            {
                record.Info.Title = title;
                record.Info.UpdatedAt = DateTime.Now;
            }
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteSessionAsync(string sessionId)
    {
        lock (_gate)
        {
            _sessions.Remove(sessionId);
            if (string.Equals(CurrentSession?.SessionId, sessionId, StringComparison.Ordinal))
                CurrentSession = null;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<SessionMessage> AddMessageAsync(string sessionId, string role, string content, int? tokens = null, string? thinking = null, string? attachments = null, string? toolCalls = null)
    {
        var message = new SessionMessage
        {
            SessionId = sessionId,
            Role = role,
            Content = content,
            Thinking = thinking,
            Attachments = attachments,
            ToolCalls = toolCalls,
            Tokens = tokens,
            CreatedAt = DateTime.Now
        };

        lock (_gate)
        {
            if (!_sessions.TryGetValue(sessionId, out var record))
            {
                // 会话不存在：与持久化实现一致，返回未落库的消息（Id = 0），不抛异常
                return Task.FromResult(message);
            }

            message.Id = ++_nextMessageId;
            record.Messages.Add(message);
            record.Info.UpdatedAt = DateTime.Now;
            record.Info.TotalTokens += tokens ?? 0;
            if (!string.Equals(role, "summary", StringComparison.Ordinal))
                record.Info.MessageCount++;

            return Task.FromResult(Clone(message));
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<SessionMessage>> GetMessagesAsync(string sessionId, int? limit = null)
    {
        lock (_gate)
        {
            if (!_sessions.TryGetValue(sessionId, out var record))
                return Task.FromResult<IEnumerable<SessionMessage>>(Array.Empty<SessionMessage>());

            var query = record.Messages.OrderBy(m => m.Id);
            var list = (limit.HasValue ? query.Take(limit.Value) : query)
                .Select(Clone)
                .ToList();
            return Task.FromResult<IEnumerable<SessionMessage>>(list);
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<SessionMessage>> GetLatestMessagesAsync(string sessionId, int count)
    {
        lock (_gate)
        {
            if (!_sessions.TryGetValue(sessionId, out var record))
                return Task.FromResult<IEnumerable<SessionMessage>>(Array.Empty<SessionMessage>());

            var list = record.Messages
                .OrderByDescending(m => m.Id)
                .Take(count)
                .OrderBy(m => m.Id)
                .Select(Clone)
                .ToList();
            return Task.FromResult<IEnumerable<SessionMessage>>(list);
        }
    }

    /// <inheritdoc />
    public Task ClearMessagesAsync(string sessionId)
    {
        lock (_gate)
        {
            if (_sessions.TryGetValue(sessionId, out var record))
            {
                record.Messages.Clear();
                record.CompactedIds.Clear();
                record.Info.MessageCount = 0;
                record.Info.TotalTokens = 0;
                record.Info.UpdatedAt = DateTime.Now;
            }
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<SessionStats> GetSessionStatsAsync(string sessionId)
    {
        lock (_gate)
        {
            if (!_sessions.TryGetValue(sessionId, out var record))
                return Task.FromResult(new SessionStats());

            var counted = record.Messages
                .Where(m => !string.Equals(m.Role, "summary", StringComparison.Ordinal))
                .ToList();

            var totalTokens = record.Messages.Sum(m => m.Tokens ?? 0);
            return Task.FromResult(new SessionStats
            {
                TotalMessages = counted.Count,
                UserMessages = counted.Count(m => string.Equals(m.Role, "user", StringComparison.Ordinal)),
                AssistantMessages = counted.Count(m => string.Equals(m.Role, "assistant", StringComparison.Ordinal)),
                TotalTokens = totalTokens,
                AverageMessageLength = counted.Count > 0 ? totalTokens / (double)counted.Count : 0
            });
        }
    }

    /// <inheritdoc />
    public Task SetCurrentSessionAsync(string sessionId)
    {
        lock (_gate)
        {
            CurrentSession = _sessions.TryGetValue(sessionId, out var record) ? record.Info : null;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void ClearCurrentSession()
    {
        lock (_gate)
        {
            CurrentSession = null;
        }
    }

    /// <inheritdoc />
    public async Task ClearAllSessionsAsync(string? workspaceId = null)
    {
        lock (_gate)
        {
            _sessions.Clear();
            CurrentSession = null;
        }

        // 保持「有当前工作区必有当前会话」不变量：清空后重建新会话
        if (!string.IsNullOrEmpty(workspaceId))
            await CreateSessionAsync(title: "新会话", workspaceId: workspaceId);
    }

    /// <inheritdoc />
    public Task<GlobalSessionStats> GetGlobalStatsAsync(int? days = null)
    {
        lock (_gate)
        {
            var since = days.HasValue ? DateTime.Now.AddDays(-days.Value) : (DateTime?)null;
            var sessions = _sessions.Values
                .Where(r => !since.HasValue || r.Info.CreatedAt >= since.Value)
                .ToList();

            var messages = sessions.Sum(r => r.Info.MessageCount);
            var tokens = sessions.Sum(r => (long)r.Info.TotalTokens);
            var earliest = sessions.Count > 0 ? sessions.Min(r => r.Info.CreatedAt) : (DateTime?)null;

            var spanDays = days ?? (earliest.HasValue
                ? Math.Max(1, (int)(DateTime.Now - earliest.Value).TotalDays + 1)
                : 1);

            return Task.FromResult(new GlobalSessionStats
            {
                TotalSessions = sessions.Count,
                TotalMessages = messages,
                TotalTokens = tokens,
                Days = spanDays,
                AverageDailyTokens = spanDays > 0 ? tokens / (double)spanDays : 0
            });
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<SessionMessage>> GetActiveMessagesAsync(string sessionId)
    {
        lock (_gate)
        {
            if (!_sessions.TryGetValue(sessionId, out var record))
                return Task.FromResult<IEnumerable<SessionMessage>>(Array.Empty<SessionMessage>());

            var list = record.Messages
                .Where(m => !record.CompactedIds.Contains(m.Id))
                .OrderBy(m => m.Id)
                .Select(Clone)
                .ToList();
            return Task.FromResult<IEnumerable<SessionMessage>>(list);
        }
    }

    /// <inheritdoc />
    public Task MarkMessagesCompactedAsync(string sessionId, IEnumerable<long> messageIds)
    {
        lock (_gate)
        {
            if (_sessions.TryGetValue(sessionId, out var record))
            {
                foreach (var id in messageIds)
                    record.CompactedIds.Add(id);
            }
        }

        return Task.CompletedTask;
    }

    private static SessionMessage Clone(SessionMessage message) => new()
    {
        Id = message.Id,
        SessionId = message.SessionId,
        Role = message.Role,
        Content = message.Content,
        Thinking = message.Thinking,
        Attachments = message.Attachments,
        ToolCalls = message.ToolCalls,
        Tokens = message.Tokens,
        CreatedAt = message.CreatedAt
    };

    private sealed class SessionRecord(SessionInfo info, string? workspaceId)
    {
        public SessionInfo Info { get; } = info;

        public string? WorkspaceId { get; } = workspaceId;

        public List<SessionMessage> Messages { get; } = new();

        public HashSet<long> CompactedIds { get; } = new();
    }
}