/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.DI.Core
*文件名： DispatchProxyHandler
*版本号： V1.0.0.0
*唯一标识：d77cb940-1533-426e-898a-d055adda0078
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/8/13 15:41:17
*描述：代理分发处理
*
*=================================================
*修改标记
*修改时间：2025/8/13 15:41:17
*修改人： yswenli
*版本号： V1.0.0.0
*描述：代理分发处理
*
*****************************************************************************/
namespace LuBan.DI.Core;

/// <summary>
/// 代理分发处理
/// </summary>
public class DispatchProxyHandler
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public DispatchProxyHandler()
    {
    }

    /// <summary>
    /// 同步处理
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public object InvokeHandle(object[] args)
    {
        return AspectDispatchProxyGenerator.Invoke(args);
    }

    /// <summary>
    /// 异步处理
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public Task InvokeAsyncHandle(object[] args)
    {
        return AspectDispatchProxyGenerator.InvokeAsync(args);
    }

    /// <summary>
    /// 异步带返回值处理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="args"></param>
    /// <returns></returns>
    public Task<T?> InvokeAsyncHandleT<T>(object[] args)
    {
        return AspectDispatchProxyGenerator.InvokeAsync<T>(args);
    }
}
