/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Sessions
*文件名： ISessionManager
*版本号： V1.0.0.0
*唯一标识：16dd929d-87db-4a59-ab7e-1ee9d434caad
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：Session 管理接口（在库中定义，不包含具体实现）
*
*=================================================
*修改标记
*修改时间：2026/7/27
*修改人： yswenli
*版本号： V1.0.0.0
*描述：Session 管理接口（在库中定义，不包含具体实现）
*
*****************************************************************************/

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
    /// 获取会话最近 N 条消息。
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="count">消息数量。</param>
    /// <returns>消息列表（时间正序）。</returns>
    Task<IEnumerable<SessionMessage>> GetLatestMessagesAsync(string sessionId, int count);

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
    /// 清除当前会话（切换到无会话状态）
    /// </summary>
    void ClearCurrentSession();

    /// <summary>
    /// 当前活动会话
    /// </summary>
    SessionInfo? CurrentSession { get; }

    /// <summary>
    /// 物理删除全部会话及消息数据
    /// </summary>
    Task ClearAllSessionsAsync();

    /// <summary>
    /// 获取全局会话统计
    /// </summary>
    /// <param name="days">限定最近 N 天（按会话创建时间），null 统计全部</param>
    Task<GlobalSessionStats> GetGlobalStatsAsync(int? days = null);

    /// <summary>
    /// 获取会话的活跃消息（未被压缩的，含 role=summary 摘要消息）
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    Task<IEnumerable<SessionMessage>> GetActiveMessagesAsync(string sessionId);

    /// <summary>
    /// 将指定消息标记为已压缩（保留数据，不再进入模型上下文）
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="messageIds">消息ID列表</param>
    Task MarkMessagesCompactedAsync(string sessionId, IEnumerable<long> messageIds);
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

/// <summary>
/// 全局会话统计
/// </summary>
public class GlobalSessionStats
{
    /// <summary>
    /// 总会话数
    /// </summary>
    public int TotalSessions { get; set; }

    /// <summary>
    /// 总消息数（不含摘要消息）
    /// </summary>
    public int TotalMessages { get; set; }

    /// <summary>
    /// 总 Token 数（含摘要消息 token）
    /// </summary>
    public long TotalTokens { get; set; }

    /// <summary>
    /// 统计覆盖天数
    /// </summary>
    public int Days { get; set; }

    /// <summary>
    /// 日均 Token
    /// </summary>
    public double AverageDailyTokens { get; set; }
}
