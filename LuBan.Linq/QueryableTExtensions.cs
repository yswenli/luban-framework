/*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：System.Linq.Dynamic
*文件名： QueryableTExtensions
*版本号： V1.0.0.0
*唯一标识：3a0499e4-68ef-4244-8707-17ecc8bef9df
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 18:21:32
* 描述：泛型IQueryable 拓展
*
*=================================================
*修改标记
* 修改时间：2023 / 12 / 4 18:21:32
* 修改人： yswenli
* 版本号： V1.0.0.0
*描述：泛型IQueryable 拓展
*
*****************************************************************************/
namespace System.Linq.Dynamic;

/// <summary>
/// 泛型IQueryable集合扩展方法
/// </summary>
public static class QueryableTExtensions
{
    /// <summary>
    /// 根据条件成立再构建 Where 查询
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <param name="sources">集合对象</param>
    /// <param name="condition">布尔条件</param>
    /// <param name="expression">表达式</param>
    /// <returns>新的集合对象</returns>
    public static IQueryable<TSource> Where<TSource>(this IQueryable<TSource> sources, bool condition, Expression<Func<TSource, bool>> expression)
    {
        return condition ? Queryable.Where(sources, expression) : sources;
    }

    /// <summary>
    /// 根据条件成立再构建 Where 查询，支持索引器
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <param name="sources">集合对象</param>
    /// <param name="condition">布尔条件</param>
    /// <param name="expression">表达式</param>
    /// <returns>新的集合对象</returns>
    public static IQueryable<TSource> Where<TSource>(this IQueryable<TSource> sources, bool condition, Expression<Func<TSource, int, bool>> expression)
    {
        return condition ? Queryable.Where(sources, expression) : sources;
    }

    /// <summary>
    /// 与操作合并多个表达式
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <param name="sources">集合对象</param>
    /// <param name="expressions">表达式数组</param>
    /// <returns>新的集合对象</returns>
    public static IQueryable<TSource> Where<TSource>(this IQueryable<TSource> sources, params Expression<Func<TSource, bool>>[] expressions)
    {
        if (expressions == null || !expressions.Any()) return sources;
        if (expressions.Length == 1) return Queryable.Where(sources, expressions[0]);

        var expression = LinqExpression.Or<TSource>();
        foreach (var _expression in expressions)
        {
            expression = expression.Or(_expression);
        }
        return Queryable.Where(sources, expression);
    }

    /// <summary>
    /// 与操作合并多个表达式，支持索引器
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <param name="sources">集合对象</param>
    /// <param name="expressions">表达式数组</param>
    /// <returns>新的集合对象</returns>
    public static IQueryable<TSource> Where<TSource>(this IQueryable<TSource> sources, params Expression<Func<TSource, int, bool>>[] expressions)
    {
        if (expressions == null || !expressions.Any()) return sources;
        if (expressions.Length == 1) return Queryable.Where(sources, expressions[0]);

        var expression = LinqExpression.IndexOr<TSource>();
        foreach (var _expression in expressions)
        {
            expression = expression.Or(_expression);
        }
        return Queryable.Where(sources, expression);
    }

    /// <summary>
    /// 根据条件成立再构建 WhereOr 查询
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <param name="sources">集合对象</param>
    /// <param name="conditionExpressions">条件表达式</param>
    /// <returns>新的集合对象</returns>
    public static IQueryable<TSource> Where<TSource>(this IQueryable<TSource> sources, params (bool condition, Expression<Func<TSource, bool>> expression)[] conditionExpressions)
    {
        var expressions = new List<Expression<Func<TSource, bool>>>();
        foreach (var (condition, expression) in conditionExpressions)
        {
            if (condition) expressions.Add(expression);
        }
        return Where(sources, expressions.ToArray());
    }

    /// <summary>
    /// 根据条件成立再构建 WhereOr 查询，支持索引器
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <param name="sources">集合对象</param>
    /// <param name="conditionExpressions">条件表达式</param>
    /// <returns>新的集合对象</returns>
    public static IQueryable<TSource> Where<TSource>(this IQueryable<TSource> sources, params (bool condition, Expression<Func<TSource, int, bool>> expression)[] conditionExpressions)
    {
        var expressions = new List<Expression<Func<TSource, int, bool>>>();
        foreach (var (condition, expression) in conditionExpressions)
        {
            if (condition) expressions.Add(expression);
        }
        return Where(sources, expressions.ToArray());
    }

    /// <summary>
    /// 根据条件成立再构建 Where 查询
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <param name="sources">集合对象</param>
    /// <param name="condition">布尔条件</param>
    /// <param name="expression">表达式</param>
    /// <returns>新的集合对象</returns>
    public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> sources, bool condition, Func<TSource, bool> expression)
    {
        return condition ? sources.Where(expression) : sources;
    }

    /// <summary>
    /// 根据条件成立再构建 Where 查询，支持索引器
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <param name="sources">集合对象</param>
    /// <param name="condition">布尔条件</param>
    /// <param name="expression">表达式</param>
    /// <returns>新的集合对象</returns>
    public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> sources, bool condition, Func<TSource, int, bool> expression)
    {
        return condition ? sources.Where(expression) : sources;
    }

    /// <summary>
    /// 通过动态表达式树对IQueryable<TSource>进行筛选
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="source"></param>
    /// <param name="filterField">筛选字段（如"Age"）</param>
    /// <param name="filterValue">筛选值（如30）</param>
    /// <returns></returns>
    public static IQueryable<TSource> Where<TSource>(this IQueryable<TSource> source, string filterField, object? filterValue)
    {
        var parameter = Expression.Parameter(typeof(TSource), "x");
        var property = Expression.Property(parameter, filterField);
        var constant = Expression.Constant(filterValue);
        var body = Expression.Equal(property, constant);
        var lambda = Expression.Lambda<Func<TSource, bool>>(body, parameter);
        return source.Where(lambda);
    }

    /// <summary>
    /// 通过动态表达式树对IQueryable<TSource>进行排序
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="source"></param>
    /// <param name="orderByField"></param>
    /// <param name="isDesc"></param>
    /// <returns></returns>
    public static IQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> source, string orderByField, bool isDesc)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source), "原始IQueryable不能为null");
        if (string.IsNullOrWhiteSpace(orderByField))
            throw new ArgumentException("排序字段名不能为空", nameof(orderByField));

        var parameter = Expression.Parameter(typeof(TSource), "x");
        var property = Expression.Property(parameter, orderByField);
        var lambda = Expression.Lambda(property, parameter);
        var methodName = isDesc ? "OrderByDescending" : "OrderBy";
        var method = typeof(Queryable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == methodName && m.GetParameters().Length == 2)
            .Single()
            .MakeGenericMethod(typeof(TSource), property.Type);
        var result = method.Invoke(null, [source, lambda]);
        return (IQueryable<TSource>)result!;
    }

    /// <summary>
    /// 将IQueryable<TSource>动态投影为仅包含指定filterField的IQueryable
    /// </summary>
    /// <typeparam name="TSource">源集合元素类型（编译时确定，避免反射获取类型）</typeparam>
    /// <param name="source">泛型数据源IQueryable<TSource></param>
    /// <param name="filterField">需投影的字段名（必须是T的公共实例属性）</param>
    /// <returns>投影后的非泛型IQueryable（元素为filterField的字段值，类型与字段一致）</returns>
    /// <exception cref="ArgumentNullException">source或filterField为null</exception>
    /// <exception cref="ArgumentException">filterField无效（不存在/非公共实例属性）</exception>
    public static IQueryable Select<TSource>(this IQueryable<TSource> source, string filterField)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source), "源IQueryable<TSource>不能为null");
        if (string.IsNullOrWhiteSpace(filterField))
            throw new ArgumentException("投影字段名不能为空或空白", nameof(filterField));

        PropertyInfo? fieldProperty = typeof(TSource).GetProperty(
            name: filterField,
            bindingAttr: BindingFlags.Public | BindingFlags.Instance);

        if (fieldProperty == null)
        {
            // 明确提示错误原因：字段不存在/非公共/静态等
            throw new ArgumentException(
                message: $"泛型类型 {typeof(TSource).FullName} 不存在公共实例属性 {filterField}（请检查：1. 字段名拼写/大小写；2. 是否为静态属性；3. 是否为私有属性/字段）",
                paramName: nameof(filterField));
        }
        Type fieldType = fieldProperty.PropertyType;
        ParameterExpression parameter = Expression.Parameter(typeof(TSource), "x");
        MemberExpression fieldAccess = Expression.Property(parameter, fieldProperty);
        LambdaExpression projectionLambda = Expression.Lambda(
            delegateType: typeof(Func<,>).MakeGenericType(typeof(TSource), fieldType),
            body: fieldAccess,
            parameters: parameter);
        MethodInfo selectMethod = typeof(Queryable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == nameof(Queryable.Select) // 方法名是Select
                        && m.GetParameters().Length == 2) // 排除带IComparer的重载
            .Single()
            .MakeGenericMethod(typeof(TSource), fieldType);
        object projectedQuery = selectMethod.Invoke(null, [source, projectionLambda])!;
        return (IQueryable)projectedQuery;
    }

    /// <summary>
    /// 动态投影：通过字段名从IQueryable<TSource>中筛选指定属性，返回IQueryable<Val>
    /// </summary>
    /// <typeparam name="TSource">源集合元素类型</typeparam>
    /// <typeparam name="Val">投影后元素类型（需与筛选字段类型一致）</typeparam>
    /// <param name="source">源IQueryable<TSource></param>
    /// <param name="filterField">需投影的属性名（必须是T的公共实例属性）</param>
    /// <returns>投影后的IQueryable<Val></returns>
    /// <exception cref="ArgumentNullException">source或filterField为null</exception>
    /// <exception cref="ArgumentException">filterField不是T的有效属性，或属性类型与Val不匹配</exception>
    public static IQueryable<Val> Select<TSource, Val>(this IQueryable<TSource> source, string filterField)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source), "源IQueryable<TSource>不能为null");
        if (string.IsNullOrWhiteSpace(filterField))
            throw new ArgumentException("投影字段名不能为空或空白", nameof(filterField));


        PropertyInfo propertyInfo = typeof(TSource).GetProperty(
            name: filterField,
            bindingAttr: BindingFlags.Public | BindingFlags.Instance)!;

        if (propertyInfo == null)
        {
            throw new ArgumentException(
                message: $"类型 {typeof(TSource).FullName} 不存在公共实例属性 {filterField}（请检查字段名拼写和大小写）",
                paramName: nameof(filterField));
        }

        Type propertyType = propertyInfo.PropertyType;
        Type targetType = typeof(Val);

        if (propertyType != targetType && !propertyType.IsImplicitlyConvertibleTo(targetType))
        {
            throw new ArgumentException(
                message: $"投影字段 {filterField} 的类型为 {propertyType.FullName}，无法隐式转换为目标类型 {targetType.FullName}（请确保Val类型与字段类型一致）",
                paramName: nameof(filterField));
        }

        ParameterExpression parameter = Expression.Parameter(typeof(TSource), "x");
        MemberExpression propertyAccess = Expression.Property(parameter, propertyInfo);
        Expression body = propertyType == targetType
            ? propertyAccess
            : Expression.Convert(propertyAccess, targetType); // 显式转换（确保类型安全）
        Expression<Func<TSource, Val>> lambda = Expression.Lambda<Func<TSource, Val>>(body, parameter);
        return source.Select(lambda);
    }

    /// <summary>
    /// 泛型版本：判断IQueryable<TSource>中是否存在满足"指定字段 == 值"的元素
    /// </summary>
    /// <typeparam name="TSource">数据源元素类型（编译时确定，无需运行时反射获取）</typeparam>
    /// <param name="source">泛型数据源IQueryable<TSource></param>
    /// <param name="filterField">筛选字段名（必须是T的公共实例属性）</param>
    /// <param name="val">筛选值（类型需与筛选字段类型匹配或可安全转换）</param>
    /// <returns>存在匹配元素返回true，否则返回false</returns>
    /// <exception cref="ArgumentNullException">source或filterField为null</exception>
    /// <exception cref="ArgumentException">filterField无效、类型不兼容或值无法转换</exception>
    public static bool Any<TSource>(this IQueryable<TSource> source, string filterField, object val)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source), "源IQueryable<TSource>不能为null");
        if (string.IsNullOrWhiteSpace(filterField))
            throw new ArgumentException("筛选字段名不能为空或空白", nameof(filterField));

        PropertyInfo propertyInfo = typeof(TSource).GetProperty(
            name: filterField,
            bindingAttr: BindingFlags.Public | BindingFlags.Instance)!;

        if (propertyInfo == null)
        {
            throw new ArgumentException(
                message: $"泛型类型 {typeof(TSource).FullName} 不存在公共实例属性 {filterField}（请检查字段名拼写/大小写，或是否为静态/私有字段）",
                paramName: nameof(filterField));
        }
        Type fieldType = propertyInfo.PropertyType;
        object convertedVal = val;
        if (val != null)
        {
            Type valType = val.GetType();

            if (valType != fieldType)
            {
                try
                {
                    Type? underlyingNullableType = Nullable.GetUnderlyingType(fieldType);
                    if (underlyingNullableType != null)
                    {
                        convertedVal = Convert.ChangeType(val, underlyingNullableType);
                    }
                    else if (fieldType.IsEnum)
                    {
                        convertedVal = Enum.Parse(fieldType, val.ToString() ?? string.Empty);
                    }
                    else
                    {
                        convertedVal = Convert.ChangeType(val, fieldType);
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(
                        message: $"筛选值类型 {valType.FullName}（值：{val}）无法转换为字段 {filterField} 的类型 {fieldType.FullName}（错误原因：{ex.Message}）",
                        paramName: nameof(val),
                        innerException: ex);
                }
            }
        }
        else
        {
            if (fieldType.IsValueType && Nullable.GetUnderlyingType(fieldType) != null)
            {
                throw new ArgumentException(
                    message: $"字段 {filterField} 的类型 {fieldType.FullName} 是不可为null的值类型，无法用null值筛选",
                    paramName: nameof(val));
            }
        }
        ParameterExpression parameter = Expression.Parameter(typeof(TSource), "x");
        MemberExpression propertyAccess = Expression.Property(parameter, propertyInfo);
        ConstantExpression constant = Expression.Constant(convertedVal, fieldType);
        BinaryExpression equalExpression = Expression.Equal(propertyAccess, constant);
        Expression<Func<TSource, bool>> filterLambda = Expression.Lambda<Func<TSource, bool>>(equalExpression, parameter);
        return source.Any(filterLambda);
    }

    /// <summary>
    /// 泛型版本：判断IQueryable<TSource>中是否存在满足"指定字段 != 值"的元素
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="source"></param>
    /// <param name="filterField"></param>
    /// <param name="val"></param>
    /// <returns></returns>
    public static bool NotAny<TSource>(this IQueryable<TSource> source, string filterField, object val)
    {
        return !source.Any(filterField, val);
    }

    /// <summary>
    /// linq分页
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="source"></param>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public static PagedList<TSource> ToPagedList<TSource>(this IQueryable<TSource> source, int page, int pageSize)
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