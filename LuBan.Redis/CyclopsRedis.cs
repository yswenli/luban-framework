/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Redis
*文件名： LuBanRedis
*版本号： V1.0.0.0
*唯一标识：890449ba-e6a7-47fe-9a77-7c1ae5b81ec8
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/8/24 13:37:49
*描述：集成的redis初始化
*
*=================================================
*修改标记
*修改时间：2023/8/24 13:37:49
*修改人： yswenli
*版本号： V1.0.0.0
*描述：集成的redis初始化
*
*****************************************************************************/
using LuBan.Redis.Interfaces;

namespace LuBan.Redis;

/// <summary>
/// 集成快捷的redis初始化，
/// 自定义的用 RedisClientBuilder.Build(redisOptions)
/// </summary>
public class LuBanRedis : BaseSingleInstance<LuBanRedis>
{
    RedisClient _redisClient;

    /// <summary>
    /// 集成的redis初始化
    /// </summary>
    [Obsolete("使用Instance属性替换")]
    public LuBanRedis()
    {
        var redisOptions = NacosConfigUtil.Read<RedisOptions>();
        if (redisOptions == null) throw new Exception("未找到redis配置");
        _redisClient = RedisClientBuilder.Build(redisOptions);
    }

    /// <summary>
    /// 集成的redis初始化
    /// </summary>
    /// <param name="redisOptionName"></param>
    /// <exception cref="Exception"></exception>
    public LuBanRedis(string redisOptionName = "RedisOptions")
    {
        var redisOptions = NacosConfigUtil.Read<RedisOptions>(redisOptionName);
        if (redisOptions == null) throw new Exception("未找到redis配置");
        _redisClient = RedisClientBuilder.Build(redisOptions);
    }

    /// <summary>
    /// 获取redis操作类
    /// </summary>
    /// <param name="dbIndex"></param>
    /// <returns></returns>
    public IDatabase GetDatabase(int dbIndex = -1)
    {
        return _redisClient.GetDatabase(dbIndex);
    }

    /// <summary>
    /// 获取所有符合pattern的key
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="dbIndex"></param>
    /// <returns></returns>
    public List<string> Keys(string pattern, int dbIndex = -1)
    {
        var server = _redisClient.GetRedisServer().First();
        var keys = server.Keys(dbIndex, pattern);
        if (keys == null || !keys.Any()) return [];
        List<string> result = [];
        foreach (var item in keys)
        {
            result.Add(item.ToString());
        }
        return result;
    }

    /// <summary>
    /// 获取或添加缓存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="fun"></param>
    /// <param name="timeout"></param>
    /// <param name="withEnv"></param>
    /// <returns></returns>
    public async Task<T?> GetFromCacheAsync<T>(string key, Func<Task<T>> fun, int timeout = 60 * 1000, bool withEnv = true)
        where T : class, new()
    {
        try
        {
            if (withEnv)
            {
                key = key.GetKeyByEnv();
            }
            if (await GetDatabase().StringSetIfNotExistsAsync("Redis:Cache:GetFromCacheAsync".GetKeyByEnv(), key, timeout))
            {
                var rv = await GetDatabase().StringGetAsync(key);
                if (rv.IsNotNullOrEmpty())
                {
                    var data = rv.GetT<T>();
                    if (data == default(T?))
                    {
                        var t = await fun.Invoke();
                        if (t != default(T?))
                        {
                            await GetDatabase().StringSetAsync(key, t.ToJson() ?? "", TimeSpan.FromMilliseconds(timeout));
                            return t;
                        }
                    }
                    return data;
                }
            }
            else
            {
                await Task.Delay(10);
                return await GetFromCacheAsync<T>(key, fun, timeout, withEnv);
            }
        }
        finally
        {
            await GetDatabase().KeyDeleteAsync("Redis:Cache:GetFromCacheAsync".GetKeyByEnv());
        }
        return default;
    }

    /// <summary>
    /// 获取或添加缓存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="t"></param>
    /// <param name="timeout"></param>
    /// <param name="withEnv"></param>
    /// <returns></returns>
    public async Task<T?> GetFromCacheAsync<T>(string key, T t, int timeout = 60 * 1000, bool withEnv = true) where T : class, new()
    {
        return await GetFromCacheAsync(key, async () => await Task.FromResult(t), timeout, withEnv);
    }


    /// <summary>
    /// 获取或添加缓存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="fun"></param>
    /// <param name="timeout"></param>
    /// <param name="withEnv"></param>
    /// <returns></returns>
    public T? GetFromCache<T>(string key, Func<T> fun, int timeout = 60 * 1000, bool withEnv = true) where T : class, new()
    {
        try
        {
            if (withEnv)
            {
                key = key.GetKeyByEnv();
            }
            if (GetDatabase().StringSetIfNotExists("Redis:Cache:GetFromCache".GetKeyByEnv(), key, timeout))
            {
                var rv = GetDatabase().StringGet(key);
                if (rv.IsNotNullOrEmpty())
                {
                    var data = rv.GetT<T>();
                    if (data == default(T?))
                    {
                        var t = fun.Invoke();
                        GetDatabase().StringSet(key, t.ToJson(), TimeSpan.FromMilliseconds(timeout));
                        return t;
                    }
                    return data;
                }
            }
            else
            {
                Thread.Sleep(10);
                return GetFromCache<T>(key, fun, timeout, withEnv);
            }
        }
        finally
        {
            GetDatabase().KeyDelete("Redis:Cache:GetFromCache".GetKeyByEnv());
        }
        return default;
    }

    /// <summary>
    /// 获取或添加缓存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="t"></param>
    /// <param name="timeout"></param>
    /// <param name="withEnv"></param>
    /// <returns></returns>
    public T? GetFromCache<T>(string key, T t, int timeout = 60 * 1000, bool withEnv = true) where T : class, new()
    {
        return GetFromCache(key, () => t, timeout, withEnv);
    }

    /// <summary>
    /// 获取或添加缓存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="fun"></param>
    /// <param name="timeout"></param>
    /// <param name="withEnv"></param>
    /// <returns></returns> 
    public string? GetFromCache(string key, Func<string> fun, int timeout = 60 * 1000, bool withEnv = true)
    {
        try
        {
            if (withEnv)
            {
                key = key.GetKeyByEnv();
            }
            if (GetDatabase().StringSetIfNotExists("Redis:Cache:GetFromCache".GetKeyByEnv(), key, timeout))
            {
                var rv = GetDatabase().StringGet(key);
                if (rv.IsNotNullOrEmpty())
                {
                    var t = fun.Invoke();
                    if (t.IsNotNullOrEmpty())
                        GetDatabase().StringSet(key, t, TimeSpan.FromMilliseconds(timeout));
                    return t;
                }
                return rv;
            }
            else
            {
                Thread.Sleep(10);
                return GetFromCache(key, fun, timeout, withEnv);
            }
        }
        finally
        {
            GetDatabase().KeyDelete("Redis:Cache:GetFromCache".GetKeyByEnv());
        }
    }

    /// <summary>
    /// 获取或添加缓存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="t"></param>
    /// <param name="timeout"></param>
    /// <param name="withEnv"></param>
    /// <returns></returns>
    public string? GetFromCache<T>(string key, string t, int timeout = 60 * 1000, bool withEnv = true)
    {
        return GetFromCache(key, () => t, timeout, withEnv);
    }

    /// <summary>
    /// 获取或添加缓存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="fun"></param>
    /// <param name="timeout"></param>
    /// <param name="withEnv"></param>
    /// <returns></returns>
    public async Task<string?> GetFromCacheAsync(string key, Func<Task<string>> fun, int timeout = 60 * 1000, bool withEnv = true)
    {
        try
        {
            if (withEnv)
            {
                key = key.GetKeyByEnv();
            }
            if (await GetDatabase().StringSetIfNotExistsAsync("Redis:Cache:GetFromCacheAsync".GetKeyByEnv(), key, timeout))
            {
                var rv = await GetDatabase().StringGetAsync(key);
                if (rv.IsNotNullOrEmpty())
                {
                    var t = await fun.Invoke();
                    if (t.IsNotNullOrEmpty())
                        await GetDatabase().StringSetAsync(key, t, TimeSpan.FromMilliseconds(timeout));
                    return t;
                }
                return rv;
            }
            else
            {
                await Task.Delay(10);
                return await GetFromCacheAsync(key, fun, timeout, withEnv);
            }
        }
        finally
        {
            GetDatabase().KeyDelete("Redis:Cache:GetFromCacheAsync".GetKeyByEnv());
        }
    }

    /// <summary>
    /// 获取或添加缓存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="t"></param>
    /// <param name="timeout"></param>
    /// <param name="withEnv"></param>
    /// <returns></returns>
    public async Task<string?> GetFromCacheAsync(string key, string t, int timeout = 60 * 1000, bool withEnv = true)
    {
        return await GetFromCacheAsync(key, () => Task.FromResult(t), timeout, withEnv);
    }


    /// <summary>
    /// 获取redis list队列
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="timeout"></param>
    /// <param name="dbIndex"></param>
    /// <returns></returns>
    public RedisQueue<T> GetRedisQueue<T>(string topic, int timeout = 7 * 24 * 60 * 60 * 1000, int dbIndex = 0) where T : class, new()
    {
        return _redisClient.GetRedisQueue<T>(topic, timeout, dbIndex);
    }

    #region Redis Stream

    /// <summary>
    /// 获取redis生产者
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="dbIndex"></param>
    /// <param name="maxLength"></param>
    /// <returns></returns>
    public RedisProducer GetRedisProducer(string topic, int dbIndex = 0, int maxLength = 100000)
    {
        return _redisClient.GetRedisProducer(topic, dbIndex, maxLength);
    }
    /// <summary>
    /// 获取redis消费者
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="dbIndex"></param>
    /// <param name="groupName"></param>
    /// <returns></returns>
    public RedisConsumer GetRedisConsumer(string topic, int dbIndex = 0, string? groupName = "")
    {
        return _redisClient.GetRedisConsumer(topic, dbIndex, groupName);
    }

    #endregion


    #region redis pub/sub 

    /// <summary>
    /// 获取redis发布者
    /// </summary>
    /// <param name="channel"></param>
    /// <returns></returns>
    public RedisPublisher GetPublisher(string channel)
    {
        return _redisClient.GetPublisher(channel);
    }

    /// <summary>
    /// 获取redis订阅者
    /// </summary>
    /// <param name="channel"></param>
    /// <returns></returns>
    public RedisSubscriber GetSubscriber(string channel)
    {
        return _redisClient.GetSubscriber(channel);
    }

    /// <summary>
    /// 获取redis订阅者
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="channel"></param>
    /// <returns></returns>
    public RedisSubscriber<T> GetSubscriber<T>(string channel) where T : class, new()
    {
        return _redisClient.GetSubscriber<T>(channel);
    }


    /// <summary>
    /// 获取Redis键过期监听器
    /// </summary>
    /// <param name="dbIndex"></param>
    /// <returns></returns>
    public RedisKeyExpireListener GetKeyExpireListener(int dbIndex = 0)
    {
        return _redisClient.GetKeyExpireListener(dbIndex);
    }

    #endregion


    /// <summary>
    /// 获取分布式锁
    /// </summary>
    /// <param name="key">锁键名</param>
    /// <param name="timeout">锁过期时长（毫秒），默认 60 秒</param>
    /// <param name="dbIndex">Redis 数据库索引</param>
    /// <param name="token">自定义 token，不传则自动生成 GUID</param>
    /// <param name="reentrant">是否启用可重入模式（默认 false，性能更优）</param>
    /// <returns>分布式锁接口实例</returns>
    public IDistributedLock GetDistributedLock(
        string key,
        int timeout = 60 * 1000,
        int dbIndex = 0,
        string token = "",
        bool reentrant = false)
    {
        return _redisClient.CreateDistributedLock(
            key,
            TimeSpan.FromMilliseconds(timeout),
            dbIndex,
            token,
            reentrant);
    }
}
