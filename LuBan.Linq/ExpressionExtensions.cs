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
* 描述：IEnumerable 拓展
*
*=================================================
*修改标记
* 修改时间：2023 / 12 / 4 18:21:32
* 修改人： yswenli
* 版本号： V1.0.0.0
*描述：IEnumerable 拓展
*
*****************************************************************************/

namespace System.Linq.Dynamic;


/// <summary>
/// 表达式工具类
/// </summary>
public static class ExpressionExtensions
{
    /// <summary>
    /// 获取成员名称
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    public static string GetMemberName(this Expression expression)
    {
        if (expression is UnaryExpression unaryExpression)
        {
            return unaryExpression.Operand.GetMemberName();
        }
        var me = expression as MemberExpression;
        if (me == null || me.Member == null) throw new Exception("表达式类型不正确");
        return me.Member.Name;
    }

    /// <summary>
    /// 获取值数组
    /// </summary>
    /// <param name="ne"></param>
    /// <param name="dataDic"></param>
    /// <returns></returns>
    public static object[] GetValues(this NewExpression ne, Dictionary<string, dynamic> dataDic)
    {
        List<object> result = new List<object>();
        foreach (var item in ne.Arguments)
        {
            if (item is MemberExpression me)
            {
                var val = GetValue(me, dataDic);
                if (val != null)
                    result.Add(val);
            }
        }
        return result.ToArray();
    }

    /// <summary>
    /// 获取值
    /// </summary>
    /// <param name="me"></param>
    /// <param name="dataDic"></param>
    /// <returns></returns>
    public static object? GetValue(this MemberExpression me, Dictionary<string, dynamic> dataDic)
    {
        var mn = me.Member.Name;
        if (dataDic.TryGetValue(mn, out var value))
        {
            return new { value };
        }
        return null;
    }


    /// <summary>
    /// 获取过滤表达式
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <param name="owners"></param>
    /// <returns></returns>
    public static LambdaExpression GetConditionExpression<T>(this Type type, List<long> owners) where T : Attribute
    {
        var fieldNames = type.GetPropertyNames<T>();
        ParameterExpression parameter = Expression.Parameter(type, "c");
        Expression right = Expression.Constant(false);
        fieldNames.ForEach(fieldName =>
        {
            owners.ForEach(owner =>
            {
                var property = type.GetProperty(fieldName);
                if (property == null)
                {
                    throw new Exception($"{type.Name}中不存在{fieldName}属性");
                }

                Expression temp = Expression.Property(parameter, property);

                // 如果属性是可为空的类型，则转换为其基础类型
                var propertyType = property.PropertyType;
                if (propertyType != null)
                {
                    var np = Nullable.GetUnderlyingType(propertyType);
                    if (np != null)
                    {
                        temp = Expression.Convert(temp, np);
                    }
                }
                Expression left = Expression.Equal(temp, Expression.Constant(owner));
                right = Expression.OrElse(left, right);
            });
        });
        return Expression.Lambda(right, [parameter]);
    }



    /// <summary>
    /// 组合两个表达式
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <param name="expression">表达式1</param>
    /// <param name="extendExpression">表达式2</param>
    /// <param name="mergeWay">组合方式</param>
    /// <returns>新的表达式</returns>
    public static Expression<TSource> Compose<TSource>(this Expression<TSource> expression, Expression<TSource> extendExpression, Func<Expression, Expression, Expression> mergeWay)
    {
        var parameterExpressionSetter = expression.Parameters
            .Select((u, i) => new { u, Parameter = extendExpression.Parameters[i] })
            .ToDictionary(d => d.Parameter, d => d.u);

        var extendExpressionBody = ParameterReplaceExpressionVisitor.ReplaceParameters(parameterExpressionSetter, extendExpression.Body);
        return Expression.Lambda<TSource>(mergeWay(expression.Body, extendExpressionBody), expression.Parameters);
    }

    /// <summary>
    /// 与操作合并两个表达式
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <param name="expression">表达式1</param>
    /// <param name="extendExpression">表达式2</param>
    /// <returns>新的表达式</returns>
    public static Expression<Func<TSource, bool>> And<TSource>(this Expression<Func<TSource, bool>> expression, Expression<Func<TSource, bool>> extendExpression)
    {
        return expression.Compose(extendExpression, Expression.AndAlso);
    }

    /// <summary>
    /// 与操作合并两个表达式，支持索引器
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <param name="expression">表达式1</param>
    /// <param name="extendExpression">表达式2</param>
    /// <returns>新的表达式</returns>
    public static Expression<Func<TSource, int, bool>> And<TSource>(this Expression<Func<TSource, int, bool>> expression, Expression<Func<TSource, int, bool>> extendExpression)
    {
        return expression.Compose(extendExpression, Expression.AndAlso);
    }

    /// <summary>
    /// 根据条件成立再与操作合并两个表达式
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <param name="expression">表达式1</param>
    /// <param name="condition">布尔条件</param>
    /// <param name="extendExpression">表达式2</param>
    /// <returns>新的表达式</returns>
    public static Expression<Func<TSource, bool>> AndIf<TSource>(this Expression<Func<TSource, bool>> expression, bool condition, Expression<Func<TSource, bool>> extendExpression)
    {
        return condition ? expression.Compose(extendExpression, Expression.AndAlso) : expression;
    }

    /// <summary>
    /// 根据条件成立再与操作合并两个表达式，支持索引器
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <param name="expression">表达式1</param>
    /// <param name="condition">布尔条件</param>
    /// <param name="extendExpression">表达式2</param>
    /// <returns>新的表达式</returns>
    public static Expression<Func<TSource, int, bool>> AndIf<TSource>(this Expression<Func<TSource, int, bool>> expression, bool condition, Expression<Func<TSource, int, bool>> extendExpression)
    {
        return condition ? expression.Compose(extendExpression, Expression.AndAlso) : expression;
    }

    /// <summary>
    /// 或操作合并两个表达式
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <param name="expression">表达式1</param>
    /// <param name="extendExpression">表达式2</param>
    /// <returns>新的表达式</returns>
    public static Expression<Func<TSource, bool>> Or<TSource>(this Expression<Func<TSource, bool>> expression, Expression<Func<TSource, bool>> extendExpression)
    {
        return expression.Compose(extendExpression, Expression.OrElse);
    }

    /// <summary>
    /// 或操作合并两个表达式，支持索引器
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <param name="expression">表达式1</param>
    /// <param name="extendExpression">表达式2</param>
    /// <returns>新的表达式</returns>
    public static Expression<Func<TSource, int, bool>> Or<TSource>(this Expression<Func<TSource, int, bool>> expression, Expression<Func<TSource, int, bool>> extendExpression)
    {
        return expression.Compose(extendExpression, Expression.OrElse);
    }

    /// <summary>
    /// 根据条件成立再或操作合并两个表达式
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <param name="expression">表达式1</param>
    /// <param name="condition">布尔条件</param>
    /// <param name="extendExpression">表达式2</param>
    /// <returns>新的表达式</returns>
    public static Expression<Func<TSource, bool>> OrIf<TSource>(this Expression<Func<TSource, bool>> expression, bool condition, Expression<Func<TSource, bool>> extendExpression)
    {
        return condition ? expression.Compose(extendExpression, Expression.OrElse) : expression;
    }

    /// <summary>
    /// 根据条件成立再或操作合并两个表达式，支持索引器
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <param name="expression">表达式1</param>
    /// <param name="condition">布尔条件</param>
    /// <param name="extendExpression">表达式2</param>
    /// <returns>新的表达式</returns>
    public static Expression<Func<TSource, int, bool>> OrIf<TSource>(this Expression<Func<TSource, int, bool>> expression, bool condition, Expression<Func<TSource, int, bool>> extendExpression)
    {
        return condition ? expression.Compose(extendExpression, Expression.OrElse) : expression;
    }

    /// <summary>
    /// 获取Lambda表达式属性名，只限 u=>u.Property 表达式
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <param name="expression">表达式</param>
    /// <returns>属性名</returns>
    public static string GetExpressionPropertyName<TSource>(this Expression<Func<TSource, object>> expression)
    {
        if (expression.Body is UnaryExpression unaryExpression)
        {
            return ((MemberExpression)unaryExpression.Operand).Member.Name;
        }
        else if (expression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }
        else if (expression.Body is ParameterExpression parameterExpression)
        {
            return parameterExpression.Type.Name;
        }

        throw new InvalidCastException(nameof(expression));
    }

    /// <summary>
    /// 是否是空集合
    /// </summary>
    /// <typeparam name="TSource">泛型类型</typeparam>
    /// <param name="sources">集合对象</param>
    /// <returns>是否为空集合</returns>
    public static bool IsNullOrEmpty<TSource>([NotNullWhen(false)] this IEnumerable<TSource> sources)
    {
        return sources == null || !sources.Any();
    }

    /// <summary>
    /// 获取动态参数对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="model"></param>
    /// <param name="expression"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static object? ToDynamic<T>(this T model, Expression<Func<T, object>> expression) where T : class, new()
    {
        if (model == null) throw new Exception("传入参数model不能为空");
        var dic = model.ToDictionary();
        if (dic == null || dic.Count < 1) throw new Exception("泛型类中不存在任何属性");
        if (expression.Body is MemberExpression me)
        {
            var member = GetValue(me, dic);
            return new { member };
        }
        else if (expression.Body is NewExpression ne)
        {
            var members = GetValues(ne, dic);
            return new { members };
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 构建强类型过滤表达式：element => element.Property == filterValue
    /// </summary>
    /// <param name="elementType"></param>
    /// <param name="property"></param>
    /// <param name="filterValue"></param>
    /// <returns></returns>
    public static Expression<Func<object, bool>> BuildFilterExpression(this Type elementType, PropertyInfo property, object filterValue)
    {
        ParameterExpression paramExpr = Expression.Parameter(elementType, "element");
        MemberExpression propertyExpr = Expression.Property(paramExpr, property);
        ConstantExpression constantExpr = Expression.Constant(filterValue, property.PropertyType);
        BinaryExpression equalExpr = Expression.Equal(propertyExpr, constantExpr);
        LambdaExpression lambdaExpr = Expression.Lambda(equalExpr, paramExpr);
        return (Expression<Func<object, bool>>)lambdaExpr;
    }

    /// <summary>
    /// 将集合转换为动态列表
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static List<object> ToDynamicList(this IEnumerable source)
    {
        return source.AsQueryable().ToDynamicList();
    }
}

