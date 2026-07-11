/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.Data
*文件名： HashSet
*版本号： V1.0.0.0
*唯一标识：d9055ce2-a760-4bbf-82fa-63711a40c571
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/12/5 14:55:20
*描述：
*
*=================================================
*修改标记
*修改时间：2025/12/5 14:55:20
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Common.Data;


/// <summary>
/// 线程安全的双层哈希集（基于ConcurrentDictionary实现）
/// 存储结构：T1 → T2 → T3（支持通过T1和T2快速定位T3）
/// </summary>
/// <typeparam name="T1">第一层键类型</typeparam>
/// <typeparam name="T2">第二层键类型</typeparam>
/// <typeparam name="T3">值类型</typeparam>
public class HashSet<T1, T2, T3> : IEnumerable<(T1, T2, T3)>
    where T1 : notnull
    where T2 : notnull
{
    // 底层存储：第一层字典（T1 → 第二层字典）
    private readonly ConcurrentDictionary<T1, ConcurrentDictionary<T2, T3>> _innerDictionary = new();

    /// <summary>
    /// 集合中T1的数量（第一层键的数量）
    /// </summary>
    public int CountT1 => _innerDictionary.Count;

    /// <summary>
    /// 集合中所有(T1,T2)键对的总数量
    /// </summary>
    public int TotalCount => _innerDictionary.Values.Sum(inner => inner.Count);

    /// <summary>
    /// 添加或更新元素
    /// </summary>
    /// <param name="key1">第一层键</param>
    /// <param name="key2">第二层键</param>
    /// <param name="value">关联的值</param>
    public void AddOrUpdate(T1 key1, T2 key2, T3 value)
    {
        // 确保第一层字典存在，不存在则创建
        var innerDict = _innerDictionary.GetOrAdd(key1, _ => new ConcurrentDictionary<T2, T3>());
        // 向第二层字典添加或更新值
        innerDict[key2] = value;
    }

    /// <summary>
    /// 尝试获取指定(T1,T2)对应的value
    /// </summary>
    /// <param name="key1">第一层键</param>
    /// <param name="key2">第二层键</param>
    /// <param name="value">输出值（存在则赋值）</param>
    /// <returns>是否存在该键对</returns>
    public bool TryGetValue(T1 key1, T2 key2, out T3? value)
    {
        value = default;
        // 先尝试获取第一层字典
        if (_innerDictionary.TryGetValue(key1, out var innerDict))
        {
            // 再尝试从第二层字典获取值
            return innerDict.TryGetValue(key2, out value);
        }
        return false;
    }

    /// <summary>
    /// 尝试删除指定(T1,T2)对应的元素
    /// </summary>
    /// <param name="key1">第一层键</param>
    /// <param name="key2">第二层键</param>
    /// <returns>是否删除成功</returns>
    public bool TryRemove(T1 key1, T2 key2)
    {
        if (_innerDictionary.TryGetValue(key1, out var innerDict))
        {
            // 删除第二层元素
            var removed = innerDict.TryRemove(key2, out _);
            // 如果第二层字典为空，删除第一层键（可选：避免空字典占用内存）
            if (innerDict.IsEmpty)
            {
                _innerDictionary.TryRemove(key1, out _);
            }
            return removed;
        }
        return false;
    }

    /// <summary>
    /// 删除指定T1下的所有元素
    /// </summary>
    /// <param name="key1">第一层键</param>
    /// <returns>是否删除成功</returns>
    public bool TryRemoveAll(T1 key1)
    {
        return _innerDictionary.TryRemove(key1, out _);
    }

    /// <summary>
    /// 检查是否包含指定(T1,T2)键对
    /// </summary>
    public bool Contains(T1 key1, T2 key2)
    {
        return _innerDictionary.TryGetValue(key1, out var innerDict) && innerDict.ContainsKey(key2);
    }

    /// <summary>
    /// 检查是否包含指定T1
    /// </summary>
    public bool ContainsKey1(T1 key1)
    {
        return _innerDictionary.ContainsKey(key1);
    }

    /// <summary>
    /// 获取指定T1下的所有(T2,T3)键值对
    /// </summary>
    public IEnumerable<(T2, T3)> GetValuesByKey1(T1 key1)
    {
        if (_innerDictionary.TryGetValue(key1, out var innerDict))
        {
            foreach (var (k2, v) in innerDict)
            {
                yield return (k2, v);
            }
        }
    }

    /// <summary>
    /// 清空所有元素
    /// </summary>
    public void Clear()
    {
        // 遍历并清空所有内层字典（避免直接替换为新字典导致并发问题）
        foreach (var innerDict in _innerDictionary.Values)
        {
            innerDict.Clear();
        }
        _innerDictionary.Clear();
    }

    /// <summary>
    /// 枚举所有元素（(T1,T2,T3)）
    /// </summary>
    public IEnumerator<(T1, T2, T3)> GetEnumerator()
    {
        foreach (var (k1, innerDict) in _innerDictionary)
        {
            foreach (var (k2, v) in innerDict)
            {
                yield return (k1, k2, v);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
