/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Sessions
*文件名： ISessionManager
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：Session 管理接口（在库中定义，不包含具体实现）
*
*****************************************************************************/
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LuBan.AIAgent.Sessions;

/// <summary>
/// Session 管理接口 - 在 LuBan.AIAgent 中定义，在 ConsoleApp 中实现
/// </summary>
public interface ISessionManager
{
    /// <summary>
    /// 创建新会话
    /// </summary>
    /// <param name="userId">用户ID（可选）</param>
    /// <param name="title">会话标题</param>
    /// <returns>会话信息</returns>
    Task<SessionInfo> CreateSessionAsync(string? userId = null, string? title = null);

    /// <summary>
    /// 获取会话
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <returns>会话信息</returns>
    Task<SessionInfo?> GetSessionAsync(string sessionId);

    /// <summary>
    /// 获取用户的所有会话
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>会话列表</returns>
    Task<IEnumerable<SessionInfo>> GetUserSessionsAsync(string userId);

    /// <summary>
    /// 更新会话标题
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="title">新标题</param>
    Task UpdateSessionTitleAsync(string sessionId, string title);

    /// <summary>
    /// 删除会话
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    Task DeleteSessionAsync(string sessionId);

    /// <summary>
    /// 添加消息到会话
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="role">消息角色</param>
    /// <param name="content">消息内容</param>
    /// <param name="tokens">Token 数量（可选）</param>
    /// <returns>消息信息</returns>
    Task<SessionMessage> AddMessageAsync(string sessionId, string role, string content, int? tokens = null);

    /// <summary>
    /// 获取会话消息
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="limit">限制数量（可选）</param>
    /// <returns>消息列表</returns>
    Task<IEnumerable<SessionMessage>> GetMessagesAsync(string sessionId, int? limit = null);

    /// <summary>
    /// 清除会话消息
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    Task ClearMessagesAsync(string sessionId);

    /// <summary>
    /// 获取会话统计信息
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <returns>统计信息</returns>
    Task<SessionStats> GetSessionStatsAsync(string sessionId);

    /// <summary>
    /// 设置当前活动会话
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    Task SetCurrentSessionAsync(string sessionId);

    /// <summary>
    /// 当前活动会话
    /// </summary>
    SessionInfo? CurrentSession { get; }
}

/// <summary>
/// 会话信息
/// </summary>
public class SessionInfo
{
    /// <summary>
    /// 会话ID
    /// </summary>
    public string SessionId { get; set; } = "";

    /// <summary>
    /// 用户ID
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// 会话标题
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 消息数量
    /// </summary>
    public int MessageCount { get; set; }

    /// <summary>
    /// 总 Token 数
    /// </summary>
    public int TotalTokens { get; set; }
}

/// <summary>
/// 会话消息
/// </summary>
public class SessionMessage
{
    /// <summary>
    /// 消息ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 会话ID
    /// </summary>
    public string SessionId { get; set; } = "";

    /// <summary>
    /// 消息角色（user, assistant, system）
    /// </summary>
    public string Role { get; set; } = "";

    /// <summary>
    /// 消息内容
    /// </summary>
    public string Content { get; set; } = "";

    /// <summary>
    /// Token 数量
    /// </summary>
    public int? Tokens { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 会话统计信息
/// </summary>
public class SessionStats
{
    /// <summary>
    /// 总消息数
    /// </summary>
    public int TotalMessages { get; set; }

    /// <summary>
    /// 用户消息数
    /// </summary>
    public int UserMessages { get; set; }

    /// <summary>
    /// AI 消息数
    /// </summary>
    public int AssistantMessages { get; set; }

    /// <summary>
    /// 总 Token 数
    /// </summary>
    public int TotalTokens { get; set; }

    /// <summary>
    /// 平均消息长度
    /// </summary>
    public double AverageMessageLength { get; set; }
}