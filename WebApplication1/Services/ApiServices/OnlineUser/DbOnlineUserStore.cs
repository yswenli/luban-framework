using Yitter.IdGenerator;

namespace WebApplication1.Services.ApiServices.OnlineUser;

/// <summary>
/// 数据库在线用户存储
/// </summary>
[Injection(Pattern = EnumInjectionPatterns.SelfWithFirstInterface)]
public class DbOnlineUserStore : IOnlineUserStore, ISingleton
{
    private readonly int _expireSeconds;

    /// <summary>
    /// 数据库在线用户存储
    /// </summary>
    public DbOnlineUserStore()
    {
        _expireSeconds = HostingOptions.Default.AppOptions.JwtAuthConfig?.AccessExpiration ?? 7200;
    }

    private static BaseRepository<DbOnlineUser> GetRepository(long tenantId)
    {
        return new BaseRepository<DbOnlineUser>(tenantId);
    }

    /// <summary>
    /// 写入或覆盖会话
    /// </summary>
    public async Task WriteAsync(OnlineUserSession session)
    {
        var rep = GetRepository(session.TenantId);
        var now = DateTime.UtcNow;

        // 使用 ClearFilter 查询，避免软删除记录被过滤导致唯一索引冲突
        var onlineUser = await rep.AsQueryable().ClearFilter()
            .Where(q => q.UserId == session.UserId && q.TenantId == session.TenantId)
            .FirstAsync();

        if (onlineUser == null)
        {
            var entity = new DbOnlineUser
            {
                Id = YitIdHelper.NextId(),
                UserId = session.UserId,
                TenantId = session.TenantId,
                UserName = session.UserName,
                LoginTime = session.LoginTime,
                LastActiveTime = now,
                Ip = session.Ip,
                Device = session.Device
            };
            await rep.InsertAsync(entity);
        }
        else
        {
            await rep.AsUpdateable()
                .SetColumns(q => new DbOnlineUser
                {
                    UserName = session.UserName,
                    LoginTime = session.LoginTime,
                    LastActiveTime = now,
                    Ip = session.Ip,
                    Device = session.Device,
                    IsDelete = false
                })
                .Where(q => q.UserId == session.UserId && q.TenantId == session.TenantId)
                .ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 读取会话
    /// </summary>
    public async Task<OnlineUserSession?> ReadAsync(long tenantId, long userId)
    {
        var rep = GetRepository(tenantId);
        var entity = await rep.Where(q => q.IsDelete == false && q.UserId == userId).FirstAsync();
        if (entity == null) return null;
        if (entity.LastActiveTime.AddSeconds(_expireSeconds) < DateTime.UtcNow) return null;
        return ToSession(entity);
    }

    /// <summary>
    /// 刷新最后活跃时间
    /// </summary>
    public async Task RefreshAsync(long tenantId, long userId)
    {
        var rep = GetRepository(tenantId);
        await rep.AsUpdateable()
            .SetColumns(q => new DbOnlineUser { LastActiveTime = DateTime.UtcNow })
            .Where(q => q.IsDelete == false && q.UserId == userId)
            .ExecuteCommandAsync();
    }

    /// <summary>
    /// 移除会话
    /// </summary>
    public async Task RemoveAsync(long tenantId, long userId)
    {
        var rep = GetRepository(tenantId);
        await rep.DeleteAsync(q => q.UserId == userId);
    }

    /// <summary>
    /// 分页查询
    /// </summary>
    public async Task<PagedList<OnlineUserSession>> PageAsync(int page, int pageSize, long? tenantId, string? userName = null)
    {
        var sessions = new List<OnlineUserSession>();
        var expireTime = DateTime.UtcNow.AddSeconds(-_expireSeconds);

        if (tenantId.HasValue)
        {
            var rep = GetRepository(tenantId.Value);
            var query = rep.AsQueryable().Where(q => q.IsDelete == false && q.LastActiveTime >= expireTime);
            if (!string.IsNullOrWhiteSpace(userName))
            {
                query = query.Where(q => q.UserName != null && q.UserName.Contains(userName));
            }
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(q => q.LastActiveTime).ToPageListAsync(page, pageSize);
            return items.Select(ToSession).ToPagedList(total, page, pageSize);
        }

        var configs = LuBanOrm.DbConnectionOptions?.ConnectionConfigs;
        if (configs != null)
        {
            foreach (var config in configs)
            {
                if (config.ConfigId?.ToString() == LuBanOrmConst.LogConfigId)
                {
                    continue;
                }

                if (!long.TryParse(config.ConfigId?.ToString(), out var configTenantId))
                {
                    continue;
                }

                try
                {
                    var rep = GetRepository(configTenantId);
                    var query = rep.AsQueryable().Where(q => q.IsDelete == false && q.LastActiveTime >= expireTime);
                    if (!string.IsNullOrWhiteSpace(userName))
                    {
                        query = query.Where(q => q.UserName != null && q.UserName.Contains(userName));
                    }
                    var items = await query.ToListAsync();
                    sessions.AddRange(items.Select(ToSession));
                }
                catch (Exception ex)
                {
                    Logger.Error($"查询租户在线用户失败：{config.ConfigId}", ex);
                }
            }
        }

        var ordered = sessions.OrderByDescending(s => s.LastActiveTime).ToList();
        var totalAll = ordered.Count;
        var pageItems = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return pageItems.ToPagedList(totalAll, page, pageSize);
    }

    private static OnlineUserSession ToSession(DbOnlineUser entity)
    {
        return new OnlineUserSession
        {
            UserId = entity.UserId,
            TenantId = entity.TenantId ?? 0,
            UserName = entity.UserName ?? "",
            LoginTime = entity.LoginTime,
            LastActiveTime = entity.LastActiveTime,
            Ip = entity.Ip ?? "",
            Device = entity.Device ?? ""
        };
    }
}
