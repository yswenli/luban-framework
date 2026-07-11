/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： CollectionUtil
*版本号： V1.0.0.0
*唯一标识：ef9d9112-8296-416f-9c4c-9fc467bc0f83
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/3/3 11:29:16
*描述：列表工具类
*
*=================================================
*修改标记
*修改时间：2023/3/3 11:29:16
*修改人： yswenli
*版本号： V1.0.0.0
*描述：列表工具类
*
*****************************************************************************/

namespace System;

/// <summary>
/// 列表工具类
/// </summary>
public static class CollectionUtil
{

    /// <summary>
    /// 按指定条件获取元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arr"></param>
    /// <param name="match"></param>
    /// <returns></returns>
    public static T[]? Take<T>(this T[] arr, Predicate<T> match)
    {
        if (arr == null) return null;
        var length = arr.Length;
        if (length == 0) return null;
        int freeIndex = 0;
        while (freeIndex < length && !match(arr[freeIndex])) freeIndex++;
        if (freeIndex >= length) return null;

        int current = freeIndex + 1;
        while (current < length && match(arr[current])) current++;

        var result = new T[current];

        Array.Copy(arr, freeIndex, result, 0, current);

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            Array.Clear(arr, freeIndex, current);
        }

        return result;
    }

    /// <summary>
    /// 按指定条件移除元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arr"></param>
    /// <param name="match"></param>
    /// <returns></returns>
    public static int Remove<T>(this T[] arr, Predicate<T> match)
    {
        if (arr == null) return 0;
        var length = arr.Length;
        if (length == 0) return 0;
        int freeIndex = 0;
        while (freeIndex < length && !match(arr[freeIndex])) freeIndex++;
        if (freeIndex >= length) return 0;

        int current = freeIndex + 1;
        while (current < length && match(arr[current])) current++;

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            Array.Clear(arr, freeIndex, current);
        }
        return current - freeIndex;
    }

    /// <summary>
    /// 按指定条件移除元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="match"></param>
    /// <returns></returns>
    public static int Remove<T>(this List<T> list, Predicate<T> match)
    {
        return list.RemoveAll(match);
    }

    /// <summary>
    /// 按指定条件移除元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="match"></param>
    /// <returns></returns>
    public static int Remove<T>(this IList<T> list, Predicate<T> match)
    {
        if (list == null) return 0;
        var removes = new List<T>();
        foreach (var item in list)
        {
            if (match.Invoke(item))
            {
                removes.Add(item);
            }
        }
        if (removes.Count == 0) return 0;
        foreach (var item in removes)
        {
            list.Remove(item);
        }
        return removes.Count;
    }

    /// <summary>
    /// ForEach
    /// </summary>
    /// <param name="args"></param>
    /// <param name="action"></param>
    public static void ForEach(this string[]? args, Action<string, int> action)
    {
        if (args == null || args.Length < 1) return;
        int index = 0;
        foreach (var item in args)
        {
            action?.Invoke(item, index);
            index++;
        }
    }

    /// <summary>
    /// ForEach
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="action"></param>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
    {
        if (source == null || !source.Any()) return;
        int index = 0;
        foreach (var item in source)
        {
            action?.Invoke(item, index);
            index++;
        }
    }

    /// <summary>
    /// 字典添加或返回
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dic"></param>
    /// <param name="key"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static T GetOrAdd<T>(this IDictionary<string, T> dic, string key, Func<T> func)
    {
        using var locker = LockerBuilder.Default.Create("CollectionUtil.GetOrAdd");
        if (dic.ContainsKey(key))
        {
            return dic[key];
        }
        else
        {
            var val = func.Invoke();
            dic.Add(key, val);
            return val;
        }
    }

    /// <summary>
    /// 字典添加或更新
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dic"></param>
    /// <param name="key"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static bool AddOrUpdate<T>(this IDictionary<string, T> dic, string key, Func<T> func)
    {
        using var locker = LockerBuilder.Default.Create("CollectionUtil.GetOrAddT");
        var val = func.Invoke();
        if (dic.ContainsKey(key))
        {
            dic[key] = val;
            return false;
        }
        else
        {
            dic.Add(key, val);
            return true;
        }
    }
    /// <summary>
    /// 将字典转换成url中的query
    /// </summary>
    /// <param name="dic"></param>
    /// <returns></returns>
    public static string ToQuery(this IDictionary<string, object>? dic)
    {
        if (dic == null || dic.Count < 1) return string.Empty;
        var sp = new StringPlus("?");
        foreach (var kvp in dic)
        {
            sp.Append($"{kvp.Key}={kvp.Value?.ToString() ?? "".UrlEncode()}&");
        }
        sp.RemoveLast();
        return sp.ToString();
    }

    /// <summary>
    /// 将字典中转换成http请求form
    /// </summary>
    /// <param name="dic"></param>
    /// <returns></returns>
    public static string ToFormData(this IDictionary<string, object>? dic)
    {
        if (dic == null || dic.Count < 1) return string.Empty;
        var sp = new StringPlus("?");
        foreach (var kvp in dic)
        {
            sp.Append($"{kvp.Key}={kvp.Value?.ToString() ?? ""}&");
        }
        sp.RemoveLast();
        return sp.ToString();
    }

    /// <summary>
    /// 将字典转换成对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dic"></param>
    /// <returns></returns>
    public static T? ToObject<T>(this Dictionary<string, string> dic) where T : class, new()
    {
        if (dic == null || dic.Count < 1) return default;
        var json = SerializeUtil.Serialize(dic);
        return SerializeUtil.Deserialize<T>(json);
    }
}
