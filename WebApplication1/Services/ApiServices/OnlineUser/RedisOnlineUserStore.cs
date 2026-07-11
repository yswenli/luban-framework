using LuBan.Redis;
using LuBan.Web.Core.OnlineUser;

namespace WebApplication1.Services.ApiServices.OnlineUser;

/// <summary>
/// Redis 在线用户存储
/// </summary>
[Injection(Pattern = EnumInjectionPatterns.SelfWithFirstInterface)]
public class RedisOnlineUserStore : IOnlineUserStore, ISingleton
{
    private readonly int _expireSeconds;

    /// <summary>
    /// Redis 在线用户存储
    /// </summary>
    public RedisOnlineUserStore()
    {
        _expireSeconds = HostingOptions.Default.AppOptions.JwtAuthConfig?.AccessExpiration ?? 7200;
    }

    private static string BuildKey(long tenantId, long userId)
    {
        return $"online:{tenantId}:{userId}".GetKeyByEnv();
    }

    /// <summary>
    /// 写入或覆盖会话
    /// </summary>
    public async Task WriteAsync(OnlineUserSession session)
    {
        var key = BuildKey(session.TenantId, session.UserId);
        await LuBanRedis.Instance.GetDatabase().StringSetAsync(
            key,
            session.ToJson() ?? "",
            TimeSpan.FromSeconds(_expireSeconds));
    }

    /// <summary>
    /// 读取会话
    /// </summary>
    public async Task<OnlineUserSession?> ReadAsync(long tenantId, long userId)
    {
        var key = BuildKey(tenantId, userId);
        var value = await LuBanRedis.Instance.GetDatabase().StringGetAsync(key);
        if (value.IsNullOrEmpty()) return null;
        return value.GetT<OnlineUserSession>();
    }

    /// <summary>
    /// 刷新最后活跃时间
    /// </summary>
    public async Task RefreshAsync(long tenantId, long userId)
    {
        var key = BuildKey(tenantId, userId);
        var db = LuBanRedis.Instance.GetDatabase();
        var value = await db.StringGetAsync(key);
        if (value.IsNullOrEmpty()) return;

        var session = value.GetT<OnlineUserSession>();
        if (session == null) return;

        session.LastActiveTime = DateTime.UtcNow;
        await db.StringSetAsync(key, session.ToJson() ?? "", TimeSpan.FromSeconds(_expireSeconds));
    }

    /// <summary>
    /// 移除会话
    /// </summary>
    public async Task RemoveAsync(long tenantId, long userId)
    {
        var key = BuildKey(tenantId, userId);
        await LuBanRedis.Instance.GetDatabase().KeyDeleteAsync(key);
    }

    /// <summary>
    /// 分页查询
    /// </summary>
    public async Task<PagedList<OnlineUserSession>> PageAsync(int page, int pageSize, long? tenantId, string? userName = null)
    {
        var pattern = $"online:*".GetKeyByEnv();
        var keys = LuBanRedis.Instance.Keys(pattern);

        var sessions = new List<OnlineUserSession>();
        foreach (var fullKey in keys)
        {
            var value = await LuBanRedis.Instance.GetDatabase().StringGetAsync(fullKey);
            if (value.IsNullOrEmpty()) continue;
            var session = value.GetT<OnlineUserSession>();
            if (session == null) continue;
            if (tenantId.HasValue && session.TenantId != tenantId.Value) continue;
            if (!string.IsNullOrWhiteSpace(userName) && (session.UserName?.Contains(userName) != true)) continue;
            sessions.Add(session);
        }

        var ordered = sessions.OrderByDescending(s => s.LastActiveTime).ToList();
        var total = ordered.Count;
        var items = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return items.ToPagedList(total, page, pageSize);
    }
}
