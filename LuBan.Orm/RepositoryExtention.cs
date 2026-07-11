/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm
*文件名： RepositoryExtention
*版本号： V1.0.0.0
*唯一标识：b709182b-bbc9-4692-85a0-a9f1fe36d0ac
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/5 13:46:19
*描述：数据仓储扩展类
*
*=================================================
*修改标记
*修改时间：2023/12/5 13:46:19
*修改人： yswenli
*版本号： V1.0.0.0
*描述：数据仓储扩展类
*
*****************************************************************************/
namespace System;

/// <summary>
/// 数据仓储扩展类
/// </summary>
public static class RepositoryExtention
{
    /// <summary>
    /// 将字典转换为SugarParameter列表
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static SugarParameter[] ToSugarParameters(this Dictionary<string, object>? parameters)
    {
        if (parameters == null || parameters.Count == 0) return [];
        var list = new List<SugarParameter>();
        foreach (var item in parameters)
        {
            list.Add(new SugarParameter(item.Key, item.Value));
        }
        return [.. list];
    }

    /// <summary>
    /// 集成基于input的条件排序
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    public static ISugarQueryable<T> OrderBy<T>(this ISugarQueryable<T> query, IBasePageInput input)
    {
        if (input == null) return query;
        if (input.Orders == null || input.Orders.Count == 0) return query;
        foreach (var item in input.Orders)
        {
            query.OrderByPropertyName(item.FieldName, item.IsAsc ? OrderByType.Asc : OrderByType.Desc);
        }
        return query;
    }



    /// <summary>
    /// 集成基于input的条件排序
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    public static ISugarQueryable<T> OrderBy<T>(this ISugarQueryable<T> query, BasePageOrder input)
    {
        if (input == null) return query;
        if (input.FieldName.IsNullOrEmpty()) return query;
        query = query.OrderByPropertyName(input.FieldName, input.IsAsc ? OrderByType.Asc : OrderByType.Desc);
        return query;
    }


    #region 分页 page 扩展


    /// <summary>
    /// 分页拓展
    /// </summary>
    /// <param name="query"><see cref="ISugarQueryable{TEntity}"/>对象</param>
    /// <param name="pageIndex">当前页码，从1开始</param>
    /// <param name="pageSize">页码容量</param>
    /// <param name="expression">查询结果 Select 表达式</param>
    /// <returns></returns>
    public static PagedList<TModel> ToPagedList<T, TModel>(this ISugarQueryable<T> query, int pageIndex, int pageSize, Expression<Func<T, TModel>> expression)
    {
        if (pageIndex <= 0) pageIndex = 1;
        if (pageSize <= 0) pageSize = 100;
        var total = 0;
        var items = query.ToPageList(pageIndex, pageSize, ref total, expression);
        return items.ToPagedList(total, pageIndex, pageSize);
    }

    /// <summary>
    /// 分页拓展
    /// </summary>
    /// <param name="query"><see cref="ISugarQueryable{TEntity}"/>对象</param>
    /// <param name="pageIndex">当前页码，从1开始</param>
    /// <param name="pageSize">页码容量</param>
    /// <returns></returns>
    public static PagedList<T> ToPagedList<T>(this ISugarQueryable<T> query, int pageIndex, int pageSize)
    {
        if (pageIndex <= 0) pageIndex = 1;
        if (pageSize <= 0) pageSize = 100;
        var total = 0;
        var items = query.ToPageList(pageIndex, pageSize, ref total);
        return items.ToPagedList(total, pageIndex, pageSize);
    }


    /// <summary>
    /// 分页拓展
    /// </summary>
    /// <param name="query"><see cref="ISugarQueryable{TEntity}"/>对象</param>
    /// <param name="pageIndex">当前页码，从1开始</param>
    /// <param name="pageSize">页码容量</param>
    /// <param name="expression">查询结果 Select 表达式</param>
    /// <returns></returns>
    public static async Task<PagedList<TModel>> ToPagedListAsync<T, TModel>(this ISugarQueryable<T> query, int pageIndex, int pageSize,
        Expression<Func<T, TModel>> expression)
    {
        if (pageIndex <= 0) pageIndex = 1;
        if (pageSize <= 0) pageSize = 100;
        RefAsync<int> total = 0;
        var items = await query.ToPageListAsync(pageIndex, pageSize, total, expression);
        return items.ToPagedList(total, pageIndex, pageSize);
    }

    /// <summary>
    /// 分页拓展
    /// </summary>
    /// <param name="query"><see cref="ISugarQueryable{TEntity}"/>对象</param>
    /// <param name="pageIndex">当前页码，从1开始</param>
    /// <param name="pageSize">页码容量</param>
    /// <returns></returns>
    public static async Task<PagedList<T>> ToPagedListAsync<T>(this ISugarQueryable<T> query, int pageIndex, int pageSize)
    {
        if (pageIndex <= 0) pageIndex = 1;
        if (pageSize <= 0) pageSize = 100;
        RefAsync<int> total = 0;
        var items = await query.ToPageListAsync(pageIndex, pageSize, total);
        return items.ToPagedList(total, pageIndex, pageSize);
    }

    /// <summary>
    /// 分页拓展
    /// </summary>
    /// <param name="list">集合对象</param>
    /// <param name="pageIndex">当前页码，从1开始</param>
    /// <param name="pageSize">页码容量</param>
    /// <returns></returns>
    public static PagedList<TModel> ToPagedList<TModel>(this IEnumerable<TModel> list, int pageIndex, int pageSize)
    {
        if (pageIndex <= 0) pageIndex = 1;
        if (pageSize <= 0) pageSize = 100;
        var total = list.Count();
        var items = list.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        return items.ToPagedList(total, pageIndex, pageSize);
    }

    /// <summary>
    /// 创建集成基于input的分页对象，
    /// 不包括where条件，需自行添加
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    public static PagedList<T> ToPagedList<T>(this ISugarQueryable<T> query, IBasePageInput input)
    {
        return query.OrderBy(input).ToPagedList(input.Page, input.PageSize);
    }


    /// <summary>
    /// 创建集成基于input的分页对象，
    /// 不包括where条件，需自行添加
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="query"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    public static PagedList<TModel> ToPagedList<T, TModel>(this ISugarQueryable<T> query, IBasePageInput input)
    {
        var pagedList = query.OrderBy(input).ToPagedList(input.Page, input.PageSize);
        var result = new PagedList<TModel>()
        {
            HasNextPage = pagedList.HasNextPage,
            HasPrevPage = pagedList.HasPrevPage,
            Page = pagedList.Page,
            PageSize = pagedList.PageSize,
            Total = pagedList.Total,
            TotalPages = pagedList.TotalPages
        };
        if (pagedList.Items.Any())
        {
            result.Items = pagedList.Items.Adapt<List<TModel>>();
        }
        return result;
    }

    /// <summary>
    /// 创建集成基于input的分页对象，
    /// 不包括where条件，需自行添加
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    public static async Task<PagedList<T>> ToPagedListAsync<T>(this ISugarQueryable<T> query, IBasePageInput input)
    {
        return await query.OrderBy(input).ToPagedListAsync(input.Page, input.PageSize);
    }


    /// <summary>
    /// 创建集成基于input的分页对象，
    /// 不包括where条件，需自行添加
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="query"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    public static async Task<PagedList<TModel>> ToPagedListAsync<T, TModel>(this ISugarQueryable<T> query, IBasePageInput input)
    {
        var pagedList = await query.OrderBy(input).ToPagedListAsync(input.Page, input.PageSize);
        var result = new PagedList<TModel>()
        {
            HasNextPage = pagedList.HasNextPage,
            HasPrevPage = pagedList.HasPrevPage,
            Page = pagedList.Page,
            PageSize = pagedList.PageSize,
            Total = pagedList.Total,
            TotalPages = pagedList.TotalPages
        };
        if (pagedList.Items.Any())
        {
            result.Items = pagedList.Items.Adapt<List<TModel>>();
        }
        return result;
    }

    /// <summary>
    /// 创建集成基于input的分页对象，
    /// 不包括where条件，需自行添加，
    /// 并返回树形结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="input"></param>
    /// <param name="idExpression"></param>
    /// <param name="childListExpression"></param>
    /// <param name="parentIdExpression"></param>
    /// <param name="rootValue"></param>
    /// <param name="maxLevel"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static PagedList<T> ToPagedTreeList<T>(this ISugarQueryable<T> query, IBasePageInput input,
        Expression<Func<T, object>> idExpression,
        Expression<Func<T, IEnumerable<object>>> childListExpression,
        Expression<Func<T, object?>> parentIdExpression,
        object? rootValue = null,
        int maxLevel = 3) where T : class, new()
    {
        var pagedList = query.ToPagedList(input);
        if (pagedList.Items.Any())
        {
            var source = pagedList.Items;
            if (source == null) return pagedList;
            var idName = idExpression.Body.GetMemberName();
            var childListName = childListExpression.Body.GetMemberName();
            var parentIdName = parentIdExpression.Body.GetMemberName();
            if (idName.IsNullOrEmpty()) throw new ArgumentException("idExpression is null or empty");
            if (childListName.IsNullOrEmpty()) throw new ArgumentException("childListExpression is null or empty");
            if (parentIdName.IsNullOrEmpty()) throw new ArgumentException("parentIdExpression is null or empty");
            if (rootValue == null)
            {
                rootValue = source.OrderBy(parentIdName, true).Select<T, object>(parentIdName).FirstOrDefault();
            }
            pagedList.Items = source.ToTreeList(idName, childListName, parentIdName, rootValue, maxLevel) ?? [];
        }
        return pagedList;
    }

    /// <summary>
    /// 创建集成基于input的分页对象，
    /// 不包括where条件，需自行添加，
    /// 并返回树形结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="input"></param>
    /// <param name="idExpression"></param>
    /// <param name="childListExpression"></param>
    /// <param name="parentIdExpression"></param>
    /// <param name="rootValue"></param>
    /// <param name="maxLevel"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static async Task<PagedList<T>> ToPagedTreeListAsync<T>(this ISugarQueryable<T> query, IBasePageInput input,
        Expression<Func<T, object>> idExpression,
        Expression<Func<T, IEnumerable<object>>> childListExpression,
        Expression<Func<T, object?>> parentIdExpression,
        object? rootValue = null,
        int maxLevel = 3) where T : class, new()
    {
        var pagedList = await query.ToPagedListAsync(input);
        if (pagedList.Items.Any())
        {
            var source = pagedList.Items;
            if (source == null) return pagedList;
            var idName = idExpression.Body.GetMemberName();
            var childListName = childListExpression.Body.GetMemberName();
            var parentIdName = parentIdExpression.Body.GetMemberName();
            if (idName.IsNullOrEmpty()) throw new ArgumentException("idExpression is null or empty");
            if (childListName.IsNullOrEmpty()) throw new ArgumentException("childListExpression is null or empty");
            if (parentIdName.IsNullOrEmpty()) throw new ArgumentException("parentIdExpression is null or empty");
            if (rootValue == null || query.Count() == 1)
            {
                rootValue = source.OrderBy(parentIdName, true).Select<T, object>(parentIdName).FirstOrDefault();
            }
            pagedList.Items = source.ToTreeList(idName, childListName, parentIdName, rootValue, maxLevel) ?? [];
        }
        return pagedList;
    }

    /// <summary>
    /// 创建集成基于input的分页对象，
    /// 不包括where条件，需自行添加，
    /// 并返回树形结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="query"></param>
    /// <param name="input"></param>
    /// <param name="idExpression"></param>
    /// <param name="childListExpression"></param>
    /// <param name="parentIdExpression"></param>
    /// <param name="rootValue"></param>
    /// <param name="maxLevel"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static PagedList<TModel> ToPagedTreeList<T, TModel>(this ISugarQueryable<T> query, IBasePageInput input,
        Expression<Func<T, object>> idExpression,
        Expression<Func<T, IEnumerable<object>>> childListExpression,
        Expression<Func<T, object?>> parentIdExpression,
        object? rootValue = null,
        int maxLevel = 3) where TModel : class, new()
    {
        var pagedList = query.ToPagedList<T, TModel>(input);
        if (pagedList.Items.Any())
        {
            var source = pagedList.Items;
            if (source == null) return pagedList;
            var idName = idExpression.Body.GetMemberName();
            var childListName = childListExpression.Body.GetMemberName();
            var parentIdName = parentIdExpression.Body.GetMemberName();
            if (idName.IsNullOrEmpty()) throw new ArgumentException("idExpression is null or empty");
            if (childListName.IsNullOrEmpty()) throw new ArgumentException("childListExpression is null or empty");
            if (parentIdName.IsNullOrEmpty()) throw new ArgumentException("parentIdExpression is null or empty");

            if (rootValue == null || source.NotAny(parentIdName, rootValue))
            {
                rootValue = source.OrderBy(parentIdName, true).Select<TModel, object>(parentIdName).FirstOrDefault();
            }
            pagedList.Items = source.ToTreeList(idName, childListName, parentIdName, rootValue, maxLevel) ?? [];
        }
        return pagedList;
    }


    /// <summary>
    /// 创建集成基于input的分页对象，
    /// 不包括where条件，需自行添加，
    /// 并返回树形结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="query"></param>
    /// <param name="input"></param>
    /// <param name="idExpression"></param>
    /// <param name="childListExpression"></param>
    /// <param name="parentIdExpression"></param>
    /// <param name="rootValue"></param>
    /// <param name="maxLevel"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static async Task<PagedList<TModel>> ToPagedTreeListAsync<T, TModel>(this ISugarQueryable<T> query, IBasePageInput input,
        Expression<Func<T, object>> idExpression,
        Expression<Func<T, IEnumerable<object>>> childListExpression,
        Expression<Func<T, object?>> parentIdExpression,
        object? rootValue = null,
        int maxLevel = 3) where TModel : class, new()
    {
        var pagedList = await query.ToPagedListAsync<T, TModel>(input);
        if (pagedList.Items.Any())
        {
            var source = pagedList.Items;
            if (source == null) return pagedList;
            var idName = idExpression.Body.GetMemberName();
            var childListName = childListExpression.Body.GetMemberName();
            var parentIdName = parentIdExpression.Body.GetMemberName();
            if (idName.IsNullOrEmpty()) throw new ArgumentException("idExpression is null or empty");
            if (childListName.IsNullOrEmpty()) throw new ArgumentException("childListExpression is null or empty");
            if (parentIdName.IsNullOrEmpty()) throw new ArgumentException("parentIdExpression is null or empty");
            if (rootValue == null)
            {
                rootValue = source.OrderBy(parentIdName, true).Select<TModel, object>(parentIdName).FirstOrDefault();
            }
            pagedList.Items = source.ToTreeList(idName, childListName, parentIdName, rootValue, maxLevel) ?? [];
        }
        return pagedList;
    }

    #endregion

    #region 申请使用过滤器

    /// <summary>
    /// 使用过滤器
    /// </summary>
    /// <typeparam name="Model"></typeparam>
    /// <param name="res"></param>
    /// <returns></returns>
    public static IDeleteable<Model> UseFilter<Model>(this IDeleteable<Model> res) where Model : class, IDeletedFilter, new()
    {
        return res.EnableQueryFilter();
    }
    /// <summary>
    /// 使用过滤器
    /// </summary>
    /// <typeparam name="Model"></typeparam>
    /// <param name="res"></param>
    /// <returns></returns>
    public static IUpdateable<Model> UseFilter<Model>(this IUpdateable<Model> res) where Model : class, IDeletedFilter, new()
    {
        return res.EnableQueryFilter();
    }
    /// <summary>
    /// 使用过滤器
    /// </summary>
    /// <typeparam name="Model"></typeparam>
    /// <param name="res"></param>
    /// <returns></returns>
    public static Subqueryable<Model> UseFilter<Model>(this Subqueryable<Model> res) where Model : class, IDeletedFilter, new()
    {
        return res.EnableTableFilter();
    }

    #endregion

    #region 额外增加条件

    /// <summary>
    /// 额外增加条件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="where"></param>
    /// <returns></returns>
    public static async Task<Expression<Func<T, bool>>?> GetExpressionAsync<T>(Expression<Func<T, bool>> where)
         where T : class, new()
    {
        return await Task.FromResult(Expressionable.Create<T>().And(where).ToExpression());
    }

    /// <summary>
    /// where条件增加额外条件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="queryable"></param>
    /// <param name="wheres"></param>
    /// <returns></returns>
    public static ISugarQueryable<T> And<T>(this ISugarQueryable<T> queryable, params Expression<Func<T, bool>>[] wheres)
         where T : class, new()
    {
        if (wheres == null || wheres.Length == 0) return queryable;
        var exp = Expressionable.Create<T>();
        foreach (var where in wheres)
        {
            exp = exp.And(where);
        }
        return queryable.Where(exp.ToExpression());
    }

    /// <summary>
    /// where条件增加额外条件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="queryable"></param>
    /// <param name="wheres"></param>
    /// <returns></returns>
    public static ISugarQueryable<T> Or<T>(this ISugarQueryable<T> queryable, params Expression<Func<T, bool>>[] wheres)
         where T : class, new()
    {
        if (wheres == null || wheres.Length == 0) return queryable;
        var exp = Expressionable.Create<T>();
        foreach (var where in wheres)
        {
            exp = exp.Or(where);
        }
        return queryable.Where(exp.ToExpression());
    }

    #endregion



    /// <summary>
    /// 获取映射SQL语句, 用于创建视图
    /// </summary>
    /// <param name="queryable"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static string GetQueryableSqlString<T>(this ISugarQueryable<T> queryable) where T : class
    {
        ArgumentNullException.ThrowIfNull(queryable);

        // 获取实体映射信息
        var entityInfo = queryable.Context.EntityMaintenance.GetEntityInfo(typeof(T));
        if (entityInfo?.Columns == null || entityInfo.Columns.Count == 0) return queryable.ToSqlString();

        // 构建需要替换的字段名映射（只处理实际有差异的字段）
        var nameMap = entityInfo.Columns
            .Where(c => !string.Equals(c.PropertyName, c.DbColumnName, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(k => k.PropertyName.ToLower(), v => v.DbColumnName, StringComparer.OrdinalIgnoreCase);
        if (nameMap.Count == 0) return queryable.ToSqlString();

        // 预编译正则表达式提升性能
        var sql = queryable.ToSqlString();
        foreach (var kv in nameMap)
        {
            sql = Regex.Replace(sql, $@"\b{kv.Key}\b", kv.Value ?? kv.Key, RegexOptions.IgnoreCase | RegexOptions.Compiled); // 单词边界匹配
        }
        return sql;
    }
}
