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
*描述：服务缓存接口，提供缓存的基本操作功能
*
*=====================================================================
*修改标记
*修改时间：2021/5/7 19:49:01
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：服务缓存接口，提供缓存的基本操作功能
*
*****************************************************************************/


namespace System;

/// <summary>
/// 服务缓存接口，提供缓存的基本操作功能
/// </summary>
public interface IServiceCache
{
    /// <summary>
    /// 索引器，通过键值对获取缓存中的动态类型值
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <returns>缓存值，可能为null</returns>
    dynamic? this[string key] { get; }

    /// <summary>
    /// 获取所有缓存键的数组
    /// </summary>
    string[] Keys { get; }

    /// <summary>
    /// 检查是否包含指定键的缓存项
    /// </summary>
    /// <param name="key">要检查的缓存键</param>
    /// <returns>如果包含返回true，否则返回false</returns>
    bool ContainsKey(string key);

    /// <summary>
    /// 删除指定键的缓存项
    /// </summary>
    /// <param name="key">要删除的缓存键</param>
    void Delete(string key);

    /// <summary>
    /// 获取指定键的缓存值，并转换为指定类型
    /// </summary>
    /// <typeparam name="T">要转换的类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <returns>转换后的值，可能为null</returns>
    T? Get<T>(string key);

    /// <summary>
    /// 获取缓存值，如果不存在则通过提供的函数计算并设置新值
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="addFun">用于计算新值的函数</param>
    /// <returns>获取或计算得到的值，可能为null</returns>
    T? GetOrSet<T>(string key, Func<string, T?> addFun);

    /// <summary>
    /// 获取缓存值，如果不存在则通过提供的函数计算并设置新值，并指定过期时间
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="addFun">用于计算新值的函数</param>
    /// <param name="timeOut">过期时间</param>
    /// <returns>获取或计算得到的值，可能为null</returns>
    T? GetOrSet<T>(string key, Func<string, T?> addFun, TimeSpan timeOut);

    /// <summary>
    /// 异步获取缓存值，如果不存在则通过提供的异步函数计算并设置新值，并指定过期时间
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="addFun">用于异步计算新值的函数</param>
    /// <param name="timeOut">过期时间</param>
    /// <returns>获取或计算得到的值的任务，结果可能为null</returns>
    Task<T?> GetOrSet<T>(string key, Func<string, Task<T?>> addFun, TimeSpan timeOut);

    /// <summary>
    /// 设置指定键的缓存值
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="value">要设置的值</param>
    void Set<T>(string key, T value);

    /// <summary>
    /// 设置指定键的缓存值，并指定过期时间
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="value">要设置的值</param>
    /// <param name="timeOut">过期时间</param>
    void Set<T>(string key, T value, TimeSpan timeOut);
}
