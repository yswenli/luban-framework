/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：System.Linq.Dynamic
*文件名： EnumerableTExtensions.cs
*版本号： V1.0.0.0
*唯一标识：bd5dcf74-731e-402e-84f8-84c26c707eef
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：EnumerableTExtensions 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：EnumerableTExtensions 类
*
*****************************************************************************/

namespace System.Linq.Dynamic;

/// <summary>
/// Enumerable<T>集合扩展方法
/// </summary>
public static class EnumerableTExtensions
{
    /// <summary>
    /// 通过动态表达式树对IEnumerable<TSource>进行筛选
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="filterField"></param>
    /// <param name="filterValue"></param>
    /// <returns></returns>
    public static IQueryable<TSource> Where<TSource>(this IEnumerable<TSource> source, string filterField, object? filterValue)
    {
        return source.AsQueryable().Where(filterField, filterValue);
    }


    /// <summary>
    /// 通过动态表达式树对IEnumerable<TSource>进行排序
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="orderByField"></param>
    /// <param name="isDesc"></param>
    /// <returns></returns>
    public static IQueryable<TSource> OrderBy<TSource>(this IEnumerable<TSource> source, string orderByField, bool isDesc)
    {
        return source.AsQueryable().OrderBy(orderByField, isDesc);
    }


    /// <summary>
    /// 通过动态表达式树对IEnumerable<TSource>进行取值
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="Val"></typeparam>
    /// <param name="source"></param>
    /// <param name="filterField"></param>
    /// <returns></returns>
    public static IQueryable<Val> Select<TSource, Val>(this IEnumerable<TSource> source, string filterField)
    {
        return source.AsQueryable().Select<TSource, Val>(filterField);
    }

    /// <summary>
    /// 泛型版本：判断IEnumerable中是否存在满足"指定字段 == 值"的元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="filterField"></param>
    /// <param name="val"></param>
    /// <returns></returns>
    public static bool Any<TSource>(this IEnumerable<TSource> source, string filterField, object val)
    {
        return source.AsQueryable().Any(filterField, val);
    }


    /// <summary>
    /// 泛型版本：判断IEnumerable中是否存在满足"指定字段 != 值"的元素
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="source"></param>
    /// <param name="filterField"></param>
    /// <param name="val"></param>
    /// <returns></returns>    
    public static bool NotAny<TSource>(this IEnumerable<TSource> source, string filterField, object val)
    {
        return !source.AsQueryable().Any(filterField, val);
    }

    /// <summary>
    /// linq分页
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="source"></param>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public static PagedList<TSource> ToPagedList<TSource>(this IEnumerable<TSource> source, int page, int pageSize)
    {
        return source.AsQueryable().ToPagedList(page, pageSize);
    }
}
