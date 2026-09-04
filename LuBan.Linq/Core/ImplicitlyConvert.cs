/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Linq.Core
*文件名： ImplicitlyConvert.cs
*版本号： V1.0.0.0
*唯一标识：c5d15f5b-5616-4424-8703-964f28b9f56a
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：ImplicitlyConvert 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ImplicitlyConvert 类
*
*****************************************************************************/

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
    [UnconditionalSuppressMessage("System.Diagnostics.CodeAnalysis", "IL2070", Justification = "Reflection-based code; library is not trim/AOT targeted.")]
    public static bool HasImplicitConversionOperatorTo(this Type sourceType, Type targetType)
    {
        var sourceConversionMethods = sourceType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == "op_Implicit"
                        && m.ReturnType == targetType
                        && m.GetParameters().Length == 1
                        && m.GetParameters()[0].ParameterType == sourceType);

        if (sourceConversionMethods.Any())
            return true;

        var targetConversionMethods = targetType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == "op_Implicit"
                        && m.ReturnType == targetType
                        && m.GetParameters().Length == 1
                        && m.GetParameters()[0].ParameterType == sourceType);

        return targetConversionMethods.Any();
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
            if (underlyingType.IsEnum)
            {
                return Enum.Parse(underlyingType, filterValue.ToString() ?? string.Empty);
            }
            
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
        return queryable.ElementType;
    }
}