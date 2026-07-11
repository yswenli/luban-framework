namespace LuBan.Linq.Core;
/// <summary>
/// 隐式转换帮助类
/// </summary>
internal static class ImplicitlyConvert
{
    /// <summary>
    /// 扩展方法：判断当前类型是否可隐式转换为目标类型
    /// </summary>
    public static bool IsImplicitlyConvertibleTo(this Type sourceType, Type targetType)
    {
        // 1. 若目标类型是object → 所有类型都可隐式转换
        if (targetType == typeof(object))
            return true;

        // 2. 检查内置隐式转换（如int→long、double→decimal）
        if (sourceType.IsImplicitlyConvertibleToBuiltIn(targetType))
            return true;

        // 3. 检查用户定义的隐式转换运算符（如自定义类的implicit operator）
        return sourceType.HasImplicitConversionOperatorTo(targetType);
    }

    /// <summary>
    /// 检查内置类型的隐式转换（基于.NET框架内置规则）
    /// </summary>
    public static bool IsImplicitlyConvertibleToBuiltIn(this Type sourceType, Type targetType)
    {
        // 排除引用类型（除string外）和 nullable类型的特殊处理
        if (!sourceType.IsValueType || !targetType.IsValueType)
            return false;

        // 内置值类型隐式转换规则（示例核心规则，可根据需求扩展）
        return (sourceType == typeof(sbyte) && (targetType == typeof(short) || targetType == typeof(int) || targetType == typeof(long) || targetType == typeof(float) || targetType == typeof(double) || targetType == typeof(decimal)))
            || (sourceType == typeof(byte) && (targetType == typeof(short) || targetType == typeof(ushort) || targetType == typeof(int) || targetType == typeof(uint) || targetType == typeof(long) || targetType == typeof(ulong) || targetType == typeof(float) || targetType == typeof(double) || targetType == typeof(decimal)))
            || (sourceType == typeof(short) && (targetType == typeof(int) || targetType == typeof(long) || targetType == typeof(float) || targetType == typeof(double) || targetType == typeof(decimal)))
            || (sourceType == typeof(ushort) && (targetType == typeof(int) || targetType == typeof(uint) || targetType == typeof(long) || targetType == typeof(ulong) || targetType == typeof(float) || targetType == typeof(double) || targetType == typeof(decimal)))
            || (sourceType == typeof(int) && (targetType == typeof(long) || targetType == typeof(float) || targetType == typeof(double) || targetType == typeof(decimal)))
            || (sourceType == typeof(uint) && (targetType == typeof(long) || targetType == typeof(ulong) || targetType == typeof(float) || targetType == typeof(double) || targetType == typeof(decimal)))
            || (sourceType == typeof(long) && (targetType == typeof(float) || targetType == typeof(double) || targetType == typeof(decimal)))
            || (sourceType == typeof(char) && (targetType == typeof(ushort) || targetType == typeof(int) || targetType == typeof(uint) || targetType == typeof(long) || targetType == typeof(ulong) || targetType == typeof(float) || targetType == typeof(double) || targetType == typeof(decimal)))
            || (sourceType == typeof(float) && (targetType == typeof(double) || targetType == typeof(decimal)))
            || (sourceType == typeof(double) && targetType == typeof(decimal));
    }

    /// <summary>
    /// 检查是否存在用户定义的隐式转换运算符（sourceType → targetType）
    /// </summary>
    public static bool HasImplicitConversionOperatorTo(this Type sourceType, Type targetType)
    {
        // 获取sourceType中所有静态方法，筛选“隐式转换运算符”
        var conversionMethods = sourceType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == "op_Implicit" // 隐式转换运算符的方法名
                        && m.ReturnType == targetType // 返回类型是目标类型
                        && m.GetParameters().Length == 1 // 只有一个参数
                        && m.GetParameters()[0].ParameterType == sourceType); // 参数类型是源类型

        // 若存在匹配的转换方法 → 支持用户定义的隐式转换
        return conversionMethods.Any();
    }


    /// <summary>
    /// 将筛选值转换为字段的类型（如 string → int，兼容可空类型）
    /// </summary>
    public static object ConvertValueToPropertyType(object filterValue, PropertyInfo property)
    {
        Type propertyType = property.PropertyType;
        Type underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType; // 处理可空类型（如 int? → int）

        try
        {
            // 转换值类型（支持基础类型转换，如 string → int、int → long）
            return Convert.ChangeType(filterValue, underlyingType);
        }
        catch (Exception ex)
        {
            throw new InvalidCastException(
                $"筛选值 {filterValue}（类型：{filterValue.GetType().Name}）无法转换为字段 {property.Name} 的类型（{propertyType.Name}）",
                ex);
        }
    }

    /// <summary>
    /// 从 IQueryable 中提取元素类型（如 IQueryable<User> → User）
    /// </summary>
    /// <param name="queryable"></param>
    /// <returns></returns>
    public static Type? GetIQueryableElementType(this IQueryable queryable)
    {
        // 遍历类型层级，找到泛型 IQueryable<T> 接口
        Type? currentType = queryable.GetType();
        while (currentType != null)
        {
            // 检查是否实现了 IQueryable<T>
            if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == typeof(IQueryable<>))
            {
                return currentType.GetGenericArguments()[0]; // 返回泛型参数（元素类型）
            }
            currentType = currentType?.BaseType ?? null;
        }
        return null;
    }
}