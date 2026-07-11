/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Redis.Core
*文件名： RedisPublisher
*版本号： V1.0.0.0
*唯一标识：df094fd7-5112-4fa8-b181-ab4e5b57ff9b
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/8/20 10:24:43
*描述：redis发布者
*
*=================================================
*修改标记
*修改时间：2025/8/20 10:24:43
*修改人： yswenli
*版本号： V1.0.0.0
*描述：redis发布者
*
*****************************************************************************/
namespace LuBan.Redis.Core;

/// <summary>
/// redis发布者
/// </summary>
public class RedisPublisher
{
    RedisChannel _redisChannel;
    ISubscriber _subscriber;

    /// <summary>
    /// redis发布者
    /// </summary>
    /// <param name="redisClient"></param>
    /// <param name="channel"></param>
    internal RedisPublisher(RedisClient redisClient, string channel)
    {
        _redisChannel = new RedisChannel(channel, RedisChannel.PatternMode.Auto);
        _subscriber = redisClient.GetSubscriber();
    }

    /// <summary>
    /// 发布消息
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task PublishAsync(string value)
    {
        await _subscriber.PublishAsync(_redisChannel, value);
    }

    /// <summary>
    /// 发布消息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="t"></param>
    /// <returns></returns>
    public async Task PublishAsync<T>(T t) where T : class, new()
    {
        var json = t.ToJson();
        await PublishAsync(json);
    }
}
