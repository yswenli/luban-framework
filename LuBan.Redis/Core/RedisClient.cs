/****************************************************************************
*Copyright @ 2023-2024 riverland All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：riverland
*命名空间：LuBan.Redis
*文件名： RedisClient
*版本号： V1.0.0.0
*唯一标识：97f49c78-9aab-4af2-b6a4-5c17f4a013ac
*当前的用户域：riverland
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/14 15:20:34
*描述：
*
*=====================================================================
*修改标记
*修改时间：2022/7/14 15:20:34
*修改人： walle.wen
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
using LuBan.Redis.Interfaces;

namespace LuBan.Redis.Core;

/// <summary>
/// redis容器类
/// 此类不要直接new(),需要用RedisHelperBuilder来构造
/// </summary>
public class RedisClient
{
    private RedisOptions _redisConfig;
    private ConnectionMultiplexer _cnn;

    /// <summary>
    /// redis容器类
    /// </summary>
    internal RedisClient(RedisOptions redisOptions)
    {
        _redisConfig = redisOptions ?? throw new Exception("传入的redisConfig实例不能空");
        var configStr = GenerateConnectionString(_redisConfig);
        var options = ConfigurationOptions.Parse(configStr);
        options.ReconnectRetryPolicy = new ExponentialRetry(_redisConfig.BusyRetryWaitMS, _redisConfig.BusyRetryWaitMS * _redisConfig.BusyRetry);
        _cnn = ConnectionMultiplexer.Connect(options);
    }


    /// <summary>
    /// 根据redis使用类型来生成相应的连接字符串
    /// </summary>
    /// <param name="redisConfig"></param>
    /// <returns></returns>
    private static string GenerateConnectionString(RedisOptions redisConfig)
    {
        var configStr = string.Empty;

        switch (redisConfig.Type)
        {
            case EnumRedisType.Instance:
                if (!string.IsNullOrWhiteSpace(redisConfig.Slaves))
                    configStr = string.Format("{0},{1},defaultDatabase={2}", redisConfig.Masters, redisConfig.Slaves, redisConfig.DefaultDatabase);
                else
                    configStr = string.Format("{0},defaultDatabase={1}", redisConfig.Masters, redisConfig.DefaultDatabase);
                if (!string.IsNullOrWhiteSpace(redisConfig.Password))
                    configStr += ",password=" + redisConfig.Password;
                break;
            case EnumRedisType.Sentinel:
                //哨兵
                configStr = string.Format("{0},defaultDatabase={1},serviceName={2}", redisConfig.Masters, redisConfig.DefaultDatabase, redisConfig.ServiceName);
                break;
            case EnumRedisType.Cluster:
                //集群
                configStr = string.Format("{0}", redisConfig.Masters);
                if (!string.IsNullOrWhiteSpace(redisConfig.Password))
                    configStr += ",password=" + redisConfig.Password;
                break;
            default:
                if (!string.IsNullOrWhiteSpace(redisConfig.Slaves))
                    configStr = redisConfig.Masters + "," + redisConfig.Slaves;
                else
                    configStr = redisConfig.Masters;

                if (!string.IsNullOrWhiteSpace(redisConfig.Password))
                    configStr += ",password=" + redisConfig.Password;
                break;
        }
        configStr +=
            string.Format(",allowAdmin={0},connectRetry={1},connectTimeout={2},keepAlive={3},syncTimeout={4},responseTimeout={4},abortConnect=true", redisConfig.AllowAdmin, redisConfig.ConnectRetry, redisConfig.ConnectTimeout, redisConfig.KeepAlive, redisConfig.CommandTimeout);

        if (!string.IsNullOrWhiteSpace(redisConfig.Extention))
        {
            configStr += "," + redisConfig.Extention;
        }

        return configStr;
    }

    /// <summary>
    /// 获取redis服务器集合
    /// </summary>
    /// <returns></returns>
    public IServer[] GetRedisServer()
    {
        return _cnn.GetServers();
    }

    /// <summary>
    /// 获取redis数据库
    /// </summary>
    /// <param name="dbIndex"></param>
    /// <param name="asyncState">异步状态，不为空且dbIndex<16时，创建一个新的redis数据库实例</param>
    /// <returns></returns>
    public IDatabase GetDatabase(int dbIndex = -1, object? asyncState = null)
    {
        if (dbIndex < 1)
        {
            dbIndex = 0;
        }
        return _cnn.GetDatabase(dbIndex, asyncState);
    }

    /// <summary>
    /// 获取redis订阅者
    /// </summary>
    /// <param name="asyncState"></param>
    /// <returns></returns>
    public ISubscriber GetSubscriber(object? asyncState = null)
    {
        return _cnn.GetSubscriber(asyncState);
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
        return MemoryCache.Instance.GetOrSet($"{CacheConst.KeySystem}redis:queue:{topic}", (k) =>
        {
            return new RedisQueue<T>(this, topic, timeout, dbIndex);
        })!;
    }

    #region redis stream

    /// <summary>
    /// 获取redis生产者
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="dbIndex"></param>
    /// <param name="maxLength"></param>
    /// <returns></returns>
    public RedisProducer GetRedisProducer(string topic, int dbIndex = 0, int maxLength = 100000)
    {
        return MemoryCache.Instance.GetOrSet($"{CacheConst.KeySystem}redis:producer:{topic}", (k) =>
        {
            return new RedisProducer(this, topic, dbIndex, maxLength);
        })!;
    }
    /// <summary>
    /// 获取redis消费者
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="dbIndex"></param>
    /// <param name="groupName"></param>
    /// <returns></returns>
    public RedisConsumer GetRedisConsumer(string topic, int dbIndex, string? groupName = "")
    {
        return MemoryCache.Instance.GetOrSet($"{CacheConst.KeySystem}redis:consumer:{topic}_{groupName}", (k) =>
        {
            return new RedisConsumer(this, topic, dbIndex, groupName);
        })!;
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
        return MemoryCache.Instance.GetOrSet($"{CacheConst.KeySystem}redis:publisher:{channel}", (k) =>
        {
            return new RedisPublisher(this, channel);
        })!;
    }

    /// <summary>
    /// 获取redis订阅者
    /// </summary>
    /// <param name="channel"></param>
    /// <returns></returns>
    public RedisSubscriber GetSubscriber(string channel)
    {
        return MemoryCache.Instance.GetOrSet($"{CacheConst.KeySystem}redis:subscriber:{channel}", (k) =>
        {
            return new RedisSubscriber(this, channel);
        })!;
    }

    /// <summary>
    /// 获取redis订阅者
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="channel"></param>
    /// <returns></returns>
    public RedisSubscriber<T> GetSubscriber<T>(string channel) where T : class, new()
    {
        return MemoryCache.Instance.GetOrSet($"{CacheConst.KeySystem}redis:subscriber:{channel}", (k) =>
        {
            return new RedisSubscriber<T>(this, channel);
        })!;
    }

    /// <summary>
    /// 获取redis键过期监听器
    /// </summary>
    /// <param name="dbIndex"></param>
    /// <returns></returns>
    public RedisKeyExpireListener GetKeyExpireListener(int dbIndex = 0)
    {
        return MemoryCache.Instance.GetOrSet($"{CacheConst.KeySystem}redis:key_expire_listener:{dbIndex}", (k) =>
        {
            return new RedisKeyExpireListener(this, dbIndex);
        })!;
    }

    #endregion

    /// <summary>
    /// 创建分布式锁实例
    /// </summary>
    /// <param name="lockKey">锁键，建议加环境前缀</param>
    /// <param name="expiry">过期时长</param>
    /// <param name="dbIndex">数据库索引</param>
    /// <param name="token">可选自定义 token，不传则自动生成</param>
    /// <param name="reentrant">是否启用可重入模式（默认 false，性能更优）</param>
    /// <returns>分布式锁接口实例</returns>
    public IDistributedLock CreateDistributedLock(
        string lockKey,
        TimeSpan expiry,
        int dbIndex = 0,
        string? token = null,
        bool reentrant = false)
    {
        var db = GetDatabase(dbIndex);
        var value = token.IsNullOrEmpty() ? GuidUtil.New : token!;

        // 根据是否需要可重入选择不同的实现
        if (reentrant)
        {
            return new RedisDistributedLock(db, lockKey, value, expiry);
        }
        else
        {
            return new RedisDistributedLockV3(db, lockKey, value, expiry, false);
        }
    }
}
