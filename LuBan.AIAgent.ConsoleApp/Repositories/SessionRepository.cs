/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.ConsoleApp.Repositories
*文件名： SessionRepository
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：Session 仓储
*
*****************************************************************************/
using LuBan.AIAgent.ConsoleApp.Entities;
using LuBan.Orm;

using SqlSugar;

namespace LuBan.AIAgent.ConsoleApp.Repositories;

/// <summary>
/// Session 仓储
/// </summary>
public class SessionRepository : BaseRepository<DbSession>
{
    public SessionRepository(long tenantId = LuBanOrmConst.DefaultTenantId)
        : base(tenantId)
    {
    }

    /// <summary>
    /// 根据会话ID获取会话
    /// </summary>
    public async Task<DbSession?> GetBySessionIdAsync(string sessionId)
    {
        return await GetFirstAsync(s => s.SessionId == sessionId && !s.IsDelete);
    }

    /// <summary>
    /// 获取用户的所有会话
    /// </summary>
    public async Task<List<DbSession>> GetUserSessionsAsync(string userId)
    {
        return await AsQueryable()
            .Where(s => s.UserId == userId && !s.IsDelete)
            .OrderByDescending(s => s.UpdateTime ?? s.CreateTime)
            .ToListAsync();
    }

    /// <summary>
    /// 更新会话标题
    /// </summary>
    public async Task UpdateTitleAsync(string sessionId, string title)
    {
        await UpdateAsync(s => new DbSession { Title = title }, s => s.SessionId == sessionId);
    }

    /// <summary>
    /// 软删除会话
    /// </summary>
    public async Task SoftDeleteAsync(string sessionId)
    {
        await LogicDeleteAsync(s => s.SessionId == sessionId);
    }

    /// <summary>
    /// 增加消息计数
    /// </summary>
    public async Task IncrementMessageCountAsync(string sessionId, int tokens = 0)
    {
        await Context.Updateable<DbSession>()
            .SetColumns(s => s.MessageCount == s.MessageCount + 1)
            .SetColumns(s => s.TotalTokens == s.TotalTokens + tokens)
            .SetColumns(s => s.UpdateTime == DateTime.Now)
            .Where(s => s.SessionId == sessionId)
            .ExecuteCommandAsync();
    }
}

/// <summary>
/// Session 消息仓储
/// </summary>
public class SessionMessageRepository : BaseRepository<DbSessionMessage>
{
    public SessionMessageRepository(long tenantId = LuBanOrmConst.DefaultTenantId)
        : base(tenantId)
    {
    }

    /// <summary>
    /// 获取会话消息
    /// </summary>
    public async Task<List<DbSessionMessage>> GetSessionMessagesAsync(string sessionId, int? limit = null)
    {
        var query = AsQueryable()
            .Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.CreateTime, OrderByType.Asc);

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// 清除会话消息
    /// </summary>
    public async Task ClearMessagesAsync(string sessionId)
    {
        await DeleteAsync(m => m.SessionId == sessionId);
    }

    /// <summary>
    /// 获取会话消息统计
    /// </summary>
    public async Task<(int total, int userMsgs, int assistantMsgs, int totalTokens)> GetStatsAsync(string sessionId)
    {
        var stats = await AsQueryable()
            .Where(m => m.SessionId == sessionId)
            .GroupBy(m => m.Role)
            .Select(m => new
            {
                Role = m.Role,
                Count = SqlFunc.AggregateCount(m.Id),
                Tokens = SqlFunc.AggregateSum(m.Tokens ?? 0)
            })
            .ToListAsync();

        int total = 0;
        int userMsgs = 0;
        int assistantMsgs = 0;
        int totalTokens = 0;

        foreach (var stat in stats)
        {
            total += stat.Count;
            totalTokens += stat.Tokens;

            if (stat.Role == "user")
                userMsgs = stat.Count;
            else if (stat.Role == "assistant")
                assistantMsgs = stat.Count;
        }

        return (total, userMsgs, assistantMsgs, totalTokens);
    }
}
