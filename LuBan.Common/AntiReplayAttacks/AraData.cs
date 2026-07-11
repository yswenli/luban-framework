/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.AntiReplayAttacks
*文件名： AraData
*版本号： V1.0.0.0
*唯一标识：872c8a0f-7807-4bf8-a246-0284667b0603
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/4/14 17:40:03
*描述：安全校验参数类
*
*=================================================
*修改标记
*修改时间：2025/4/14 17:40:03
*修改人： yswenli
*版本号： V1.0.0.0
*描述：安全校验参数类
*
*****************************************************************************/
namespace LuBan.Common.AntiReplayAttacks;

/// <summary>
/// 安全校验参数类
/// </summary>
public class AraData : List<KeyValueStringString>, IDisposable
{
    protected readonly SortedDictionary<string, string> _data;
    /// <summary>
    /// 安全校验参数类
    /// </summary>
    public AraData()
    {
        _data = new SortedDictionary<string, string>();
    }

    /// <summary>
    /// 数量
    /// </summary>
    public new int Count
    {
        get { return _data.Count; }
    }

    /// <summary>
    /// 数据
    /// </summary>
    public SortedDictionary<string, string> Data
    {
        get { return _data; }
    }


    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void TryAdd(string key, string value)
    {
        _data.TryAdd(key, value);
    }

    /// <summary>
    /// 添加范围值
    /// </summary>
    /// <param name="data"></param>
    public void AddRange(SortedDictionary<string, string> data)
    {
        if (data != null && data.Count > 0)
        {
            foreach (var item in data)
            {
                TryAdd(item.Key, item.Value);
            }
        }
    }

    /// <summary>
    /// 是否包含
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool Contain(string key)
    {
        var keys = _data.Keys;
        if (keys == null || keys.Count < 1) return false;
        foreach (var item in keys)
        {
            if (item.IsNullOrEmpty()) return false;
            return item.Contains(key, true);
        }
        return false;
    }

    /// <summary>
    /// 清空
    /// </summary>
    public new void Clear()
    {
        _data.Clear();
    }

    /// <summary>
    /// 清空
    /// </summary>
    public void Dispose()
    {
        Clear();
    }

    /// <summary>
    /// 隐式转换成安全校验参数类
    /// </summary>
    /// <param name="data"></param>
    public static implicit operator AraData(SortedDictionary<string, string> data)
    {
        AraData result = new AraData();
        if (data != null && data.Count > 0)
            result.AddRange(data);
        return result;
    }

    /// <summary>
    /// 显式转换
    /// </summary>
    /// <param name="data"></param>
    public static explicit operator SortedDictionary<string, string>(AraData data)
    {
        if (data != null && data.Count > 0)
        {
            return data.Data;
        }
        return new SortedDictionary<string, string>();
    }
}
