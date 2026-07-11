/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Redis.Core
*文件名： RedisKeyExpireListener
*版本号： V1.0.0.0
*唯一标识：6d071947-8664-44e6-b854-96626998f8ee
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/8/20 9:58:30
*描述：redis键过期监听器
*
*=================================================
*修改标记
*修改时间：2025/8/20 9:58:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：redis键过期监听器
*
*****************************************************************************/
namespace LuBan.Redis.Core;

/// <summary>
/// redis键过期监听器
/// </summary>
public class RedisKeyExpireListener : IDisposable
{
    RedisClient _redisClient;
    int _dbIndex = 0;
    ISubscriber _subscriber;
    RedisChannel _redisChannel;

    /// <summary>
    /// redis键过期事件
    /// </summary>
    public event Action<RedisKeyExpireListener, string> OnKeyExpired;

    /// <summary>
    /// redis键过期监听器
    /// </summary>
    /// <param name="redisClient"></param>
    /// <param name="dbIndex"></param>
    internal RedisKeyExpireListener(RedisClient redisClient, int dbIndex = 0)
    {
        _redisClient = redisClient;
        _dbIndex = dbIndex;
        _subscriber = _redisClient.GetSubscriber();
        _redisChannel = new RedisChannel($"__keyevent@{_dbIndex}__:expired", RedisChannel.PatternMode.Auto);
    }

    /// <summary>
    /// 启动监听
    /// </summary>
    public async Task StartAsync()
    {
        //设置 notify-keyspace-events 为 Ex（监听过期事件）
        var servers = _redisClient.GetRedisServer();
        foreach (var server in servers)
        {
            await server.ConfigSetAsync("notify-keyspace-events", "Ex");
            //将当前配置写入 redis.conf，需要redis有权限
            try
            {
                await server.ConfigRewriteAsync();
            }
            catch { }
        }
        await _subscriber.SubscribeAsync(_redisChannel, KeyExpired);
    }

    /// <summary>
    /// 停止监听
    /// </summary>
    public async Task StopAsync()
    {
        await _subscriber.UnsubscribeAsync(_redisChannel);
    }

    /// <summary>
    /// 处理键过期事件
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="key"></param>
    private void KeyExpired(RedisChannel channel, RedisValue key)
    {
        OnKeyExpired?.Invoke(this, key.ToString());
    }


    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        try
        {
            StopAsync().GetAwaiter().GetResult();
        }
        catch { }
    }

}
