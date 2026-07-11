/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Redis.Core
*文件名： RedisProducer
*版本号： V1.0.0.0
*唯一标识：c672a91d-41f8-4175-8c95-9b0ab2cc524e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/8/14 9:43:42
*描述：redis生产者
*
*=================================================
*修改标记
*修改时间：2025/8/14 9:43:42
*修改人： yswenli
*版本号： V1.0.0.0
*描述：redis生产者
*
*****************************************************************************/
namespace LuBan.Redis.Core;

/// <summary>
/// redis生产者
/// </summary>
public class RedisProducer
{
    private RedisClient _redisClient;
    private string _topic;
    private int _dbIndex;
    private int _maxLength;

    /// <summary>
    /// redis生产者
    /// </summary>
    /// <param name="redisClient"></param>
    /// <param name="topic"></param>
    /// <param name="dbIndex"></param>
    /// <param name="maxLength"></param>
    public RedisProducer(RedisClient redisClient, string topic, int dbIndex = 0, int maxLength = 100000)
    {
        _redisClient = redisClient;
        _topic = topic;
        _dbIndex = dbIndex;
        _maxLength = maxLength;
    }

    /// <summary>
    /// 发布消息
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="msgId"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public string Publish(Dictionary<string, object> msg, string? msgId = null)
    {
        if (msg == null || msg.Count == 0) throw new ArgumentNullException(nameof(msg));
        var messageFields = new List<NameValueEntry>();
        foreach (var item in msg)
        {
            var val = item.Value;
            if (val == null)
            {
                messageFields.Add(new NameValueEntry(item.Key, ""));
            }
            else if (val is DateTime dt)
            {
                messageFields.Add(new NameValueEntry(item.Key, dt.ToString("yyyy-MM-dd HH:mm:ss.fff")));
            }
            else
            {
                messageFields.Add(new NameValueEntry(item.Key, val.ToString()));
            }
        }
        var db = _redisClient.GetDatabase(_dbIndex);
        if (_maxLength <= 100000)
        {
            db.StreamTrim(_topic, _maxLength);
        }
        else
        {
            db.StreamTrim(_topic, _maxLength, true, 10000);
        }
        if (msgId == null)
        {
            return db.StreamAdd(_topic, [.. messageFields], null, _maxLength).ToString();
        }
        return db.StreamAdd(_topic, [.. messageFields], msgId, _maxLength).ToString();
    }


    /// <summary>
    /// 发布消息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="msg"></param>
    /// <param name="msgId"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public string Publish<T>(T msg, string? msgId = null) where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(msg);
        var dic = msg.ToDictionary();
        if (dic == null || dic.Count == 0) throw new ArgumentNullException(nameof(msg));
        return Publish(dic, msgId);
    }


    /// <summary>
    /// 发布消息
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="msgId"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<string> PublishAsync(Dictionary<string, object> msg, string? msgId = null)
    {
        if (msg == null || msg.Count == 0) throw new ArgumentNullException(nameof(msg));
        var messageFields = new List<NameValueEntry>();
        foreach (var item in msg)
        {
            var val = item.Value;
            if (val == null)
            {
                messageFields.Add(new NameValueEntry(item.Key, ""));
            }
            else if (val is DateTime dt)
            {
                messageFields.Add(new NameValueEntry(item.Key, dt.ToString("yyyy-MM-dd HH:mm:ss.fff")));
            }
            else
            {
                messageFields.Add(new NameValueEntry(item.Key, val.ToString()));
            }
        }
        var db = _redisClient.GetDatabase(_dbIndex);
        if (_maxLength <= 100000)
        {
            await db.StreamTrimAsync(_topic, _maxLength);
        }
        else
        {
            await db.StreamTrimAsync(_topic, _maxLength, true, 10000);
        }
        if (msgId == null)
        {
            return (await db.StreamAddAsync(_topic, [.. messageFields], null, _maxLength)).ToString();
        }
        return (await db.StreamAddAsync(_topic, [.. messageFields], msgId, _maxLength)).ToString();
    }

    /// <summary>
    /// 发布消息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="msg"></param>
    /// <param name="msgId"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<string> PublishAsync<T>(T msg, string? msgId = null) where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(msg);
        var dic = msg.ToDictionary();
        if (dic == null || dic.Count == 0) throw new ArgumentNullException(nameof(msg));
        return await PublishAsync(dic, msgId);
    }

}
