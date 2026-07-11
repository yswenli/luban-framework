/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Redis
*文件名： RedisCache
*版本号： V1.0.0.0
*唯一标识：6f485a6c-c5c5-4775-a58d-fc0cd965f51c
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/11/20 14:56:02
*描述：RedisCache
*
*=================================================
*修改标记
*修改时间：2025/11/20 14:56:02
*修改人： yswenli
*版本号： V1.0.0.0
*描述：RedisCache
*
*****************************************************************************/
namespace LuBan.Redis;

/// <summary>
/// Redis缓存项，包含数据和过期时间信息
/// </summary>
public class RedisCacheItem<T>
{
    /// <summary>
    /// 缓存数据
    /// </summary>
    public T? Value { get; set; }

    /// <summary>
    /// 过期时间戳（Unix毫秒时间戳，-1表示永不过期）
    /// </summary>
    public long ExpireTimestamp { get; set; }

    /// <summary>
    /// 检查是否过期
    /// </summary>
    /// <returns>是否已过期</returns>
    public bool IsExpired()
    {
        // 永不过期
        if (ExpireTimestamp == -1)
        {
            return false;
        }

        // 检查当前时间是否超过过期时间
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() > ExpireTimestamp;
    }
}

/// <summary>
/// RedisCache
/// </summary>
public class RedisCache : IServiceCache
{
    static Lazy<RedisCache> _instance = new Lazy<RedisCache>(() => new RedisCache());
    LuBanRedis _LuBanRedis;
    const string _redisKey = "LuBan.RedisCache";

    /// <summary>
    /// 缓存
    /// </summary>
    /// <param name="redisOptionName"></param>
    public RedisCache(string redisOptionName = "RedisOptions")
    {
        _LuBanRedis = new LuBanRedis(redisOptionName);
    }

    /// <summary>
    /// 实例
    /// </summary>
    public static RedisCache Instance
    {
        get
        {
            return _instance.Value;
        }
    }

    /// <summary>
    /// 索引器，根据键获取缓存值
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <returns>缓存值</returns>
    public dynamic? this[string key]
    {
        get
        {
            return Get<dynamic>(key);
        }
    }

    /// <summary>
    /// 获取所有缓存键
    /// </summary>
    public string[] Keys
    {
        get
        {
            return _LuBanRedis.GetDatabase().HashKeys(_redisKey).Select(x => x.ToString()).ToArray();
        }
    }

    /// <summary>
    /// 检查缓存键是否存在
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <returns>是否存在</returns>
    public bool ContainsKey(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return false;
        }
        return _LuBanRedis.GetDatabase().HashExists(_redisKey, key);
    }

    /// <summary>
    /// 删除指定键的缓存
    /// </summary>
    /// <param name="key">缓存键</param>
    public void Delete(string key)
    {
        if (!string.IsNullOrEmpty(key))
        {
            _LuBanRedis.GetDatabase().HashDelete(_redisKey, key);
        }
    }

    /// <summary>
    /// 获取指定键的缓存值
    /// </summary>
    /// <typeparam name="T">缓存值类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <returns>缓存值</returns>
    public T? Get<T>(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return default;
        }

        try
        {
            var db = _LuBanRedis.GetDatabase();
            var value = db.HashGet(_redisKey, key);

            if (value.IsNull)
            {
                return default;
            }
            var cacheItem = SerializeUtil.Deserialize<RedisCacheItem<T>>(value.ToString());
            if (cacheItem == null)
            {
                return default;
            }
            if (cacheItem.IsExpired())
            {
                Delete(key);
                return default;
            }

            return cacheItem.Value;
        }
        catch (Exception ex)
        {
            Logger.Error("Redis获取缓存失败", ex);
            return default;
        }
    }

    /// <summary>
    /// 获取或设置缓存值
    /// </summary>
    /// <typeparam name="T">缓存值类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="addFun">当缓存不存在时，用于生成缓存值的函数</param>
    /// <returns>缓存值</returns>
    public T? GetOrSet<T>(string key, Func<string, T?> addFun)
    {
        return GetOrSet<T>(key, addFun, TimeSpan.FromMilliseconds(-1));
    }

    /// <summary>
    /// 获取或设置缓存值（带过期时间）
    /// </summary>
    /// <typeparam name="T">缓存值类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="addFun">当缓存不存在时，用于生成缓存值的函数</param>
    /// <param name="timeOut">过期时间</param>
    /// <returns>缓存值</returns>
    public T? GetOrSet<T>(string key, Func<string, T?> addFun, TimeSpan timeOut)
    {
        if (string.IsNullOrEmpty(key) || addFun == null)
        {
            return default;
        }

        // 先尝试从缓存获取
        var value = Get<T>(key);
        if (value != null)
        {
            return value;
        }

        // 缓存不存在，执行添加函数
        value = addFun(key);
        if (value != null)
        {
            // 设置缓存
            Set(key, value, timeOut);
        }

        return value;
    }

    /// <summary>
    /// 异步获取或设置缓存值（带过期时间）
    /// </summary>
    /// <typeparam name="T">缓存值类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="addFun">当缓存不存在时，用于生成缓存值的异步函数</param>
    /// <param name="timeOut">过期时间</param>
    /// <returns>缓存值</returns>
    public async Task<T?> GetOrSet<T>(string key, Func<string, Task<T?>> addFun, TimeSpan timeOut)
    {
        if (string.IsNullOrEmpty(key) || addFun == null)
        {
            return default;
        }

        // 先尝试从缓存获取
        var value = Get<T>(key);
        if (value != null)
        {
            return value;
        }

        // 缓存不存在，执行添加函数
        value = await addFun(key);
        if (value != null)
        {
            // 设置缓存
            Set(key, value, timeOut);
        }

        return value;
    }

    /// <summary>
    /// 设置缓存值（永不过期）
    /// </summary>
    /// <typeparam name="T">缓存值类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="value">缓存值</param>
    public void Set<T>(string key, T value)
    {
        Set(key, value, TimeSpan.FromMilliseconds(-1));
    }

    /// <summary>
    /// 设置缓存值（带过期时间）
    /// </summary>
    /// <typeparam name="T">缓存值类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="value">缓存值</param>
    /// <param name="timeOut">过期时间</param>
    public void Set<T>(string key, T value, TimeSpan timeOut)
    {
        if (string.IsNullOrEmpty(key) || value == null)
        {
            return;
        }

        try
        {
            var db = _LuBanRedis.GetDatabase();

            // 计算过期时间戳
            long expireTimestamp = -1; // 默认为永不过期
            if (timeOut.TotalMilliseconds > 0)
            {
                expireTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + (long)timeOut.TotalMilliseconds;
            }

            // 创建缓存项
            var cacheItem = new RedisCacheItem<T>
            {
                Value = value,
                ExpireTimestamp = expireTimestamp
            };

            // 序列化缓存项
            string jsonValue = SerializeUtil.Serialize(cacheItem);

            // 设置缓存
            db.HashSet(_redisKey, key, jsonValue);

            // 为整个Hash键设置较长的过期时间，避免内存泄漏（实际过期逻辑在RedisCacheItem中处理）
            db.KeyExpire(_redisKey, TimeSpan.FromDays(30));
        }
        catch (Exception ex)
        {
            Logger.Error("Redis设置缓存失败", ex);
        }
    }

    /// <summary>
    /// 清除所有缓存
    /// </summary>
    public void Clear()
    {
        _LuBanRedis.GetDatabase().KeyDelete(_redisKey);
    }

    /// <summary>
    /// 根据键前缀删除缓存
    /// </summary>
    /// <param name="prefix">键前缀</param>
    /// <returns>删除的缓存数量</returns>
    public int RemoveByPrefix(string prefix)
    {
        if (string.IsNullOrEmpty(prefix))
        {
            return 0;
        }

        var db = _LuBanRedis.GetDatabase();
        var fields = db.HashKeys(_redisKey).Select(x => x.ToString());
        var matchingFields = fields.Where(f => f.StartsWith(prefix)).ToList();

        if (!matchingFields.Any())
        {
            return 0;
        }

        var count = 0;
        foreach (var field in matchingFields)
        {
            db.HashDelete(_redisKey, field);
            count++;
        }

        return count;
    }
}
