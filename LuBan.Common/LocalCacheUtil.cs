/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
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
        using var lockInfo = LockerBuilder.Default.Create("LocalCacheUtil.GetOrSet");
        if (t != null)
        {
            var item = new LocalCacheItem()
            {
                Value = t.ToJson(),
                ExpireAt = DateTimeUtil.Now.Add(expire),
                HeraldTime = heraldTime,
                Heralded = heralded
            };
            if (expire.TotalMilliseconds == -1)
            {
                item.ExpireAt = DateTime.MaxValue;
            }
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
        using var lockInfo = LockerBuilder.Default.Create("LocalCacheUtil.GetOrSet");
        if (_dic.TryGetValue(key, out LocalCacheItem? cacheItem) && cacheItem != null)
        {
            if (cacheItem.ExpireAt < DateTimeUtil.Now)
            {
                Remove(key);
            }
            else
            {
                var val = cacheItem.Value;
                if (string.IsNullOrEmpty(val)) return default;
                return SerializeUtil.Deserialize<T>(val);
            }
        }
        return default;
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
        using var lockInfo = LockerBuilder.Default.Create("LocalCacheUtil.GetOrSet");
        if (_dic.TryGetValue(key, out LocalCacheItem? cacheItem) && cacheItem != null)
        {
            cacheItem.ExpireAt = DateTimeUtil.Now.Add(expire);

            cacheItem.HeraldTime = heraldTime;

            cacheItem.Heralded = heralded;

            _dic.AddOrUpdate(key, cacheItem, (k, v) => cacheItem);
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
        using var lockInfo = LockerBuilder.Default.Create("LocalCacheUtil.GetOrSet");
        if (_dic.TryGetValue(key, out LocalCacheItem? cacheItem) && cacheItem != null)
        {
            cacheItem.ExpireAt = expireAt;

            cacheItem.HeraldTime = heraldTime;

            cacheItem.Heralded = heralded;

            _dic.AddOrUpdate(key, cacheItem, (k, v) => cacheItem);
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
        using var lockInfo = LockerBuilder.Default.Create("LocalCacheUtil.GetOrSet");
        if (_dic.TryRemove(key, out LocalCacheItem? item) && item != null && item.Value != null)
        {
            var type = item.Value.GetType();

            var ief1 = type.GetInterface("IList");

            if (ief1 != null)
            {
                var list = (IList)ief1;
                list.Clear();
            }
            else
            {
                var ief2 = type.GetInterface("IDictionary");

                if (ief2 != null)
                {
                    var list = (IDictionary)ief2;
                    list.Clear();
                }
            }
            return true;
        }
        return false;
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
                using (var lockInfo = LockerBuilder.Default.Create("LocalCacheUtil.GetOrSet"))
                {
                    foreach (var key in keys)
                    {
                        if (_dic.TryGetValue(key, out LocalCacheItem? item) && item != null)
                        {
                            if (item.ExpireAt < DateTimeUtil.Now.AddSeconds(item.HeraldTime) && !item.Heralded)
                            {
                                OnHerald?.Invoke(key);

                                SetHerald(key, item.HeraldTime, false);
                            }
                            else if (item.ExpireAt < DateTimeUtil.Now)
                            {
                                if (_dic.TryRemove(key, out LocalCacheItem? lci) && item != null && item.Value != null)
                                {
                                    var type = item.Value.GetType();

                                    var ief1 = type.GetInterface("IList");

                                    if (ief1 != null)
                                    {
                                        var list = (IList)ief1;
                                        list.Clear();
                                    }
                                    else
                                    {
                                        var ief2 = type.GetInterface("IDictionary");

                                        if (ief2 != null)
                                        {
                                            var list = (IDictionary)ief2;
                                            list.Clear();
                                        }
                                    }

                                    OnExpired?.Invoke(key, lci);
                                }
                            }
                        }
                    }
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
        using var lockInfo = LockerBuilder.Default.Create("LocalCacheUtil.GetOrSet");

        T? old = default;

        if (_dic.TryGetValue(key, out LocalCacheItem? cacheItem) && cacheItem != null)
        {
            if (cacheItem.ExpireAt < DateTimeUtil.Now)
            {
                Remove(key);
            }
            else
            {
                var val = cacheItem.Value;
                if (string.IsNullOrEmpty(val)) return default;
                old = SerializeUtil.Deserialize<T>(val);
            }
        }

        if (old != null)
        {
            return old;
        }
        else
        {
            if (t != null)
            {
                var item = new LocalCacheItem()
                {
                    Value = t.ToJson(),
                    ExpireAt = DateTimeUtil.Now.Add(expire),
                    HeraldTime = heraldTime,
                    Heralded = heralded
                };

                _dic.AddOrUpdate(key, item, (k, v) => item);
            }

            return t;
        }

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
    public static T? GetOrSet<T>(string key, Func<string, T> func, TimeSpan expire, int heraldTime = 10, bool heralded = false)
    {
        using var lockInfo = LockerBuilder.Default.Create("LocalCacheUtil.GetOrSet");

        T? old = default;

        if (_dic.TryGetValue(key, out LocalCacheItem? cacheItem) && cacheItem != null)
        {
            if (cacheItem.ExpireAt < DateTimeUtil.Now)
            {
                Remove(key);
            }
            else
            {
                var val = cacheItem.Value;
                if (string.IsNullOrEmpty(val)) return default;
                old = SerializeUtil.Deserialize<T>(val);
            }
        }

        if (old != null)
        {
            return old;
        }
        else
        {
            var t = func.Invoke(key);

            if (t != null)
            {
                var item = new LocalCacheItem()
                {
                    Value = t.ToJson(),
                    ExpireAt = DateTimeUtil.Now.Add(expire),
                    HeraldTime = heraldTime,
                    Heralded = heralded
                };

                _dic.AddOrUpdate(key, item, (k, v) => item);
            }

            return t;
        }
    }

    /// <summary>
    /// 关闭持久化和过期
    /// </summary>
    public static void Close()
    {
        _started = false;

        _timer.Dispose();
    }
}
