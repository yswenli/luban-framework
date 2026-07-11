/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.Data
*文件名： RefValue
*版本号： V1.0.0.0
*唯一标识：6b6fe3c2-7c6f-43df-8421-9eb503027a93
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/3/24 18:31:35
*描述：引用值,用于异步方法中不能用out返回值的情况
*
*=================================================
*修改标记
*修改时间：2025/3/24 18:31:35
*修改人： yswenli
*版本号： V1.0.0.0
*描述：引用值,用于异步方法中不能用out返回值的情况
*
*****************************************************************************/
namespace LuBan.Common.Data;

/// <summary>
/// 引用值，
/// 用于异步方法中不能用out返回值的情况
/// </summary>
/// <typeparam name="T"></typeparam>
public class RefValue<T>
{
    /// <summary>
    /// 值属性
    /// </summary>
    public T Value { get; set; }

    /// <summary>
    /// 引用值，
    /// 用于异步方法中不能用out返回值的情况
    /// </summary>
    /// <param name="value"></param>
    public RefValue(T value)
    {
        Value = value;
    }
}
