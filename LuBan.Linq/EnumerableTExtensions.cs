/*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：System.Linq.Dynamic
*文件名： EnumerableTExtensions
*版本号： V1.0.0.0
*唯一标识：3a0499e4-68ef-4244-8707-17ecc8bef9df
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 18:21:32
* 描述：IEnumerable<T> 拓展
*
*=================================================
*修改标记
* 修改时间：2023 / 12 / 4 18:21:32
* 修改人： yswenli
* 版本号： V1.0.0.0
*描述：IEnumerable<T> 拓展
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
        var count = source.Count();
        var items = source.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var pagedList = new PagedList<TSource>();
        pagedList.Items = items;
        pagedList.Total = count;
        pagedList.Page = page;
        pagedList.PageSize = pageSize;
        return pagedList;
    }
}
