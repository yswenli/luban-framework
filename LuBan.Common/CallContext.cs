/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： CallContext
*版本号： V1.0.0.0
*唯一标识：650d5814-2d1f-42ca-bccb-4d739fe981b7
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/8 18:28:12
*描述：
*
*=====================================================================
*修改标记
*修改时间：2022/7/8 18:28:12
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/

namespace LuBan.Common;

/// <summary>
/// 上下文对象，
/// 一般用于aspnetocre等服务端
/// </summary>
public static class CallContext
{
    /// <summary>
    /// 异步上下文对象,必须在父线程中赋值一次，否则其他子线程无法获取到数据
    /// </summary>
    static AsyncLocal<object> _asyncLocalValue;

    /// <summary>
    /// 上下文对象
    /// </summary>
    static CallContext()
    {
        _asyncLocalValue = new AsyncLocal<object>();
    }

    /// <summary>
    /// 设置数据。
    /// 必须在父线程中调用一次，否则其他子线程无法获取到数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="t"></param>
    public static void SetData<T>(T t)
    {
        if (t != null)
        {
            _asyncLocalValue.Value = t;
        }
    }

    /// <summary>
    /// 获取数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public static T? GetData<T>()
    {
        var data = _asyncLocalValue.Value;
        if (data == null) return default;
        return (T)data;
    }
}
