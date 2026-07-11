/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Redis.Core
*文件名： RedisConsumer
*版本号： V1.0.0.0
*唯一标识：4d9a1c87-207e-4d4d-8372-c5b9b3f1808e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/8/14 10:05:31
*描述：redis消费者
*
*=================================================
*修改标记
*修改时间：2025/8/14 10:05:31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：redis消费者
*
*****************************************************************************/
namespace LuBan.Redis.Core;

/// <summary>
/// redis消费者
/// </summary>
public class RedisConsumer
{
    private RedisClient _redisClient;
    private string _topic;
    private int _dbIndex;
    private string? _groupName;

    /// <summary>
    /// redis消费者
    /// </summary>
    /// <param name="redisClient"></param>
    /// <param name="topic"></param>
    /// <param name="dbIndex"></param>
    /// <param name="groupName"></param>
    public RedisConsumer(RedisClient redisClient, string topic, int dbIndex, string? groupName = "")
    {
        _redisClient = redisClient;
        _topic = topic;
        _dbIndex = dbIndex;
        _groupName = groupName;
    }

    /// <summary>
    /// 从组中获取消息
    /// </summary>
    /// <param name="count"></param>
    /// <param name="latest"></param>
    /// <returns></returns>
    public Dictionary<string, Dictionary<string, string>>? GetMessages(int count = 10, bool latest = true)
    {
        var db = _redisClient.GetDatabase(_dbIndex);
        StreamEntry[] msgs;
        if (latest)
        {
            if (_groupName.IsNotNullOrEmpty())
            {
                db.StreamCreateConsumerGroup(_topic, _groupName, StreamPosition.NewMessages);
                msgs = db.StreamReadGroup(_topic, _groupName, StreamPosition.NewMessages, count);
            }
            else
            {
                msgs = db.StreamRead(_topic, StreamPosition.NewMessages, count);
            }
        }
        else
        {
            if (_groupName.IsNotNullOrEmpty())
            {
                db.StreamCreateConsumerGroup(_topic, _groupName, StreamPosition.Beginning);
                msgs = db.StreamReadGroup(_topic, _groupName, StreamPosition.Beginning, count);
            }
            else
            {
                msgs = db.StreamRead(_topic, StreamPosition.Beginning, count);
            }
        }
        if (msgs != null && msgs.Length > 0)
        {
            return msgs.ToDictionary(x => x.Id.ToString(), x => x.Values.ToDictionary(y => y.Name.ToString(), y => y.Value.ToString()));
        }
        return null;
    }

    /// <summary>
    /// 从组中获取消息
    /// </summary>
    /// <param name="count"></param>
    /// <param name="latest"></param>
    /// <returns></returns>
    public async Task<Dictionary<string, Dictionary<string, string>>?> GetMessagesAsync(int count = 10, bool latest = true)
    {
        var db = _redisClient.GetDatabase(_dbIndex);
        StreamEntry[] msgs;
        if (latest)
        {
            if (_groupName.IsNotNullOrEmpty())
            {
                await db.StreamCreateConsumerGroupAsync(_topic, _groupName, StreamPosition.NewMessages);
                msgs = await db.StreamReadGroupAsync(_topic, _groupName, StreamPosition.NewMessages, count);
            }
            else
            {
                msgs = await db.StreamReadAsync(_topic, StreamPosition.NewMessages, count);
            }
        }
        else
        {
            if (_groupName.IsNotNullOrEmpty())
            {
                await db.StreamCreateConsumerGroupAsync(_topic, _groupName, StreamPosition.Beginning);
                msgs = await db.StreamReadGroupAsync(_topic, _groupName, StreamPosition.Beginning, count);
            }
            else
            {
                msgs = await db.StreamReadAsync(_topic, StreamPosition.Beginning, count);
            }
        }
        if (msgs != null && msgs.Length > 0)
        {
            return msgs.ToDictionary(x => x.Id.ToString(), x => x.Values.ToDictionary(y => y.Name.ToString(), y => y.Value.ToString()));
        }
        return null;
    }

    /// <summary>
    /// 从组中获取消息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="count"></param>
    /// <param name="latest"></param>
    /// <returns></returns>
    public List<StreamMessage<T>>? GetMessages<T>(int count = 10, bool latest = true) where T : class, new()
    {
        var dic = GetMessages(count, latest);
        if (dic == null || dic.Count < 1) return null;
        var result = new List<StreamMessage<T>>();
        foreach (var item in dic)
        {
            var t = item.Value.ToObject<T>();
            if (t == null) continue;
            result.Add(new StreamMessage<T>(item.Key, t));
        }
        return result;
    }

    /// <summary>
    /// 从组中获取消息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="count"></param>
    /// <param name="latest"></param>
    /// <returns></returns>
    public async Task<List<StreamMessage<T>>?> GetMessagesAsync<T>(int count = 10, bool latest = true) where T : class, new()
    {
        var dic = await GetMessagesAsync(count, latest);
        if (dic == null || dic.Count < 1) return null;
        var result = new List<StreamMessage<T>>();
        foreach (var item in dic)
        {
            var t = item.Value.ToObject<T>();
            if (t == null) continue;
            result.Add(new StreamMessage<T>(item.Key, t));
        }
        return result;
    }

    /// <summary>
    /// 处理完分组消息后确认消费，无groupName的无需调用
    /// </summary>
    /// <param name="msgIds"></param>
    /// <returns></returns>
    public long Acknowledge(List<string> msgIds)
    {
        var db = _redisClient.GetDatabase(_dbIndex);
        return db.StreamAcknowledge(_topic, _groupName, [.. msgIds]);
    }

    /// <summary>
    /// 处理完分组消息后确认消费，无groupName的无需调用
    /// </summary>
    /// <param name="msgIds"></param>
    /// <returns></returns>
    public async Task<long> AcknowledgeAsync(List<string> msgIds)
    {
        var db = _redisClient.GetDatabase(_dbIndex);
        return await db.StreamAcknowledgeAsync(_topic, _groupName, [.. msgIds]);
    }



}
