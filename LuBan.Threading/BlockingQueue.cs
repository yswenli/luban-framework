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
*描述：阻塞式队列
*
*=================================================
*修改标记
*修改时间：2023/9/14 15:21:27
*修改人： yswenli
*版本号： V1.0.0.0
*描述：阻塞式队列
*
*****************************************************************************/
namespace LuBan.Threading;

/// <summary>
/// 阻塞式队列
/// </summary>
/// <typeparam name="T"></typeparam>
public class BlockingQueue<T> : IDisposable
{
    private readonly LinkedList<T> _items = new();
    private readonly object _lock = new();
    private readonly ManualResetEvent _gate = new(false);
    private volatile bool _isDisposed = false;

    /// <summary>
    /// 长度
    /// </summary>
    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _items.Count;
            }
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
        if (_isDisposed) throw new ObjectDisposedException(nameof(BlockingQueue<T>));
        if (item == null) throw new ArgumentNullException(nameof(item));

        lock (_lock)
        {
            _items.AddLast(item);
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
            lock (_lock)
            {
                if (_items.Count > 0)
                {
                    var item = _items.First!.Value;
                    _items.RemoveFirst();
                    if (_items.Count == 0)
                        _gate.Reset();
                    return item;
                }
                if (_isDisposed)
                    return default;
            }

            if (!_gate.WaitOne(maxTimeout))
            {
                return default;
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
            lock (_lock)
            {
                if (_items.Count > 0)
                {
                    return _items.First!.Value;
                }
                if (_isDisposed)
                    return default;
            }

            if (!_gate.WaitOne(maxTimeout))
            {
                return default;
            }
        }
    }

    /// <summary>
    /// 移除首元素
    /// </summary>
    /// <param name="match"></param>
    public bool RemoveFirst(Predicate<T> match)
    {
        if (_isDisposed) throw new ObjectDisposedException(nameof(BlockingQueue<T>));
        if (match == null) throw new ArgumentNullException(nameof(match));

        lock (_lock)
        {
            if (_items.Count > 0 && match(_items.First!.Value))
            {
                _items.RemoveFirst();
                if (_items.Count == 0)
                    _gate.Reset();
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// 移除首元素
    /// </summary>
    /// <returns></returns>
    public T RemoveFirst()
    {
        if (_isDisposed) throw new ObjectDisposedException(nameof(BlockingQueue<T>));
        
        lock (_lock)
        {
            if (_items.Count == 0)
                throw new InvalidOperationException("队列为空，无法移除元素");
            var item = _items.First!.Value;
            _items.RemoveFirst();
            if (_items.Count == 0)
                _gate.Reset();
            return item;
        }
    }

    /// <summary>
    /// 查找元素
    /// </summary>
    /// <param name="match"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public T? FirstOrDefault(Predicate<T> match)
    {
        if (match == null) throw new ArgumentNullException(nameof(match));

        lock (_lock)
        {
            return _items.FirstOrDefault(x => match(x));
        }
    }

    /// <summary>
    /// 清理
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _items.Clear();
            _gate.Reset();
        }
    }

    /// <summary>
    /// dispose
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        Clear();
        _gate.Dispose();
    }
}
