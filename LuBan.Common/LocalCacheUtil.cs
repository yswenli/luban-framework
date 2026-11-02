/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： LocalCacheUtil
*版本号： V1.0.0.0
*唯一标识：07478858-c23a-49c5-828f-588220fffcb1
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2021/5/28 15:06:58
*描述：本地缓存工具类
*
*=====================================================================
*修改标记
*修改时间：2021/5/28 15:06:58
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：本地缓存工具类
*
*****************************************************************************/
namespace LuBan.Common;

/// <summary>
/// 本地缓存工具类
/// </summary>
public static class LocalCacheUtil
{
    /// <summary>
    /// 缓存字典的命名锁；锁内只做字典读写，序列化与外部回调一律放到锁外
    /// </summary>
    const string LockName = "LocalCacheUtil.GetOrSet";

    static ConcurrentDictionary<string, LocalCacheItem> _dic;

    static PersistenceFile _persistenceUtil;

    static Timer _timer;

    /// <summary>
    /// 本地缓存工具类
    /// </summary>
    static LocalCacheUtil()
    {
        _persistenceUtil = new PersistenceFile("LuBanFrameworkLocalCache.data");
        try
        {
            _dic = _persistenceUtil.ReadData<ConcurrentDictionary<string, LocalCacheItem>>();
        }
        catch
        {
            _dic = new ConcurrentDictionary<string, LocalCacheItem>();
        }
        MaintainData();
    }

    /// <summary>
    /// 过期预告事件
    /// </summary>
    public static event OnHeraldHandler OnHerald;

    /// <summary>
    /// 过期事件委托
    /// </summary>
    public static event OnExpiredHandler OnExpired;

    /// <summary>
    /// 设置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="t"></param>
    /// <param name="expire"></param>
    /// <param name="heraldTime"></param>
    /// <param name="heralded"></param>
    public static void Set<T>(string key, T t, TimeSpan expire, int heraldTime = 10, bool heralded = false)
    {
        if (t == null)
        {
            return;
        }

        var item = CreateItem(t, expire, heraldTime, heralded);

        using (LockerBuilder.Default.Create(LockName))
        {
            _dic.AddOrUpdate(key, item, (k, v) => item);
        }
    }

    /// <summary>
    /// 获取
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public static T? Get<T>(string key)
    {
        string? value = null;
        var found = false;

        using (LockerBuilder.Default.Create(LockName))
        {
            if (_dic.TryGetValue(key, out LocalCacheItem? cacheItem) && cacheItem != null)
            {
                if (cacheItem.ExpireAt < DateTimeUtil.Now)
                {
                    RemoveItem(key);
                }
                else
                {
                    value = cacheItem.Value;
                    found = true;
                }
            }
        }

        if (!found || string.IsNullOrEmpty(value))
        {
            return default;
        }

        return SerializeUtil.Deserialize<T>(value!);
    }

    /// <summary>
    /// 设置过期
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expire"></param>
    /// <param name="heraldTime"></param>
    /// <param name="heralded"></param>
    public static void SetExpireTime(string key, TimeSpan expire, int heraldTime = 10, bool heralded = false)
    {
        using (LockerBuilder.Default.Create(LockName))
        {
            if (_dic.TryGetValue(key, out LocalCacheItem? cacheItem) && cacheItem != null)
            {
                cacheItem.ExpireAt = DateTimeUtil.Now.Add(expire);

                cacheItem.HeraldTime = heraldTime;

                cacheItem.Heralded = heralded;

                _dic.AddOrUpdate(key, cacheItem, (k, v) => cacheItem);
            }
        }
    }

    /// <summary>
    /// 设置过期
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expireAt"></param>
    /// <param name="heraldTime"></param>
    /// <param name="heralded"></param>
    public static void SetExpireTime(string key, DateTime expireAt, int heraldTime = 10, bool heralded = false)
    {
        using (LockerBuilder.Default.Create(LockName))
        {
            if (_dic.TryGetValue(key, out LocalCacheItem? cacheItem) && cacheItem != null)
            {
                cacheItem.ExpireAt = expireAt;

                cacheItem.HeraldTime = heraldTime;

                cacheItem.Heralded = heralded;

                _dic.AddOrUpdate(key, cacheItem, (k, v) => cacheItem);
            }
        }
    }

    /// <summary>
    /// 设置预告
    /// </summary>
    /// <param name="key"></param>
    /// <param name="heraldTime"></param>
    /// <param name="heralded"></param>
    public static void SetHerald(string key, int heraldTime = 10, bool heralded = false)
    {
        if (_dic.TryGetValue(key, out LocalCacheItem? cacheItem) && cacheItem != null)
        {
            cacheItem.HeraldTime = heraldTime;

            cacheItem.Heralded = heralded;

            _dic.AddOrUpdate(key, cacheItem, (k, v) => cacheItem);
        }
    }

    /// <summary>
    /// 清理缓存
    /// </summary>
    /// <param name="key"></param>
    public static bool Remove(string key)
    {
        using (LockerBuilder.Default.Create(LockName))
        {
            return RemoveItem(key);
        }
    }

    static bool _started = true;

    /// <summary>
    /// 运维数据
    /// </summary>
    static void MaintainData()
    {
        _timer = new Timer(new TimerCallback((o) =>
        {
            try
            {
                if (!_started)
                {
                    return;
                }
                var keys = _dic.Keys;
                var heraldKeys = new List<string>();
                var expiredItems = new List<(string Key, LocalCacheItem Item)>();

                using (LockerBuilder.Default.Create(LockName))
                {
                    foreach (var key in keys)
                    {
                        if (_dic.TryGetValue(key, out LocalCacheItem? item) && item != null)
                        {
                            if (item.ExpireAt < DateTimeUtil.Now.AddSeconds(item.HeraldTime) && !item.Heralded)
                            {
                                SetHerald(key, item.HeraldTime, false);

                                heraldKeys.Add(key);
                            }
                            else if (item.ExpireAt < DateTimeUtil.Now)
                            {
                                if (RemoveItem(key, out LocalCacheItem? removed) && removed != null)
                                {
                                    expiredItems.Add((key, removed));
                                }
                            }
                        }
                    }
                }

                foreach (var key in heraldKeys)
                {
                    OnHerald?.Invoke(key);
                }

                foreach (var (key, item) in expiredItems)
                {
                    OnExpired?.Invoke(key, item);
                }

                _persistenceUtil.WriteData(_dic);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

        }), null, 0, 1000);
    }

    /// <summary>
    /// 过期预告事件委托
    /// </summary>
    /// <param name="key"></param>
    public delegate void OnHeraldHandler(string key);

    /// <summary>
    /// 过期事件委托
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public delegate void OnExpiredHandler(string key, object value);

    /// <summary>
    /// 缓存项
    /// </summary>
    internal class LocalCacheItem
    {
        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime ExpireAt { get; set; }

        /// <summary>
        /// 过期预告
        /// </summary>
        public int HeraldTime { get; set; }

        /// <summary>
        /// 已预告
        /// </summary>
        public bool Heralded { get; set; }
    }

    /// <summary>
    /// 获取或更新
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="t"></param>
    /// <param name="expire"></param>
    /// <param name="heraldTime"></param>
    /// <param name="heralded"></param>
    /// <returns></returns>
    public static T? GetOrSet<T>(string key, T t, TimeSpan expire, int heraldTime = 10, bool heralded = false)
    {
        var cached = TryRead(key);

        if (cached.Found)
        {
            if (string.IsNullOrEmpty(cached.Value))
            {
                return default;
            }

            var old = SerializeUtil.Deserialize<T>(cached.Value!);
            if (old != null)
            {
                return old;
            }
        }

        if (t != null)
        {
            var item = CreateItem(t, expire, heraldTime, heralded);

            using (LockerBuilder.Default.Create(LockName))
            {
                _dic.AddOrUpdate(key, item, (k, v) => item);
            }
        }

        return t;
    }

    /// <summary>
    /// 获取或更新
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="func"></param>
    /// <param name="expire"></param>
    /// <param name="heraldTime"></param>
    /// <param name="heralded"></param>
    /// <returns></returns>
    /// <remarks>工厂在锁外执行：并发首次访问可能重复执行工厂，以最后一次写入为准</remarks>
    public static T? GetOrSet<T>(string key, Func<string, T> func, TimeSpan expire, int heraldTime = 10, bool heralded = false)
    {
        var cached = TryRead(key);

        if (cached.Found)
        {
            if (string.IsNullOrEmpty(cached.Value))
            {
                return default;
            }

            var old = SerializeUtil.Deserialize<T>(cached.Value!);
            if (old != null)
            {
                return old;
            }
        }

        var t = func.Invoke(key);

        if (t != null)
        {
            var item = CreateItem(t, expire, heraldTime, heralded);

            using (LockerBuilder.Default.Create(LockName))
            {
                _dic.AddOrUpdate(key, item, (k, v) => item);
            }
        }

        return t;
    }

    /// <summary>
    /// 关闭持久化和过期
    /// </summary>
    public static void Close()
    {
        _started = false;

        _timer.Dispose();
    }

    /// <summary>
    /// 读取缓存项原始值；过期项在读锁内移除。调用方须自行承担反序列化开销（锁外）
    /// </summary>
    static (bool Found, string? Value) TryRead(string key)
    {
        using (LockerBuilder.Default.Create(LockName))
        {
            if (_dic.TryGetValue(key, out LocalCacheItem? cacheItem) && cacheItem != null)
            {
                if (cacheItem.ExpireAt < DateTimeUtil.Now)
                {
                    RemoveItem(key);

                    return (false, null);
                }

                return (true, cacheItem.Value);
            }
        }

        return (false, null);
    }

    /// <summary>
    /// 组装缓存项（序列化在锁外完成）
    /// </summary>
    static LocalCacheItem CreateItem<T>(T t, TimeSpan expire, int heraldTime, bool heralded)
    {
        // 调用方均已排除 null（Set 直接返回，GetOrSet 判空后写入）
        var item = new LocalCacheItem()
        {
            Value = t!.ToJson(),
            ExpireAt = DateTimeUtil.Now.Add(expire),
            HeraldTime = heraldTime,
            Heralded = heralded
        };

        if (expire.TotalMilliseconds == -1)
        {
            item.ExpireAt = DateTime.MaxValue;
        }

        return item;
    }

    /// <summary>
    /// 移除缓存项（调用方须已持有 <see cref="LockName"/> 锁）
    /// </summary>
    static bool RemoveItem(string key) => RemoveItem(key, out _);

    /// <summary>
    /// 移除缓存项并返回被移除的实例（调用方须已持有 <see cref="LockName"/> 锁）
    /// </summary>
    static bool RemoveItem(string key, out LocalCacheItem? removed)
    {
        removed = null;

        if (_dic.TryRemove(key, out LocalCacheItem? item) && item != null && item.Value != null)
        {
            removed = item;

            return true;
        }

        return false;
    }
}