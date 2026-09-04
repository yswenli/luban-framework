/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：WebApplication1.Services.ApiServices.OnlineUser
*文件名： OnlineUserStoreFactory.cs
*版本号： V1.0.0.0
*唯一标识：c85102c2-1ea8-4a4b-9993-0a99942df62e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:30
*描述：OnlineUserStoreFactory 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：OnlineUserStoreFactory 类
*
*****************************************************************************/

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
