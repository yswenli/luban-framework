/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：System
*文件名： LinQExtention
*版本号： V1.0.0.0
*唯一标识：3f2a0753-1d2d-4b31-8a23-b2f21babe6bd
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/5 15:09:22
*描述：
*
*=================================================
*修改标记
*修改时间：2023/12/5 15:09:22
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace System;

/// <summary>
/// Linq 拓展
/// </summary>
public static class LinqExpression
{
    /// <summary>
    /// 创建 Linq/Lambda 表达式
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <param name="expression">表达式</param>
    /// <returns>新的表达式</returns>
    public static Expression<Func<TSource, bool>> Create<TSource>(this Expression<Func<TSource, bool>> expression)
    {
        return expression;
    }

    /// <summary>
    /// 创建 Linq/Lambda 表达式，支持索引器
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <param name="expression">表达式</param>
    /// <returns>新的表达式</returns>
    public static Expression<Func<TSource, int, bool>> Create<TSource>(this Expression<Func<TSource, int, bool>> expression)
    {
        return expression;
    }

    /// <summary>
    /// 创建 And 表达式
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <returns>新的表达式</returns>
    public static Expression<Func<TSource, bool>> And<TSource>()
    {
        return u => true;
    }

    /// <summary>
    /// 创建 And 表达式，支持索引器
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <returns>新的表达式</returns>
    public static Expression<Func<TSource, int, bool>> IndexAnd<TSource>()
    {
        return (u, i) => true;
    }

    /// <summary>
    /// 创建 Or 表达式
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <returns>新的表达式</returns>
    public static Expression<Func<TSource, bool>> Or<TSource>()
    {
        return u => false;
    }

    /// <summary>
    /// 创建 Or 表达式，支持索引器
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <returns>新的表达式</returns>
    public static Expression<Func<TSource, int, bool>> IndexOr<TSource>()
    {
        return (u, i) => false;
    }
}