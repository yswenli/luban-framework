/****************************************************************************
*Copyright @ YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.Data
*文件名： ThreadSafeList
*版本号： V1.0.0.0
*唯一标识：79010b7d-2ae0-4711-9fa4-2f1417144db1
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/2/6 10:45:53
*描述：线程安全列表
*
*=================================================
*修改标记
*修改时间：2025/2/6 10:45:53
*修改人： yswenli
*版本号： V1.0.0.0
*描述：线程安全列表
*
*****************************************************************************/


namespace System.Collections.Generic;

/// <summary>
/// 线程安全列表项
/// </summary>
/// <typeparam name="T"></typeparam>
public class ThreadSafeListItem<T>
{
    /// <summary>
    /// 数据
    /// </summary>
    public T Item { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime Created { get; set; }
}

/// <summary>
/// 线程安全列表项超时委托
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="threadSafeList"></param>
/// <param name="t"></param>
public delegate void ThreadSafeListItemTimeOutDelegate<T>(ThreadSafeList<T> threadSafeList, T t);

/// <summary>
/// 线程安全列表
/// </summary>
/// <typeparam name="T"></typeparam>
public class ThreadSafeList<T> : ICollection<T>, IEnumerable<T>, IEnumerable
{
    private readonly List<ThreadSafeListItem<T>> _list;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly int _timeout = 0;

    /// <summary>
    /// 超时事件
    /// </summary>
    public event ThreadSafeListItemTimeOutDelegate<T> OnTimeout;

    /// <summary>
    /// 线程安全列表
    /// </summary>
    /// <param name="data"></param>
    /// <param name="timeout"></param>
    public ThreadSafeList(List<T>? data, int timeout = 0)
    {
        _list = [];
        if (data != null)
        {
            if (data.Count > 0)
                foreach (var item in data)
                {
                    _list.Add(new ThreadSafeListItem<T> { Item = item, Created = DateTime.Now });
                }
        }

        if (_timeout < 1) return;

        TaskUtil.WhileAsync(() =>
        {
            _semaphore.Wait();
            try
            {
                if (OnTimeout != null && _list != null && _list.Count > 0)
                {
                    var offset = DateTime.Now.AddMicroseconds(-timeout);
                    var list = _list.Where(q => q.Created <= offset).ToList();
                    if (list != null && list.Count > 0)
                    {
                        foreach (var item in list)
                        {
                            try
                            {
                                OnTimeout.Invoke(this, item.Item);
                            }
                            finally
                            {
                                _list.Remove(item);
                            }
                        }
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }

        }, 100);

    }


    /// <summary>
    /// 线程安全列表
    /// </summary>
    /// <param name="timeout"></param>
    public ThreadSafeList(int timeout) : this(null, timeout)
    {

    }

    /// <summary>
    /// 线程安全列表
    /// </summary>
    public ThreadSafeList() : this(0)
    {

    }

    /// <summary>
    /// 获取集合中的元素数量
    /// </summary>
    public int Count
    {
        get
        {
            _semaphore.Wait();
            try
            {
                return _list.Count;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

    /// <summary>
    /// 获取一个值，该值指示集合是否为只读
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// 从特定的数组索引开始，将集合的元素复制到数组中
    /// </summary>
    /// <param name="array"></param>
    /// <param name="arrayIndex"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public void CopyTo(T[] array, int arrayIndex)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));
        if (arrayIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (arrayIndex + Count > array.Length)
            throw new ArgumentException("目标数组中的可用空间不足");

        _semaphore.Wait();
        try
        {
            for (int i = 0; i < _list.Count; i++)
            {
                array[arrayIndex + i] = _list[i].Item;
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 返回一个循环访问集合的枚举器
    /// </summary>
    /// <returns></returns>
    public IEnumerator<T> GetEnumerator()
    {
        // 创建副本以避免枚举时持有锁
        T[] items = ToArray();
        return ((IEnumerable<T>)items).GetEnumerator();
    }

    /// <summary>
    /// 返回一个循环访问集合的枚举器
    /// </summary>
    /// <returns></returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// 添加元素
    /// </summary>
    /// <param name="item"></param>
    public void Add(T item)
    {
        _semaphore.Wait();
        try
        {
            _list.Add(new ThreadSafeListItem<T> { Item = item, Created = DateTime.Now });
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 添加元素
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public async Task AddAsync(T item)
    {
        await _semaphore.WaitAsync();
        try
        {
            _list.Add(new ThreadSafeListItem<T> { Item = item, Created = DateTime.Now });
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 移除元素
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Remove(T item)
    {
        _semaphore.Wait();
        try
        {
            var data = _list.FirstOrDefault(x => x.Item != null && x.Item.Equals(item));
            if (data != null)
                return _list.Remove(data);
            return false;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 移除元素
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public async Task<bool> RemoveAsync(T item)
    {
        await _semaphore.WaitAsync();
        try
        {
            var data = _list.FirstOrDefault(x => x.Item != null && x.Item.Equals(item));
            if (data != null)
                return _list.Remove(data);
            return false;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 获取元素
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public T Get(int index)
    {
        _semaphore.Wait();
        try
        {
            return _list[index].Item;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 获取元素
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public T? FirstOrDefault(Predicate<T> predicate)
    {
        _semaphore.Wait();
        try
        {
            var data = _list.FirstOrDefault(x => predicate(x.Item));
            if (data == null) return default;
            return data.Item;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 获取元素
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public async Task<T?> FirstOrDefaultAsync(Predicate<T> predicate)
    {
        await _semaphore.WaitAsync();
        try
        {
            var data = _list.FirstOrDefault(x => predicate(x.Item));
            if (data == null) return default;
            return data.Item;
        }
        finally
        {
            _semaphore.Release();
        }
    }


    /// <summary>
    /// 获取元素（优化版本，避免创建中间集合）
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public List<T> ToList(Predicate<T> predicate)
    {
        _semaphore.Wait();
        try
        {
            var result = new List<T>();
            for (int i = 0; i < _list.Count; i++)
            {
                if (predicate(_list[i].Item))
                {
                    result.Add(_list[i].Item);
                }
            }
            return result;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 获取元素（异步优化版本，避免创建中间集合）
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public async Task<List<T>> ToListAsync(Predicate<T> predicate)
    {
        await _semaphore.WaitAsync();
        try
        {
            var result = new List<T>();
            for (int i = 0; i < _list.Count; i++)
            {
                if (predicate(_list[i].Item))
                {
                    result.Add(_list[i].Item);
                }
            }
            return result;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 获取元素
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public async Task<T> GetAsync(int index)
    {
        await _semaphore.WaitAsync();
        try
        {
            return _list[index].Item;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 设置元素
    /// </summary>
    /// <param name="index"></param>
    /// <param name="value"></param>
    public void Set(int index, T value)
    {
        _semaphore.Wait();
        try
        {
            _list[index] = new ThreadSafeListItem<T> { Item = value, Created = DateTime.Now };
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 设置元素
    /// </summary>
    /// <param name="index"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task SetAsync(int index, T value)
    {
        await _semaphore.WaitAsync();
        try
        {
            _list[index] = new ThreadSafeListItem<T> { Item = value, Created = DateTime.Now };
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 获取元素
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public T GetOrSet(Predicate<T> predicate, Func<T> func)
    {
        _semaphore.Wait();
        try
        {
            var item = _list.FirstOrDefault(x => predicate(x.Item));
            if (item != null)
                return item.Item;
            var newItem = func();
            _list.Add(new ThreadSafeListItem<T> { Item = newItem, Created = DateTime.Now });
            return newItem;
        }
        finally
        {
            _semaphore.Release();
        }
    }
    /// <summary>
    /// 获取元素
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public async Task<T> GetOrSetAsync(Predicate<T> predicate, Func<Task<T>> func)
    {
        await _semaphore.WaitAsync();
        try
        {
            var item = _list.FirstOrDefault(x => predicate(x.Item));
            if (item != null)
                return item.Item;
            var newItem = await func();
            _list.Add(new ThreadSafeListItem<T> { Item = newItem, Created = DateTime.Now });
            return newItem;
        }
        finally
        {
            _semaphore.Release();
        }
    }


    /// <summary>
    /// 获取元素总数（异步方法版本，保持向后兼容）
    /// </summary>
    /// <returns></returns>
    public Task<int> CountAsync()
    {
        _semaphore.Wait();
        try
        {
            return Task.FromResult(_list.Count);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 检查元素是否存在
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Contains(T item)
    {
        _semaphore.Wait();
        try
        {
            return _list.Any(q => q.Item != null && q.Item.Equals(item));
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 检查元素是否存在
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public async Task<bool> ContainsAsync(T item)
    {
        await _semaphore.WaitAsync();
        try
        {
            return _list.Any(q => q.Item != null && q.Item.Equals(item));
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 查找元素（优化版本，避免创建中间集合）
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public T? Find(Predicate<T> predicate)
    {
        _semaphore.Wait();
        try
        {
            for (int i = 0; i < _list.Count; i++)
            {
                if (predicate(_list[i].Item))
                {
                    return _list[i].Item;
                }
            }
            return default;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 查找元素（异步优化版本，避免创建中间集合）
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public async Task<T?> FindAsync(Predicate<T> predicate)
    {
        await _semaphore.WaitAsync();
        try
        {
            for (int i = 0; i < _list.Count; i++)
            {
                if (predicate(_list[i].Item))
                {
                    return _list[i].Item;
                }
            }
            return default;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 查找元素（优化版本，避免创建中间集合）
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public List<T> FindAll(Predicate<T> predicate)
    {
        _semaphore.Wait();
        try
        {
            var result = new List<T>();
            for (int i = 0; i < _list.Count; i++)
            {
                if (predicate(_list[i].Item))
                {
                    result.Add(_list[i].Item);
                }
            }
            return result;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 查找元素（异步优化版本，避免创建中间集合）
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public async Task<List<T>> FindAllAsync(Predicate<T> predicate)
    {
        await _semaphore.WaitAsync();
        try
        {
            var result = new List<T>();
            for (int i = 0; i < _list.Count; i++)
            {
                if (predicate(_list[i].Item))
                {
                    result.Add(_list[i].Item);
                }
            }
            return result;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 遍历元素
    /// </summary>
    /// <param name="action"></param>
    public void ForEach(Action<T> action)
    {
        _semaphore.Wait();
        try
        {
            _list.ForEach((q) => action(q.Item));
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 遍历元素
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public async Task ForEachAsync(Action<T> action)
    {
        await _semaphore.WaitAsync();
        try
        {
            _list.ForEach((q) => action(q.Item));
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 清空集合
    /// </summary>
    public void Clear()
    {
        _semaphore.Wait();
        try
        {
            _list.Clear();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 清空集合
    /// </summary>
    /// <returns></returns>
    public async Task ClearAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            _list.Clear();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 转换为数组
    /// </summary>
    /// <returns></returns>
    public T[] ToArray()
    {
        _semaphore.Wait();
        try
        {
            return _list.Select(q => q.Item).ToArray();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 转换为数组
    /// </summary>
    /// <returns></returns>
    public async Task<T[]> ToArrayAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            return _list.Select(q => q.Item).ToArray();
        }
        finally
        {
            _semaphore.Release();
        }
    }
}

/// <summary>
/// 同ThreadSafeList
/// </summary>
/// <typeparam name="T"></typeparam>
public class ConcurrentList<T> : ThreadSafeList<T>
{
    /// <summary>
    /// 同ThreadSafeList
    /// </summary>
    /// <param name="data"></param>
    /// <param name="timeout"></param>
    public ConcurrentList(List<T>? data, int timeout = 0) : base(data, timeout)
    {
    }

    /// <summary>
    /// 同ThreadSafeList
    /// </summary>
    /// <param name="timeout"></param>
    public ConcurrentList(int timeout) : base(timeout)
    {

    }

    /// <summary>
    /// 同ThreadSafeList
    /// </summary>
    public ConcurrentList() : base()
    {

    }
}