/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：System
*文件名： MemoryCache
*版本号： V1.0.0.0
*唯一标识：d0fcf315-0e95-4416-a3e2-577307c831ab
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2021/5/7 19:49:01
*描述：LuBan缓存
*
*=====================================================================
*修改标记
*修改时间：2021/5/7 19:49:01
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：LuBan缓存
*
*****************************************************************************/

namespace System;

/// <summary>
/// LuBan缓存
/// </summary>
/// <typeparam name="T"></typeparam>
public class MemoryCache<T> : BaseSingleInstance<MemoryCache<T>>, IDisposable
{
    ConcurrentDictionary<string, MemoryCacheItem> _dic;

    Timer _timer;

    /// <summary>
    /// 数据发生变化时事件
    /// </summary>
    public event Action<MemoryCache<T>, bool, T?> OnChanged;

    /// <summary>
    /// 自定义过期缓存
    /// </summary>
    /// <param name="seconds"></param>
    public MemoryCache(int seconds)
    {
        _dic = new ConcurrentDictionary<string, MemoryCacheItem>();

        _timer = new Timer(new TimerCallback((o) =>
        {
            var keys = _dic.Keys;

            if (keys != null && keys.Count > 0)
            {
                foreach (var key in keys)
                {
                    if (_dic.TryGetValue(key, out MemoryCacheItem? item) && item != null && item.Expired < DateTimeUtil.Now)
                    {
                        _ = _dic.TryRemove(key, out _);

                        if (item != null)
                        {
                            OnChanged?.Invoke(this, false, item.Value);
                        }
                    }
                }
            }
        }), null, 0, seconds);
    }

    /// <summary>
    /// 自定义过期缓存
    /// </summary>
    public MemoryCache() : this(10)
    {

    }

    /// <summary>
    /// Count
    /// </summary>
    public int Count
    {
        get
        {
            return _dic.Count;
        }
    }
    /// <summary>
    /// Keys
    /// </summary>
    public string[] Keys
    {
        get
        {
            return _dic.Keys.ToArray();
        }
    }

    /// <summary>
    /// 索引器
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public T? this[string key]
    {
        get
        {
            return Get(key);
        }
    }

    /// <summary>
    /// Set
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="timeout"></param>
    public void Set(string key, T value, TimeSpan timeout)
    {
        if (value != null)
        {
            if (timeout.TotalMilliseconds <= 0)
            {
                _dic[key] = new MemoryCacheItem() { Key = key, Value = value, Expired = DateTimeUtil.Now.AddDays(9999) };
            }
            else
            {
                _dic[key] = new MemoryCacheItem() { Key = key, Value = value, Expired = DateTimeUtil.Now.AddSeconds(timeout.TotalSeconds) };
            }
            OnChanged?.Invoke(this, true, value);
        }
    }
    /// <summary>
    /// Set
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Set(string key, T value)
    {
        Set(key, value, TimeSpan.FromMilliseconds(-1));
    }
    /// <summary>
    /// Set
    /// </summary>
    /// <param name="key"></param>
    /// <param name="func"></param>
    /// <param name="timeout"></param>
    public void Set(string key, Func<T> func, TimeSpan timeout)
    {
        Set(key, func.Invoke(), timeout);
    }
    /// <summary>
    /// Get
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public T? Get(string key)
    {
        if (_dic.TryGetValue(key, out MemoryCacheItem? mc) && mc != null)
        {
            if (mc.Expired > DateTimeUtil.Now)
            {
                return mc.Value;
            }
            else
            {
                if (_dic.TryRemove(key, out MemoryCacheItem? mc2) && mc2 != null)
                {
                    OnChanged?.Invoke(this, false, mc2.Value);
                }
            }
        }
        return default;
    }

    /// <summary>
    /// Get
    /// </summary>
    /// <typeparam name="Model"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public Model? Get<Model>(string key) where Model : class
    {
        var t = Get(key);
        if (t != null)
        {
            return t as Model;
        }
        return null;
    }

    /// <summary>
    /// 获取或设置
    /// </summary>
    /// <param name="key"></param>
    /// <param name="addFun"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public T? GetOrSet(string key, Func<string, T?> addFun, TimeSpan timeout)
    {
        using var lockInfo = LockerBuilder.Default.Create("MemoryCache.GetOrSet");

        var t = Get(key);

        if (t == null)
        {
            t = addFun.Invoke(key);
            if (t != null)
                Set(key, t, timeout);
            return t;
        }
        return t;
    }


    /// <summary>
    /// 异步获取或设置
    /// </summary>
    /// <param name="key"></param>
    /// <param name="addFun"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<T?> GetOrSetAsync(string key, Func<string, Task<T?>> addFun, TimeSpan timeout)
    {
        using var locker = await LockerBuilder.Default.CreateAsync("MemoryCache.GetOrSetAsync");
        var t = Get(key);
        if (t == null)
        {
            t = await addFun.Invoke(key);
            if (t != null)
                Set(key, t, timeout);
            return t;
        }
        return t;
    }

    /// <summary>
    /// 是否存在
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool Exist(string key)
    {
        return _dic.ContainsKey(key);
    }

    /// <summary>
    /// 取走缓存中的项
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public T? Take(string key)
    {
        _dic.TryRemove(key, out MemoryCacheItem? mc);

        if (mc != null)
        {
            return mc.Value;
        }

        return default;
    }
    /// <summary>
    /// Active
    /// </summary>
    /// <param name="key"></param>
    /// <param name="timeout"></param>
    public void Active(string key, TimeSpan timeout)
    {
        var item = Get(key);
        if (item != null)
        {
            Set(key, item, timeout);
        }
    }

    /// <summary>
    /// GetAndActive
    /// </summary>
    /// <param name="key"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public T? GetAndActive(string key, TimeSpan timeout)
    {
        using var lockInfo = LockerBuilder.Default.Create("MemoryCache.GetAndActive");

        if (_dic.TryGetValue(key, out MemoryCacheItem? mc))
        {
            if (mc != null && mc.Value != null)
            {
                if (mc!.Expired <= DateTimeUtil.Now)
                {
                    _dic.TryRemove(key, out mc);
                    OnChanged?.Invoke(this, false, mc!.Value);
                }
                else
                {
                    Set(key, mc.Value, timeout);
                    return mc.Value;
                }
            }
        }

        return default;
    }

    /// <summary>
    /// Del
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool Del(string key)
    {
        var result = _dic.TryRemove(key, out MemoryCacheItem? mc);
        if (result && mc != null)
        {
            OnChanged?.Invoke(this, false, mc.Value);
        }
        return result;
    }

    /// <summary>
    /// 获取元素列表
    /// </summary>
    /// <returns></returns>
    public List<T> ToList()
    {
        using var lockInfo = LockerBuilder.Default.Create("MemoryCache.ToList");
        var items = _dic.Values.Select(x => x.Value).ToList();
        if (items == null) return [];
        if (items.Count > 0)
        {
            var list = new List<T>();
            foreach (var item in items)
            {
                list.Add(item);
            }
            return list;
        }
        return [];
    }

    /// <summary>
    /// ContainsKey
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool ContainsKey(string key)
    {
        return _dic.ContainsKey(key);
    }

    /// <summary>
    /// DelWithoutEvent
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool DelWithoutEvent(string key)
    {
        return _dic.TryRemove(key, out _);
    }


    /// <summary>
    /// Clear
    /// </summary>
    public void Clear()
    {
        _dic.Clear();
        OnChanged?.Invoke(this, false, default);
    }



    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        Clear();
    }


    /// <summary>
    /// 创建缓存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static MemoryCache<Model> Create<Model>()
    {
        return MemoryCache<Model>.Instance;
    }
}

/// <summary>
/// 内存缓存
/// </summary>
public class MemoryCache : IServiceCache
{
    MemoryCache<dynamic> _cache;
    static Lazy<MemoryCache> _instance = new Lazy<MemoryCache>(() => new MemoryCache());
    /// <summary>
    /// 缓存
    /// </summary>
    public MemoryCache()
    {
        _cache = MemoryCache<dynamic>.Instance;
    }

    /// <summary>
    /// 实例
    /// </summary>
    public static MemoryCache Instance
    {
        get
        {
            return _instance.Value;
        }
    }

    /// <summary>
    /// 获取
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public dynamic? this[string key]
    {
        get
        {
            return _cache[key];
        }
    }

    /// <summary>
    /// Keys
    /// </summary>
    public string[] Keys
    {
        get
        {
            return _cache.Keys;
        }
    }

    /// <summary>
    /// 是否存在key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool ContainsKey(string key)
    {
        return _cache.ContainsKey(key);
    }

    /// <summary>
    /// 设置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="timeout"></param>
    public void Set<T>(string key, T value, TimeSpan timeout)
    {
        if (value != null)
            _cache.Set(key, value, timeout);
    }
    /// <summary>
    /// 设置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Set<T>(string key, T value)
    {
        if (value != null)
            _cache.Set(key, value);
    }

    /// <summary>
    /// 获取
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public T? Get<T>(string key)
    {
        var val = _cache.Get(key);
        if (val == null) return default;
        return (T)val;
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="key"></param>
    public void Delete(string key)
    {
        _cache.Del(key);
    }
    /// <summary>
    /// 获取或设置
    /// </summary>
    /// <param name="key"></param>
    /// <param name="addFun"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public T? GetOrSet<T>(string key, Func<string, T?> addFun, TimeSpan timeout)
    {
        var val = _cache.GetOrSet(key, (k) => { return addFun.Invoke(k); }, timeout);
        if (val == null) return default;
        return (T)val;
    }

    /// <summary>
    /// 获取或设置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="addFun"></param>
    /// <returns></returns>
    public T? GetOrSet<T>(string key, Func<string, T?> addFun)
    {
        return GetOrSet<T>(key, addFun, TimeSpan.FromMilliseconds(-1));
    }
    /// <summary>
    /// 获取或设置
    /// </summary>
    /// <param name="key"></param>
    /// <param name="addFun"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<T?> GetOrSet<T>(string key, Func<string, Task<T?>> addFun, TimeSpan timeout)
    {
        var val = await _cache.GetOrSet(key, addFun, timeout);
        if (val == null) return default;
        return (T)val;
    }
}
