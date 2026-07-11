/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Threading
*文件名： BlockingQueue
*版本号： V1.0.0.0
*唯一标识：b3ba0f6e-ad41-4e51-8ab5-27d07a91467d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/9/14 15:21:27
*描述：阻塞试队列
*
*=================================================
*修改标记
*修改时间：2023/9/14 15:21:27
*修改人： yswenli
*版本号： V1.0.0.0
*描述：阻塞试队列
*
*****************************************************************************/
namespace LuBan.Threading;

/// <summary>
/// 阻塞试队列
/// </summary>
/// <typeparam name="T"></typeparam>
public class BlockingQueue<T> : IDisposable
{
    private readonly LinkedList<T> _items = new();
    private readonly ManualResetEvent _gate = new(true);


    int _readTime = 0;

    /// <summary>
    /// 长度
    /// </summary>
    public int Count
    {
        get
        {
            using var locker = LockerBuilder.Default.Create("BlockingQueue");
            return _items.Count;
        }
    }

    public bool IsEmpty
    {
        get
        {
            return Count == 0;
        }
    }

    /// <summary>
    /// 入队
    /// </summary>
    /// <param name="item"></param>
    public void Enqueue(T item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));

        using (var locker = LockerBuilder.Default.Create("BlockingQueue"))
        {
            _items.AddLast(item);
        }

        if (Interlocked.Exchange(ref _readTime, 1) == 0)
        {
            _gate.Set();
        }
    }

    /// <summary>
    /// 出队
    /// </summary>
    /// <param name="maxTimeout"></param>
    /// <returns></returns>
    public T? Dequeue(int maxTimeout = 10 * 1000)
    {
        while (true)
        {
            if (!_gate.WaitOne(maxTimeout))
            {
                _gate.Set();
                return default;
            }
            using var locker = LockerBuilder.Default.Create("BlockingQueue");
            if (_items != null && _items.Count > 0)
            {
                var item = _items.First();
                _items.RemoveFirst();
                _gate.Set();
                return item;
            }
            else
            {
                _gate.Reset();
                Interlocked.Exchange(ref _readTime, 0);
            }
        }
    }

    /// <summary>
    /// 出队
    /// </summary>
    /// <param name="timeOut"></param>
    /// <returns></returns>
    public T? Dequeue(TimeSpan timeOut)
    {
        return Dequeue((int)timeOut.TotalMilliseconds);
    }

    /// <summary>
    /// 查看
    /// </summary>
    /// <param name="maxTimeout"></param>
    /// <returns></returns>
    public T? PeekAndWait(int maxTimeout = 10 * 1000)
    {
        while (true)
        {
            if (!_gate.WaitOne(maxTimeout))
            {
                return default(T);
            }
            using var locker = LockerBuilder.Default.Create("BlockingQueue");
            if (_items.Count > 0)
            {
                _gate.Set();
                return _items.First();
            }
            else
            {
                _gate.Reset();
                Interlocked.Exchange(ref _readTime, 0);
            }
        }
    }

    /// <summary>
    /// 移除首元素
    /// </summary>
    /// <param name="match"></param>
    public void RemoveFirst(Predicate<T> match)
    {
        if (match == null) throw new ArgumentNullException(nameof(match));

        using var locker = LockerBuilder.Default.Create("BlockingQueue");
        if (_items.Count > 0 && match(_items.First()))
        {
            _items.RemoveFirst();
        }
    }

    /// <summary>
    /// 移除首元素
    /// </summary>
    /// <returns></returns>
    public T RemoveFirst()
    {
        using var locker = LockerBuilder.Default.Create("BlockingQueue");
        var item = _items.First();
        _items.RemoveFirst();
        return item;
    }

    /// <summary>
    /// 查找无素
    /// </summary>
    /// <param name="match"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public T? FirstOrDefault(Predicate<T> match)
    {
        if (match == null) throw new ArgumentNullException(nameof(match));

        using var locker = LockerBuilder.Default.Create("BlockingQueue");
        return _items.FirstOrDefault(x => match(x));
    }

    /// <summary>
    /// 清理
    /// </summary>
    public void Clear()
    {
        using var locker = LockerBuilder.Default.Create("BlockingQueue");
        _items.Clear();
        _gate.Set();
    }

    /// <summary>
    /// dispose
    /// </summary>
    public void Dispose()
    {
        Clear();
    }
}
