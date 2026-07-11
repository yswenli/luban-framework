/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Redis.Core
*文件名： RedisQueue
*版本号： V1.0.0.0
*唯一标识：cb72b941-2b3d-4822-825d-6bf8ce5b39c2
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/3/24 14:23:30
*描述：redis队列
*
*=================================================
*修改标记
*修改时间：2025/3/24 14:23:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：redis队列
*
*****************************************************************************/

namespace LuBan.Redis.Core;

/// <summary>
/// redis队列
/// </summary>
public class RedisQueue<T> where T : class, new()
{
    private RedisClient _redisClient;
    private string _topic;
    private TimeSpan _timeout;
    private int _dbIndex;

    /// <summary>
    /// redis队列
    /// </summary>
    /// <param name="redisClient"></param>
    /// <param name="topic"></param>
    /// <param name="timeout"></param>
    /// <param name="dbIndex"></param>
    internal RedisQueue(RedisClient redisClient, string topic, int timeout = 7 * 24 * 60 * 60 * 1000, int dbIndex = 0)
    {
        _redisClient = redisClient;
        _topic = topic;
        _timeout = TimeSpan.FromMilliseconds(timeout);
        _dbIndex = dbIndex;
    }

    /// <summary>
    /// 入队
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public string Enqueue(T value)
    {
        var key = GuidUtil.New;
        var db = _redisClient.GetDatabase(_dbIndex);
        if (db.ListRightPush(_topic, key) < 1)
        {
            return string.Empty;
        }
        var data = new QueueData()
        {
            Key = key,
            Input = value.ToJson(),
            Status = EnumProcessStatus.NotStart,
            Created = DateTime.Now
        };
        if (!db.StringSet(key, data.ToJson(), _timeout))
        {
            return string.Empty;
        }
        return key;
    }

    /// <summary>
    /// 入队
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task<string> EnqueueAsync(T value)
    {
        var key = GuidUtil.New;
        var db = _redisClient.GetDatabase(_dbIndex);
        if (await db.ListRightPushAsync(_topic, key) < 1)
        {
            return string.Empty;
        }
        var data = new QueueData()
        {
            Key = key,
            Input = value.ToJson(),
            Status = EnumProcessStatus.NotStart,
            Created = DateTime.Now
        };
        if (!await db.StringSetAsync(key, data.ToJson(), _timeout))
        {
            return string.Empty;
        }
        return key;
    }

    /// <summary>
    /// 出队
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public T? Dequeue(out string key)
    {
        var db = _redisClient.GetDatabase(_dbIndex);
        key = db.Dequeue(_topic) ?? string.Empty;
        if (string.IsNullOrEmpty(key))
        {
            return default;
        }
        var data = db.StringGet(key.ToString());
        if (string.IsNullOrEmpty(data))
        {
            return default;
        }
        var input = data.GetT<QueueData>()?.Input;
        if (input.IsNullOrEmpty()) return default;
        return SerializeUtil.Deserialize<T>(input);
    }

    /// <summary>
    /// 出队
    /// </summary>
    /// <param name="refKey"></param>
    /// <returns></returns>
    public async Task<T?> DequeueAsync(RefValue<string> refKey)
    {
        var db = _redisClient.GetDatabase(_dbIndex);
        var key = await db.DequeueAsync(_topic);
        refKey.Value = key ?? string.Empty;
        if (string.IsNullOrEmpty(key))
        {
            return default;
        }
        var data = await db.StringGetAsync(key.ToString());
        if (string.IsNullOrEmpty(data))
        {
            return default;
        }
        var input = data.GetT<QueueData>()?.Input;
        if (input.IsNullOrEmpty()) return default;
        return SerializeUtil.Deserialize<T>(input);

    }

    /// <summary>
    /// 出队
    /// </summary>
    /// <param name="blockTime"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public T? Dequeue(int blockTime, out string key)
    {
        key = string.Empty;
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed.TotalMilliseconds < blockTime)
        {
            try
            {
                var result = Dequeue(out key);
                if (result != default)
                {
                    return result;
                }
                else
                {
                    Thread.Sleep(100);
                    continue;
                }
            }
            catch { }
        }
        return default;
    }

    /// <summary>
    /// 出队
    /// </summary>
    /// <param name="blockTime"></param>
    /// <param name="refKey"></param>
    /// <returns></returns>
    public async Task<T?> DequeueAsync(int blockTime, RefValue<string> refKey)
    {
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed.TotalMilliseconds < blockTime)
        {
            try
            {
                var result = await DequeueAsync(refKey);
                if (result != default)
                {
                    return result;
                }
                else
                {
                    await Task.Delay(100);
                    continue;
                }
            }
            catch { }
        }
        return default;
    }

    /// <summary>
    /// 获取队列数据
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public QueueData? GetQueueData(string key)
    {
        var db = _redisClient.GetDatabase(_dbIndex);
        var data = db.StringGet(key);
        if (string.IsNullOrEmpty(data))
        {
            return default;
        }
        return data.GetT<QueueData>();
    }
    /// <summary>
    /// 获取队列数据
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<QueueData?> GetQueueDataAsync(string key)
    {
        var db = _redisClient.GetDatabase(_dbIndex);
        var data = await db.StringGetAsync(key);
        if (string.IsNullOrEmpty(data))
        {
            return default;
        }
        return data.GetT<QueueData>();
    }

    /// <summary>
    /// 更新队列数据
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool UpdateQueueData(QueueData data)
    {
        var db = _redisClient.GetDatabase(_dbIndex);
        return db.StringSet(data.Key, data.ToJson(), _timeout);
    }

    /// <summary>
    /// 更新队列数据
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public async Task<bool> UpdateQueueDataAsync(QueueData data)
    {
        var db = _redisClient.GetDatabase(_dbIndex);
        return await db.StringSetAsync(data.Key, data.ToJson(), _timeout);
    }

    /// <summary>
    /// 队列长度
    /// </summary>
    public int Length
    {
        get
        {
            var db = _redisClient.GetDatabase(_dbIndex);
            return (int)db.ListLength(_topic);
        }
    }

    /// <summary>
    /// 清空队列
    /// </summary>
    public void Clear()
    {
        var db = _redisClient.GetDatabase(_dbIndex);
        var list = db.ListRange(_topic);
        if (list != null && list.Length > 0)
            foreach (var key in list)
            {
                db.KeyDelete(key.ToString());
            }
        db.KeyDelete(_topic);
    }
    /// <summary>
    /// 清空队列
    /// </summary>
    /// <returns></returns>
    public async Task ClearAsync()
    {
        var db = _redisClient.GetDatabase(_dbIndex);
        var list = await db.ListRangeAsync(_topic);
        if (list != null && list.Length > 0)
            foreach (var key in list)
            {
                await db.KeyDeleteAsync(key.ToString());
            }
        await db.KeyDeleteAsync(_topic);
    }
}
