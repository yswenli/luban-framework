/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Redis
*文件名： LuBanRedisExtentions
*版本号： V1.0.0.0
*唯一标识：4e3b3274-7f23-4100-b8bd-b46aa067149d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/3/24 11:26:48
*描述：LuBanRedis扩展类
*
*=================================================
*修改标记
*修改时间：2025/3/24 11:26:48
*修改人： yswenli
*版本号： V1.0.0.0
*描述：LuBanRedis扩展类
*
*****************************************************************************/

namespace LuBan.Redis;

/// <summary>
/// LuBanRedis扩展类
/// </summary>
public static class LuBanRedisExtentions
{
    static readonly string _prefix;

    /// <summary>
    /// redis key 工具类
    /// </summary>
    static LuBanRedisExtentions()
    {
        _prefix = MD5Util.GetMD5Str(ConfigUtil.Read("HostingOptions:Domain"));
    }

    /// <summary>
    /// 获取增加环境的key值
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string GetKeyByEnv(this string key)
    {
        if (key.IsNullOrEmpty()) throw new Exception("key can not be empty");
        return $"{key}:{_prefix}";
    }

    /// <summary>
    /// 判断是否为空
    /// </summary>
    /// <param name="val"></param>
    /// <returns></returns>
    public static bool IsNullOrEmpty([NotNullWhen(false)] this RedisValue val)
    {
        return val == RedisValue.Null || val == RedisValue.EmptyString;
    }

    /// <summary>
    /// 判断是否为空
    /// </summary>
    /// <param name="val"></param>
    /// <returns></returns>
    public static bool IsNotNullOrEmpty([NotNullWhen(true)] this RedisValue val)
    {
        return val != RedisValue.Null && val != RedisValue.EmptyString;
    }

    /// <summary>
    /// 获取T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="val"></param>
    /// <returns></returns>
    public static T? GetT<T>(this RedisValue val) where T : class, new()
    {
        return val.IsNullOrEmpty() ? default : SerializeUtil.Deserialize<T>(val.ToString());
    }


    /// <summary>
    /// 不存在则插入值
    /// </summary>
    /// <param name="database"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="timeOut"></param>
    /// <returns></returns>
    public static bool StringSetIfNotExists(this IDatabase database, string key, string value, int timeOut = 60 * 1000)
    {
        return database.StringSet(key, value, TimeSpan.FromMilliseconds(timeOut), false, When.NotExists);
    }
    /// <summary>
    /// 不存在则插入值
    /// </summary>
    /// <param name="database"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="timeOut"></param>
    /// <returns></returns>
    public static async Task<bool> StringSetIfNotExistsAsync(this IDatabase database, string key, string value, int timeOut = 60 * 1000)
    {
        return await database.StringSetAsync(key, value, TimeSpan.FromMilliseconds(timeOut), false, When.NotExists);
    }

    /// <summary>
    /// 入队
    /// </summary>
    /// <param name="database"></param>
    /// <param name="topic"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool Enqueue(this IDatabase database, string topic, string value)
    {
        return database.ListRightPush(topic, value) > 0;
    }

    /// <summary>
    /// 入队
    /// </summary>
    /// <param name="database"></param>
    /// <param name="topic"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static async Task<bool> EnqueueAsync(this IDatabase database, string topic, string value)
    {
        return await database.ListRightPushAsync(topic, value) > 0;
    }

    /// <summary>
    /// 入队
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="database"></param>
    /// <param name="topic"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool Enqueue<T>(this IDatabase database, string topic, T value) where T : class, new()
    {
        return database.ListRightPush(topic, SerializeUtil.Serialize(value)) > 0;
    }

    /// <summary>
    /// 入队
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="database"></param>
    /// <param name="topic"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static async Task<bool> EnqueueAsync<T>(this IDatabase database, string topic, T value) where T : class, new()
    {
        return await database.ListRightPushAsync(topic, SerializeUtil.Serialize(value)) > 0;
    }

    /// <summary>
    /// 出队
    /// </summary>
    /// <param name="database"></param>
    /// <param name="topic"></param>
    /// <returns></returns>
    public static string? Dequeue(this IDatabase database, string topic)
    {
        return database.ListLeftPop(topic);
    }

    /// <summary>
    /// 出队
    /// </summary>
    /// <param name="database"></param>
    /// <param name="topic"></param>
    /// <returns></returns>
    public static async Task<string?> DequeueAsync(this IDatabase database, string topic)
    {
        return await database.ListLeftPopAsync(topic);
    }

    /// <summary>
    /// 出队
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="database"></param>
    /// <param name="topic"></param>
    /// <returns></returns>
    public static T? Dequeue<T>(this IDatabase database, string topic) where T : class, new()
    {
        var rv = database.ListLeftPop(topic);
        if (rv.IsNullOrEmpty())
        {
            return default;
        }
        return SerializeUtil.Deserialize<T>(rv.ToString());
    }

    /// <summary>
    /// 出队
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="database"></param>
    /// <param name="topic"></param>
    /// <returns></returns>
    public static async Task<T?> DequeueAsync<T>(this IDatabase database, string topic) where T : class, new()
    {
        var rv = await database.ListLeftPopAsync(topic);
        if (rv.IsNullOrEmpty())
        {
            return default;
        }
        return SerializeUtil.Deserialize<T>(rv.ToString());
    }
}
