/****************************************************************************
*Copyright (c) 2023 RiverLand All Rights Reserved.
*CLR版本： .net8
*机器名称：WALLE
*公司名称：Walle
*命名空间：Services.ApiServices
*文件名： SysCache
*版本号： V1.0.0.0
*唯一标识：601f539f-b892-40f0-a74c-6b32a8e99034
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/7 10:37:22
*描述：系统缓存服务
*
*=================================================
*修改标记
*修改时间：2023/12/7 10:37:22
*修改人： yswenli
*版本号： V1.0.0.0
*描述：系统缓存服务
*
*****************************************************************************/
namespace Services.ApiServices;

/// <summary>
/// 系统缓存服务
/// </summary>
public class CacheService : BaseService<CacheService>
{
    static MemoryCache<dynamic> _cache = MemoryCache<dynamic>.Instance;

    /// <summary>
    /// 获取缓存键名集合
    /// </summary>
    /// <returns></returns>
    public List<string> GetKeyList()
    {
        return [.. _cache.Keys];
    }


    /// <summary>
    /// 增加缓存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="timeOut"></param>
    /// <returns></returns>
    public bool Set(string key, object value, TimeSpan timeOut)
    {
        if (timeOut.TotalMilliseconds < 1)
        {
            _cache.Set(key, value);
        }
        else
        {
            _cache.Set(key, value, timeOut);
        }

        return true;
    }

    /// <summary>
    /// 增加缓存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="seconds"></param>
    /// <returns></returns>
    public bool Set(string key, object value, int seconds) => Set(key, value, TimeSpan.FromSeconds(seconds));

    /// <summary>
    /// 增加缓存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="timeOut"></param>
    /// <returns></returns>
    public bool Set(string key, object? value)
    {
        if (value == null) return false;
        _cache.Set(key, value);
        return true;
    }

    /// <summary>
    /// 获取缓存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public T? Get<T>(string key)
    {
        var val = _cache.Get(key);
        if (val != null)
        {
            if (val is T)
                return (T)val;
            return default;
        }
        return default;
    }

    /// <summary>
    /// 删除缓存
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public int Remove(string key)
    {
        _cache.Del(key);
        return 0;
    }

    /// <summary>
    /// 检查缓存是否存在
    /// </summary>
    /// <param name="key">键</param>
    /// <returns></returns>
    public bool ExistKey(string key)
    {
        return _cache.Exist(key);
    }


    /// <summary>
    /// 根据键名前缀删除缓存
    /// </summary>
    /// <param name="prefixKey">键名前缀</param>
    /// <returns></returns>
    public int RemoveByPrefixKey(string prefixKey)
    {
        var keys = GetKeyList().Where(q => q.StartsWith(prefixKey));
        if (keys != null && keys.Any())
        {
            foreach (var item in keys)
            {
                _cache.Del(item);
            }
        }
        return 0;
    }


    /// <summary>
    /// 根据键名前缀获取键名集合
    /// </summary>
    /// <param name="prefixKey">键名前缀</param>
    /// <returns></returns>
    public List<string> GetKeysByPrefixKey(string prefixKey)
    {
        return GetKeyList().Where(q => q.StartsWith(prefixKey)).ToList();
    }

    /// <summary>
    /// 获取缓存值
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string GetValue(string key)
    {
        var val = _cache.Get(key);
        if (val == null) return string.Empty;
        return SerializeUtil.Serialize(val);
    }




    /// <summary>
    /// 获取或添加缓存，在数据不存在时执行委托请求数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public T GetOrAdd<T>(string key, Func<string, T> callback)
    {
        var oldVal = _cache.Get(key);
        if (oldVal == null)
        {
            var val = callback.Invoke(key);
            if (val != null)
            {
                _cache.Set(key, val, TimeSpan.FromDays(10000));
            }
            return val;
        }
        else
        {
            return oldVal;
        }

    }
}
