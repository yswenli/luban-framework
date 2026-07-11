/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Redis.Core
*文件名： RedisSubcriber
*版本号： V1.0.0.0
*唯一标识：3aec0875-5f26-4f91-9063-b32783abe033
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/8/20 10:24:59
*描述：redis订阅者
*
*=================================================
*修改标记
*修改时间：2025/8/20 10:24:59
*修改人： yswenli
*版本号： V1.0.0.0
*描述：redis订阅者
*
*****************************************************************************/
namespace LuBan.Redis.Core;

/// <summary>
/// redis订阅者
/// </summary>
public class RedisSubscriber : IDisposable
{
    RedisChannel _redisChannel;
    ISubscriber _subscriber;

    /// <summary>
    /// redis订阅事件
    /// </summary>
    public event Action<RedisSubscriber, string> OnMessageReceived;

    /// <summary>
    /// redis订阅者
    /// </summary>
    /// <param name="redisClient"></param>
    /// <param name="channel"></param>
    internal RedisSubscriber(RedisClient redisClient, string channel)
    {
        _redisChannel = new RedisChannel(channel, RedisChannel.PatternMode.Auto);
        _subscriber = redisClient.GetSubscriber();
    }

    /// <summary>
    /// 启动订阅
    /// </summary>
    /// <returns></returns>
    public async Task StartAsync()
    {
        await _subscriber.SubscribeAsync(_redisChannel, Reveived);
    }
    /// <summary>
    /// 停止订阅
    /// </summary>
    /// <returns></returns>
    public async Task StopAsync()
    {
        await _subscriber.UnsubscribeAsync(_redisChannel);
    }


    private void Reveived(RedisChannel channel, RedisValue val)
    {
        OnMessageReceived?.Invoke(this, val.ToString());
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

/// <summary>
/// redis订阅者
/// </summary>
/// <typeparam name="T"></typeparam>
public class RedisSubscriber<T> : IDisposable where T : class, new()
{
    RedisChannel _redisChannel;
    ISubscriber _subscriber;

    /// <summary>
    /// redis订阅事件
    /// </summary>
    public event Action<RedisSubscriber<T>, T?> OnMessageReceived;

    /// <summary>
    /// redis订阅者
    /// </summary>
    /// <param name="redisClient"></param>
    /// <param name="channel"></param>
    internal RedisSubscriber(RedisClient redisClient, string channel)
    {
        _redisChannel = new RedisChannel(channel, RedisChannel.PatternMode.Auto);
        _subscriber = redisClient.GetSubscriber();
    }

    /// <summary>
    /// 启动订阅
    /// </summary>
    /// <returns></returns>
    public async Task StartAsync()
    {
        await _subscriber.SubscribeAsync(_redisChannel, Reveived);
    }
    /// <summary>
    /// 停止订阅
    /// </summary>
    /// <returns></returns>
    public async Task StopAsync()
    {
        await _subscriber.UnsubscribeAsync(_redisChannel);
    }


    private void Reveived(RedisChannel channel, RedisValue val)
    {
        if (OnMessageReceived == null) return;
        var json = val.ToString();
        var t = SerializeUtil.Deserialize<T>(json);
        OnMessageReceived?.Invoke(this, t);
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
