/*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：System.Linq.Dynamic
*文件名： IQueryableExtensions
*版本号： V1.0.0.0
*唯一标识：3a0499e4-68ef-4244-8707-17ecc8bef9df
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 18:21:32
* 描述：IQueryable 拓展
*
*=================================================
*修改标记
* 修改时间：2023 / 12 / 4 18:21:32
* 修改人： yswenli
* 版本号： V1.0.0.0
*描述：IQueryable 拓展
*
*****************************************************************************/
namespace System.Linq.Dynamic;

/// <summary>
/// 描述：IQueryable 拓展
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// 为 IQueryable 提供通用 Where 筛选（按指定字段名和值进行相等匹配）
    /// </summary>
    /// <param name="queryable">源 IQueryable 集合</param>
    /// <param name="filterField">筛选字段名（必须是元素类型的公共属性）</param>
    /// <param name="filterValue">筛选值（类型需与字段类型兼容）</param>
    /// <returns>筛选后的 IQueryable 集合</returns>
    /// <exception cref="ArgumentNullException">queryable 或 filterField 为 null</exception>
    /// <exception cref="ArgumentException">filterField 为空字符串，或不是元素类型的公共属性</exception>
    /// <exception cref="InvalidCastException">filterValue 类型与字段类型不兼容</exception>
    public static IQueryable Where(this IQueryable queryable, string filterField, object filterValue)
    {
        if (queryable == null)
            throw new ArgumentNullException(nameof(queryable), "源 IQueryable 集合不能为 null");
        if (string.IsNullOrWhiteSpace(filterField))
            throw new ArgumentException("筛选字段名不能为 null 或空字符串", nameof(filterField));

        Type? elementType = queryable.ElementType;
        if (elementType == null)
            throw new InvalidOperationException("无法解析 IQueryable 的元素类型（非泛型 IQueryable 不支持）");
        PropertyInfo? property = elementType.GetProperty(
            name: filterField,
            bindingAttr: BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (property == null)
            throw new ArgumentException($"类型 {elementType.Name} 中不存在公共属性 {filterField}", nameof(filterField));
        if (filterValue == null)
        {
            ReflectionUtil.ValidateNullComparison(property);
        }
        else
        {
            filterValue = ImplicitlyConvert.ConvertValueToPropertyType(filterValue, property);
        }
        LambdaExpression filterExpr = ExpressionExtensions.BuildFilterExpression(elementType, property, filterValue!);

        MethodInfo whereMethod = typeof(Queryable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == nameof(Queryable.Where) && m.GetParameters().Length == 2)
            .Single()
            .MakeGenericMethod(elementType);

        return (IQueryable)whereMethod.Invoke(obj: null, parameters: [queryable, filterExpr])!;
    }

    /// <summary>
    /// 为 IQueryable 提供通用 OrderBy 排序（按指定字段名和排序方向）
    /// </summary>
    /// <param name="queryable">源 IQueryable 集合</param>
    /// <param name="filterField">排序字段名（必须是元素类型的公共属性）</param>
    /// <param name="isAscending">是否升序排序（默认true）</param>
    /// <returns>排序后的 IQueryable 集合</returns>
    /// <exception cref="ArgumentNullException">queryable 或 filterField 为 null</exception>
    /// <exception cref="ArgumentException">filterField 为空字符串，或不是元素类型的公共属性</exception>
    public static IQueryable OrderBy(this IQueryable queryable, string filterField, bool isAscending = true)
    {
        if (queryable == null)
            throw new ArgumentNullException(nameof(queryable), "源 IQueryable 集合不能为 null");
        if (string.IsNullOrWhiteSpace(filterField))
            throw new ArgumentException("排序字段名不能为 null 或空字符串", nameof(filterField));

        Type elementType = queryable.ElementType ??
            throw new InvalidOperationException("无法解析 IQueryable 的元素类型，请确保 source 是有效的查询集合");

        PropertyInfo? property = elementType.GetProperty(
            name: filterField,
            bindingAttr: BindingFlags.Public | BindingFlags.Instance);

        if (property == null)
        {
            throw new ArgumentException(
                message: $"元素类型 {elementType.FullName} 不存在公共实例属性 {filterField}（请检查字段名拼写和大小写）",
                paramName: nameof(filterField));
        }

        ParameterExpression parameter = Expression.Parameter(elementType, "x");
        MemberExpression propertyAccess = Expression.Property(parameter, property);
        Type lambdaDelegateType = typeof(Func<,>).MakeGenericType(elementType, property.PropertyType);
        LambdaExpression sortLambda = Expression.Lambda(
            delegateType: lambdaDelegateType,
            body: propertyAccess,
            parameters: parameter);

        string methodName = isAscending ? "OrderBy" : "OrderByDescending";

        try
        {
            MethodInfo orderMethod = typeof(Queryable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == methodName
                            && m.GetParameters().Length == 2)
                .Single()
                .MakeGenericMethod(elementType, property.PropertyType);

            object result = orderMethod.Invoke(null, [queryable, sortLambda])!;
            return (IQueryable)result;
        }
        catch (TargetInvocationException ex)
        {
            throw new InvalidOperationException(
                message: $"调用{methodName}方法时执行查询失败（可能是数据源连接异常或表达式不支持）",
                innerException: ex.InnerException);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                message: $"动态构建{methodName}查询失败（元素类型：{elementType.FullName}）",
                innerException: ex);
        }
    }

    /// <summary>
    /// 非泛型版本：将任意IQueryable动态投影为仅包含指定filterField的IQueryable
    /// </summary>
    /// <param name="source">非泛型数据源IQueryable（元素类型运行时确定）</param>
    /// <param name="filterField">需投影的字段名（必须是数据源元素类型的公共实例属性）</param>
    /// <returns>投影后的非泛型IQueryable（元素为filterField的字段值，类型与字段一致）</returns>
    /// <exception cref="ArgumentNullException">source或filterField为null</exception>
    /// <exception cref="ArgumentException">filterField无效（不存在/非公共实例属性）</exception>
    /// <exception cref="InvalidOperationException">无法解析数据源元素类型</exception>
    public static IQueryable Select(this IQueryable source, string filterField)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source), "源IQueryable不能为null");
        if (string.IsNullOrWhiteSpace(filterField))
            throw new ArgumentException("投影字段名不能为空或空白", nameof(filterField));

        Type elementType = source.ElementType ??
            throw new InvalidOperationException("无法解析数据源的元素类型，请确保source是有效的查询集合（非空且包含元素类型信息）");

        PropertyInfo? fieldProperty = elementType.GetProperty(
            name: filterField,
            bindingAttr: BindingFlags.Public | BindingFlags.Instance); // 仅支持公共实例属性（排除静态/私有/字段）

        if (fieldProperty == null)
        {
            throw new ArgumentException(
                message: $"数据源元素类型 {elementType.FullName} 不存在公共实例属性 {filterField}（请检查：1. 字段名拼写/大小写；2. 是否为静态属性；3. 是否为私有属性/字段）",
                paramName: nameof(filterField));
        }
        Type fieldType = fieldProperty.PropertyType;
        ParameterExpression parameter = Expression.Parameter(elementType, "x");
        MemberExpression fieldAccess = Expression.Property(parameter, fieldProperty);
        // 因elementType和fieldType均为运行时确定，需用MakeGenericType创建委托类型
        Type lambdaDelegateType = typeof(Func<,>).MakeGenericType(elementType, fieldType);
        LambdaExpression projectionLambda = Expression.Lambda(
            delegateType: lambdaDelegateType,
            body: fieldAccess,
            parameters: parameter);

        MethodInfo selectMethod = typeof(Queryable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == nameof(Queryable.Select) // 方法名是Select
                        && m.GetParameters().Length == 2) // 排除带IComparer的重载
            .Single()
            .MakeGenericMethod(elementType, fieldType);

        object projectedQuery = selectMethod.Invoke(null, [source, projectionLambda])!;

        return (IQueryable)projectedQuery!;
    }

    /// <summary>
    /// 判断IQueryable是否包含任意元素（即是否非空）
    /// </summary>
    /// <param name="source">非泛型数据源IQueryable（元素类型运行时确定）</param>
    /// <returns>包含至少一个元素返回true，否则返回false</returns>
    /// <exception cref="ArgumentNullException">source为null</exception>
    /// <exception cref="InvalidOperationException">无法解析元素类型或调用泛型Any方法失败</exception>
    public static bool Any(this IQueryable source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source), "源IQueryable不能为null（无法判断空集合）");

        // 关键：通过IQueryable.ElementType获取元素类型（如IQueryable<User>的ElementType是User）
        Type elementType = source.ElementType ??
            throw new InvalidOperationException("无法解析数据源的元素类型，请确保source是有效的查询集合（如非空的IQueryable<User>，而非空IQueryable）");
        try
        {
            MethodInfo anyMethod = typeof(Queryable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == nameof(Queryable.Any) // 方法名是Any
                            && m.GetParameters().Length == 1) // 无参筛选（仅接收source）
                .Single() // 确保只匹配一个重载（排除带predicate的重载）
                .MakeGenericMethod(elementType); // 填充泛型参数TSource=元素类型（如User）

            object result = anyMethod.Invoke(null, [source])!;
            return (bool)result;
        }
        catch (TargetInvocationException ex)
        {
            throw new InvalidOperationException(
                message: $"调用Any方法时执行查询失败（可能是数据源连接异常或表达式不支持）",
                innerException: ex.InnerException);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                message: $"动态构建Any查询失败（元素类型：{elementType.FullName}）",
                innerException: ex);
        }
    }



    /// <summary>
    /// 动态判断IQueryable中是否存在满足“指定字段 == 值”的元素
    /// </summary>
    /// <param name="source">源IQueryable（非泛型，元素类型运行时确定）</param>
    /// <param name="filterField">筛选字段名（必须是源元素类型的公共实例属性）</param>
    /// <param name="val">筛选值（类型需与筛选字段类型匹配或可转换）</param>
    /// <returns>存在匹配元素返回true，否则返回false</returns>
    /// <exception cref="ArgumentNullException">source、filterField为null</exception>
    /// <exception cref="ArgumentException">filterField无效，或字段类型与val类型不兼容</exception>
    /// <exception cref="InvalidOperationException">无法调用Queryable.Any方法</exception>
    public static bool Any(this IQueryable source, string filterField, object val)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source), "源IQueryable不能为null");
        if (string.IsNullOrWhiteSpace(filterField))
            throw new ArgumentException("筛选字段名不能为空或空白", nameof(filterField));

        Type elementType = source.ElementType ??
            throw new InvalidOperationException("无法获取源IQueryable的元素类型，请确保source是有效的查询集合");

        PropertyInfo? propertyInfo = elementType.GetProperty(
            name: filterField,
            bindingAttr: BindingFlags.Public | BindingFlags.Instance); // 仅支持公共实例属性

        if (propertyInfo == null)
        {
            throw new ArgumentException(
                message: $"元素类型 {elementType.FullName} 不存在公共实例属性 {filterField}（请检查字段名拼写和大小写）",
                paramName: nameof(filterField));
        }
        Type fieldType = propertyInfo.PropertyType;

        object convertedVal = val;
        if (val != null)
        {
            if (val.GetType() != fieldType)
            {
                try
                {
                    Type? underlyingType = Nullable.GetUnderlyingType(fieldType);
                    Type targetType = underlyingType ?? fieldType;
                    
                    if (targetType.IsEnum)
                    {
                        convertedVal = Enum.Parse(targetType, val.ToString() ?? string.Empty);
                    }
                    else
                    {
                        convertedVal = Convert.ChangeType(val, targetType);
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(
                        message: $"筛选值类型 {val.GetType().FullName} 无法转换为字段 {filterField} 的类型 {fieldType.FullName}（错误：{ex.Message}）",
                        paramName: nameof(val),
                        innerException: ex);
                }
            }
        }
        else
        {
            if (fieldType.IsValueType && Nullable.GetUnderlyingType(fieldType) == null)
            {
                throw new ArgumentException(
                    message: $"字段 {filterField} 的类型 {fieldType.FullName} 是值类型且不可为null，无法用null值筛选",
                    paramName: nameof(val));
            }
        }

        ParameterExpression parameter = Expression.Parameter(elementType, "x");
        MemberExpression propertyAccess = Expression.Property(parameter, propertyInfo);
        ConstantExpression constant = Expression.Constant(convertedVal, fieldType);
        BinaryExpression equalExpression = Expression.Equal(propertyAccess, constant);
        Type lambdaType = typeof(Func<,>).MakeGenericType(elementType, typeof(bool));
        LambdaExpression lambda = Expression.Lambda(lambdaType, equalExpression, parameter);

        try
        {
            MethodInfo anyMethod = typeof(Queryable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == nameof(Queryable.Any)
                            && m.GetParameters().Length == 2)
                .Single()
                .MakeGenericMethod(elementType);

            object result = anyMethod.Invoke(null, [source, lambda])!;
            return (bool)result!;
        }
        catch (TargetInvocationException ex)
        {
            throw new InvalidOperationException(
                message: "调用Queryable.Any方法失败（可能是筛选表达式不支持ORM转换）",
                innerException: ex.InnerException);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                message: "动态构建Any查询失败",
                innerException: ex);
        }
    }

    /// <summary>
    /// 动态判断IQueryable中是否存在满足“指定字段 != 值”的元素
    /// </summary>
    /// <param name="source">源IQueryable（非泛型，元素类型运行时确定）</param>
    /// <param name="filterField">筛选字段名（必须是源元素类型的公共实例属性）</param>
    /// <param name="val">筛选值（类型需与筛选字段类型匹配或可转换）</param>
    /// <returns>存在匹配元素返回true，否则返回false</returns>
    /// <exception cref="ArgumentNullException">source、filterField为null</exception>
    /// <exception cref="ArgumentException">filterField无效，或字段类型与val类型不兼容</exception>
    /// <exception cref="InvalidOperationException">无法调用Queryable.Any方法</exception>
    public static bool NotAny(this IQueryable source, string filterField, object val)
    {
        return source.Any(filterField, val) == false;
    }




    /// <summary>
    /// 获取IQueryable的第一个元素，以dynamic类型返回（兼容任意元素类型）
    /// </summary>
    /// <param name="source">非泛型数据源IQueryable（元素类型运行时确定）</param>
    /// <returns>数据源的第一个元素（dynamic类型，可直接访问元素属性）</returns>
    /// <exception cref="ArgumentNullException">source为null</exception>
    /// <exception cref="InvalidOperationException">元素类型无法解析、数据源为空或调用First方法失败</exception>
    public static dynamic First(this IQueryable source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source), "源IQueryable不能为null（无法获取空数据源的第一个元素）");
        Type elementType = source.ElementType ??
            throw new InvalidOperationException("无法解析数据源的元素类型，请确保source是有效的查询集合（如非空的IQueryable<Student>，而非空IQueryable）");
        try
        {
            MethodInfo firstMethod = typeof(Queryable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == nameof(Queryable.First) // 方法名是First
                            && m.GetParameters().Length == 1) // 无筛选条件（仅接收source）
                .Single() // 排除带predicate的重载（如First<TSource>(source, predicate)）
                .MakeGenericMethod(elementType); // 填充泛型参数TSource=元素类型（如User）

            object firstElement = firstMethod.Invoke(null, [source])!;

            return firstElement;
        }
        catch (TargetInvocationException ex)
        {
            // 解包反射内部异常：原生First方法的异常（如数据源为空、SQL执行失败）需透传
            if (ex.InnerException is InvalidOperationException innerOpEx)
            {
                // 透传原生First的空集合异常（保持与静态First一致的错误信息）
                throw new InvalidOperationException(innerOpEx.Message, innerOpEx);
            }
            // 其他内部异常（如数据库连接失败、权限错误）
            throw new InvalidOperationException(
                message: $"获取第一个元素时执行查询失败（元素类型：{elementType.FullName}）",
                innerException: ex.InnerException);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                message: $"动态构建First查询失败（元素类型解析异常或方法调用错误）",
                innerException: ex);
        }
    }




    /// <summary>
    /// 非泛型版本：获取IQueryable的第一个元素，空集合返回null，以dynamic?类型返回（兼容值类型/引用类型）
    /// </summary>
    /// <param name="source">非泛型数据源IQueryable（元素类型运行时确定）</param>
    /// <returns>数据源第一个元素（dynamic?类型），空集合返回null</returns>
    /// <exception cref="ArgumentNullException">source为null</exception>
    /// <exception cref="InvalidOperationException">元素类型无法解析或调用FirstOrDefault方法失败</exception>
    public static dynamic? FirstOrDefault(this IQueryable source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source), "源IQueryable不能为null（无法获取空数据源的元素）");
        Type elementType = source.ElementType ??
            throw new InvalidOperationException("无法解析数据源的元素类型，请确保source是有效的查询集合（如非空的IQueryable<Student>，而非空IQueryable）");
        try
        {
            // 注：C# 8.0+ 中值类型TSource会自动转为可空类型（如int→int?），符合dynamic?的返回语义
            MethodInfo firstOrDefaultMethod = typeof(Queryable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == nameof(Queryable.FirstOrDefault) // 方法名是FirstOrDefault
                            && m.GetParameters().Length == 1) // 无筛选条件（仅接收source）
                .Single() // 排除带predicate的重载（如FirstOrDefault(source, predicate)）
                .MakeGenericMethod(elementType); // 填充泛型参数TSource=元素类型（如int、User）

            // 原生行为：非空集合返回第一个元素，空集合返回null（值类型会自动装箱为可空类型，如int→int?）
            object? result = firstOrDefaultMethod.Invoke(null, [source]);
            return result;
        }
        catch (TargetInvocationException ex)
        {
            // 解包反射内部异常：仅透传查询执行相关异常（如数据库连接失败、SQL语法错误）
            // 注意：原生FirstOrDefault不会因空集合抛异常，此处异常均为执行阶段错误
            throw new InvalidOperationException(
                message: $"获取第一个元素时执行查询失败（元素类型：{elementType.FullName}）",
                innerException: ex.InnerException);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                message: $"动态构建FirstOrDefault查询失败（元素类型解析错误或方法调用异常）",
                innerException: ex);
        }
    }

    /// <summary>
    /// 将 IQueryable 转换为可动态访问的对象列表（支持嵌套属性、枚举语义化、循环引用处理）
    /// </summary>
    /// <param name="source">源 IQueryable（延迟执行查询）</param>
    /// <returns>可动态访问的 List<object>（元素实际为 JObject，支持 dynamic 访问），查询为空时返回空列表（非 null）</returns>
    /// <exception cref="ArgumentNullException">source 为 null 时抛出</exception>
    /// <exception cref="InvalidOperationException">序列化/反序列化失败时抛出</exception>
    public static List<object> ToDynamicList(this IQueryable source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source), "源 IQueryable 不能为 null");
        try
        {
            var dataList = source.Cast<object>().ToList();
            string json = SerializeUtil.Serialize(dataList);
            var dynamicList = SerializeUtil.Deserialize<List<dynamic>>(json);
            return dynamicList?.Cast<object>().ToList() ?? [];
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"IQueryable 转换为动态列表失败（元素类型：{source.ElementType?.Name ?? "未知"}）",
                ex);
        }
    }

}

