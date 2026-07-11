/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.Models
*文件名： KeyValue
*版本号： V1.0.0.0
*唯一标识：50e874e1-1c0a-4c9b-8250-3931e508c182
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/27 15:03:11
*描述：键值对
*
*=================================================
*修改标记
*修改时间：2025/10/27 15:03:11
*修改人： yswenli
*版本号： V1.0.0.0
*描述：键值对
*
*****************************************************************************/
namespace LuBan.Common.Models;

/// <summary>
/// 键值对
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public class KeyValue<TKey, TValue>
{
    /// <summary>
    /// 键
    /// </summary>
    public TKey Key { get; set; }
    /// <summary>
    /// 值
    /// </summary>
    public TValue Value { get; set; }

    /// <summary>
    /// 键值对
    /// </summary>
    /// <param name="keyValue"></param>
    /// <returns></returns>
    public static KeyValuePair<TKey, TValue> ToKeyValuePair(KeyValue<TKey, TValue> keyValue)
    {
        return new KeyValuePair<TKey, TValue>(keyValue.Key, keyValue.Value);
    }

    /// <summary>
    /// 隐式转换
    /// </summary>
    /// <param name="keyValue"></param>
    public static implicit operator KeyValuePair<TKey, TValue>(KeyValue<TKey, TValue> keyValue)
    {
        return ToKeyValuePair(keyValue);
    }

    /// <summary>
    /// 显式转换
    /// </summary>
    /// <param name="keyValue"></param>
    public static explicit operator KeyValue<TKey, TValue>(KeyValuePair<TKey, TValue> keyValue)
    {
        return new KeyValue<TKey, TValue>()
        {
            Key = keyValue.Key,
            Value = keyValue.Value
        };
    }
}

/// <summary>
/// 键值对，此类主要解决了KeyValuePair在swagger.json中的序列化成问题，
/// Schema name System.Collections.Generic.KeyValuePair`2[[System.String, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e],[System.String, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]
/// </summary>
public class KeyValueStringString : KeyValue<string, string>
{

}

/// <summary>
/// 键值对列表扩展
/// </summary>
public static class KeyValueListExctension
{
    /// <summary>
    /// 转换成字典
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="keyValues"></param>
    /// <returns></returns>
    public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this List<KeyValue<TKey, TValue>> keyValues) where TKey : notnull
    {
        if (keyValues == null || keyValues.Count == 0) return [];
        var result = new Dictionary<TKey, TValue>();
        foreach (var keyValue in keyValues)
        {
            result.TryAdd(keyValue.Key, keyValue.Value);
        }
        return result;
    }
    /// <summary>
    /// 转换成字典
    /// </summary>
    /// <param name="keyValues"></param>
    /// <returns></returns>
    public static Dictionary<string, string> ToDictionary(this List<KeyValueStringString> keyValues)
    {
        if (keyValues == null || keyValues.Count == 0) return [];
        var result = new Dictionary<string, string>();
        foreach (var keyValue in keyValues)
        {
            result.TryAdd(keyValue.Key, keyValue.Value);
        }
        return result;
    }

}

