using LuBan.Web.Core.OnlineUser;

namespace WebApplication1.Services.ApiServices.OnlineUser;

/// <summary>
/// 在线用户存储工厂
/// </summary>
/// <remarks>
/// 根据配置选择使用数据库存储或Redis存储
/// </remarks>
[Injection(Pattern = EnumInjectionPatterns.Self)]
public class OnlineUserStoreFactory : ISingleton
{
    private readonly DbOnlineUserStore _dbStore;
    private readonly RedisOnlineUserStore _redisStore;

    /// <summary>
    /// 在线用户存储工厂
    /// </summary>
    public OnlineUserStoreFactory(
        DbOnlineUserStore dbStore,
        RedisOnlineUserStore redisStore)
    {
        _dbStore = dbStore;
        _redisStore = redisStore;

        OnlineUserStoreProvider.Register(GetStore());
    }

    /// <summary>
    /// 获取当前配置的在线用户存储实现
    /// </summary>
    public IOnlineUserStore GetStore()
    {
        return HostingOptions.Default.EnableRedisCache
            ? _redisStore
            : _dbStore;
    }
}
