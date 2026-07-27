/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.ConsoleApp.Services
*文件名： SessionManager
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：Session 管理服务实现
*
*****************************************************************************/
using LuBan.AIAgent.ConsoleApp.Entities;
using LuBan.AIAgent.ConsoleApp.Repositories;
using LuBan.AIAgent.Sessions;

namespace LuBan.AIAgent.ConsoleApp.Services;

/// <summary>
/// Session 管理服务实现 - 使用 SQLite 数据库
/// </summary>
public class SessionManager : ISessionManager
{
    private readonly SessionRepository _sessionRepo;
    private readonly SessionMessageRepository _messageRepo;

    /// <summary>
    /// 当前活动会话
    /// </summary>
    private SessionInfo? _currentSession;

    /// <summary>
    /// 创建 SessionManager 实例
    /// </summary>
    public SessionManager()
    {
        _sessionRepo = new SessionRepository();
        _messageRepo = new SessionMessageRepository();
    }

    /// <summary>
    /// 当前活动会话
    /// </summary>
    public SessionInfo? CurrentSession => _currentSession;

    /// <summary>
    /// 创建新会话
    /// </summary>
    public async Task<SessionInfo> CreateSessionAsync(string? userId = null, string? title = null)
    {
        var sessionId = Guid.NewGuid().ToString("N");
        var session = new DbSession
        {
            SessionId = sessionId,
            UserId = userId,
            Title = title ?? "新对话",
            CreateTime = DateTime.Now,
            IsDelete = false
        };

        await _sessionRepo.InsertAsync(session);

        _currentSession = ToSessionInfo(session);
        return _currentSession;
    }

    /// <summary>
    /// 获取会话
    /// </summary>
    public async Task<SessionInfo?> GetSessionAsync(string sessionId)
    {
        var session = await _sessionRepo.GetBySessionIdAsync(sessionId);
        if (session == null)
            return null;

        return ToSessionInfo(session);
    }

    /// <summary>
    /// 获取用户的所有会话
    /// </summary>
    public async Task<IEnumerable<SessionInfo>> GetUserSessionsAsync(string userId)
    {
        var sessions = await _sessionRepo.GetUserSessionsAsync(userId);
        return sessions.Select(ToSessionInfo);
    }

    /// <summary>
    /// 更新会话标题
    /// </summary>
    public async Task UpdateSessionTitleAsync(string sessionId, string title)
    {
        await _sessionRepo.UpdateTitleAsync(sessionId, title);

        if (_currentSession?.SessionId == sessionId)
        {
            _currentSession.Title = title;
        }
    }

    /// <summary>
    /// 删除会话
    /// </summary>
    public async Task DeleteSessionAsync(string sessionId)
    {
        await _sessionRepo.SoftDeleteAsync(sessionId);
        await _messageRepo.ClearMessagesAsync(sessionId);

        if (_currentSession?.SessionId == sessionId)
        {
            _currentSession = null;
        }
    }

    /// <summary>
    /// 添加消息到会话
    /// </summary>
    public async Task<SessionMessage> AddMessageAsync(string sessionId, string role, string content, int? tokens = null)
    {
        var message = new DbSessionMessage
        {
            SessionId = sessionId,
            Role = role,
            Content = content,
            Tokens = tokens,
            CreateTime = DateTime.Now,
            IsDelete = false
        };

        var id = await _messageRepo.InsertReturnIdentityAsync(message);
        message.Id = id;

        await _sessionRepo.IncrementMessageCountAsync(sessionId, tokens ?? 0);

        if (_currentSession?.SessionId == sessionId)
        {
            _currentSession.MessageCount++;
            _currentSession.TotalTokens += tokens ?? 0;
        }

        return ToSessionMessage(message);
    }

    /// <summary>
    /// 获取会话消息
    /// </summary>
    public async Task<IEnumerable<SessionMessage>> GetMessagesAsync(string sessionId, int? limit = null)
    {
        var messages = await _messageRepo.GetSessionMessagesAsync(sessionId, limit);
        return messages.Select(ToSessionMessage);
    }

    /// <summary>
    /// 清除会话消息
    /// </summary>
    public async Task ClearMessagesAsync(string sessionId)
    {
        await _messageRepo.ClearMessagesAsync(sessionId);

        if (_currentSession?.SessionId == sessionId)
        {
            _currentSession.MessageCount = 0;
            _currentSession.TotalTokens = 0;
        }
    }

    /// <summary>
    /// 获取会话统计信息
    /// </summary>
    public async Task<SessionStats> GetSessionStatsAsync(string sessionId)
    {
        var (total, userMsgs, assistantMsgs, totalTokens) = await _messageRepo.GetStatsAsync(sessionId);

        return new SessionStats
        {
            TotalMessages = total,
            UserMessages = userMsgs,
            AssistantMessages = assistantMsgs,
            TotalTokens = totalTokens,
            AverageMessageLength = total > 0 ? totalTokens / (double)total : 0
        };
    }

    /// <summary>
    /// 设置当前活动会话
    /// </summary>
    public async Task SetCurrentSessionAsync(string sessionId)
    {
        _currentSession = await GetSessionAsync(sessionId);
    }

    /// <summary>
    /// 转换为 SessionInfo
    /// </summary>
    private static SessionInfo ToSessionInfo(DbSession session)
    {
        return new SessionInfo
        {
            SessionId = session.SessionId,
            UserId = session.UserId,
            Title = session.Title,
            CreatedAt = session.CreateTime,
            UpdatedAt = session.UpdateTime,
            MessageCount = session.MessageCount,
            TotalTokens = session.TotalTokens
        };
    }

    /// <summary>
    /// 转换为 SessionMessage
    /// </summary>
    private static SessionMessage ToSessionMessage(DbSessionMessage message)
    {
        return new SessionMessage
        {
            Id = message.Id,
            SessionId = message.SessionId,
            Role = message.Role,
            Content = message.Content,
            Tokens = message.Tokens,
            CreatedAt = message.CreateTime
        };
    }
}