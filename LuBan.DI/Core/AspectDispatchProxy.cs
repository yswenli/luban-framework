/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Common.DI
*文件名： AspectDispatchProxy
*版本号： V1.0.0.0
*唯一标识：df9d8dc2-94fe-4244-acd6-327569da362b
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/6 15:06:05
*描述：异步代理分发类
*
*=================================================
*修改标记
*修改时间：2023/12/6 15:06:05
*修改人： yswenli
*版本号： V1.0.0.0
*描述：异步代理分发类
*
*****************************************************************************/

namespace LuBan.DI.Core;

/// <summary>
/// 异步代理分发类
/// </summary>
public abstract class AspectDispatchProxy
{
    /// <summary>
    /// 创建代理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProxy"></typeparam>
    /// <returns></returns>
    public static T Create<T, TProxy>() where TProxy : AspectDispatchProxy
    {
        return (T)AspectDispatchProxyGenerator.CreateProxyInstance(typeof(TProxy), typeof(T));
    }

    /// <summary>
    /// 执行同步代理
    /// </summary>
    /// <param name="method"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public abstract object Invoke(MethodInfo method, object[] args);

    /// <summary>
    /// 执行异步代理
    /// </summary>
    /// <param name="method"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public abstract Task InvokeAsync(MethodInfo method, object[] args);

    /// <summary>
    /// 执行异步返回 Task{T} 代理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="method"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public abstract Task<T> InvokeAsyncT<T>(MethodInfo method, object[] args);
}

