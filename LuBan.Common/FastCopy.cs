/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： FastCopy
*版本号： V1.0.0.0
*唯一标识：6166d746-a96d-47bb-9f01-5e4b904b8a9e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/3/15 17:35:05
*描述：泛型对象快速属性复制工具。
*
*=================================================
*修改标记
*修改时间：2023/3/15 17:35:05
*修改人： yswenli
*版本号： V1.0.0.0
*描述：泛型对象快速属性复制工具。
*
*****************************************************************************/


namespace LuBan.Common;


/// <summary>
/// 快速复制
/// </summary>
/// <typeparam name="TIn"></typeparam>
/// <typeparam name="TOut"></typeparam>
[RequiresUnreferencedCode("FastCopy uses Expression trees with reflection to copy properties. TIn and TOut must be rooted.")]
[UnconditionalSuppressMessage("ReflectionAnalysis", "IL2060:MakeGenericMethod",
    Justification = "FastCopy 内部使用 Expression trees，由 [RequiresUnreferencedCode] 类级注解覆盖。")]
/// <summary>
/// 泛型属性复制器：首次使用时用表达式树编译并缓存委托，
/// 把 <c>TIn</c> 与 <c>TOut</c> 同名的可写属性复制到新建的 <c>TOut</c> 上。
/// </summary>
public static class FastCopy<TIn, TOut>
{
    private static readonly Func<TIn, TOut> cache = GetFunc();
    private static Func<TIn, TOut> GetFunc()
    {
        var type = typeof(TIn);
        if (type == null) throw new Exception("type is null");
        ParameterExpression parameterExpression = Expression.Parameter(type, "p");
        List<MemberBinding> memberBindingList = [];

        foreach (var item in typeof(TOut).GetProperties())
        {
            if (!item.CanWrite)
                continue;

            var propertiy = type.GetProperty(item.Name);
            if (propertiy != null)
            {
                MemberExpression property = Expression.Property(parameterExpression, propertiy);
                MemberBinding memberBinding = Expression.Bind(item, property);
                memberBindingList.Add(memberBinding);
            }
        }

        MemberInitExpression memberInitExpression = Expression.MemberInit(Expression.New(typeof(TOut)), [.. memberBindingList]);
        Expression<Func<TIn, TOut>> lambda = Expression.Lambda<Func<TIn, TOut>>(memberInitExpression, [parameterExpression]);
        return lambda.Compile();
    }

    /// <summary>
    /// 复制
    /// </summary>
    /// <param name="tIn"></param>
    /// <returns></returns>
    public static TOut Copy(TIn tIn)
    {
        return cache(tIn);
    }
}

