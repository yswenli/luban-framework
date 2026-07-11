/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： SmartList
*版本号： V1.0.0.0
*唯一标识：1f0fd094-580a-486a-8c80-be8978ae502d
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2021/6/2 11:56:18
*描述：可迭代删除的集合
*
*=====================================================================
*修改标记
*修改时间：2021/6/2 11:56:18
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：可迭代删除的集合
*
*****************************************************************************/
namespace LuBan.Common;

/// <summary>
/// 可迭代删除的集合
/// </summary>
/// <typeparam name="T"></typeparam>
public class SmartList<T> : IEnumerable, IDisposable
{
    SmartItemCollection<T> _list;

    /// <summary>
    /// 可迭代删除的集合
    /// </summary>
    public SmartList()
    {
        _list = new SmartItemCollection<T>();
    }

    /// <summary>
    /// 数量
    /// </summary>
    public int Count
    {
        get
        {
            return _list.Count;
        }
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="t"></param>
    public void Add(T t)
    {
        _list.Add(t);
    }

    /// <summary>
    /// 添加列表
    /// </summary>
    /// <param name="list"></param>
    public void AddRange(IEnumerable<T> list)
    {
        _list.AddRange(list);
    }

    /// <summary>
    /// 移除
    /// </summary>
    /// <param name="t"></param>
    public void Remove(T t)
    {
        _list.Remove(t);
    }

    /// <summary>
    /// 清空
    /// </summary>
    public void Clear()
    {
        _list.Clear();
    }


    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        Clear();
    }


    /// <summary>
    /// 获取枚举器
    /// </summary>
    /// <returns></returns>
    public IEnumerator GetEnumerator()
    {
        return _list;
    }

    /// <summary>
    /// 将list转换成smartlist
    /// </summary>
    /// <param name="list"></param>
    public static implicit operator SmartList<T>(List<T> list)
    {
        var sl = new SmartList<T>();
        sl.AddRange(list);
        return sl;
    }

    /// <summary>
    /// 将smartlist转换成list
    /// </summary>
    /// <param name="smartList"></param>
    public static implicit operator List<T>(SmartList<T> smartList)
    {
        List<T> list = new List<T>();
        foreach (T item in smartList)
        {
            list.Add(item);
        }
        return list;
    }
}

/// <summary>
/// 可迭代删除的集合
/// </summary>
/// <typeparam name="T"></typeparam>
public class SmartItemCollection<T> : IEnumerator
{
    List<T> _list = new();

    object? _current = null;

    /// <summary>
    /// 当前对象
    /// </summary>
    public object? Current
    {
        get { return _current; }
    }


    int _count = 0;

    /// <summary>
    /// 下一个
    /// </summary>
    /// <returns></returns>
    public bool MoveNext()
    {
        if (_count >= _list.Count)
        {
            return false;
        }
        else
        {
            _current = _list[_count];
            _count++;
            return true;
        }
    }

    /// <summary>
    /// 重置
    /// </summary>
    public void Reset()
    {
        _count = 0;
    }

    /// <summary>
    /// 数量
    /// </summary>
    public int Count
    {
        get
        {
            return _list.Count;
        }
    }

    /// <summary>
    /// 添加元素
    /// </summary>
    /// <param name="t"></param>
    public void Add(T t)
    {
        _list.Add(t);
    }

    /// <summary>
    /// 添加列表
    /// </summary>
    /// <param name="list"></param>
    public void AddRange(IEnumerable<T> list)
    {
        _list.AddRange(list);
    }

    /// <summary>
    /// 移除
    /// </summary>
    /// <param name="t"></param>
    public void Remove(T t)
    {
        if (_list.Contains(t))
        {
            if (_list.IndexOf(t) <= _count)
            {
                _count--;
            }
            _list.Remove(t);
        }
    }

    /// <summary>
    /// 清理
    /// </summary>
    public void Clear()
    {
        _list.Clear();
        _count = 0;
    }
}
