/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Linq.Core
*文件名： ElementTypeHandler.cs
*版本号： V1.0.0.0
*唯一标识：cd921fb5-17f7-4916-96ea-57c16f64dba2
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：ElementTypeHandler 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ElementTypeHandler 类
*
*****************************************************************************/

namespace LuBan.Linq.Core;

internal class ElementTypeHandler
{
    /// <summary>
    /// 从非泛型IEnumerable中提取元素类型（优先级：泛型参数 > 非泛型集合的元素实际类型 > 空集合默认object）
    /// </summary>
    public static Type GetElementType(IEnumerable source, ICollection collection)
    {
        // 情况1：集合是泛型类型（如List<int>、HashSet<User>）→ 提取泛型参数（如int、User）
        Type sourceType = source.GetType();
        if (sourceType.IsGenericType)
        {
            Type[] genericArgs = sourceType.GetGenericArguments();
            if (genericArgs.Length == 1) // 单泛型参数集合（大部分常用集合，如List<T>、Queue<T>）
                return genericArgs[0];
        }

        // 情况2：非泛型集合（如ArrayList、Hashtable）且非空 → 取第一个元素的类型（兼容混合类型，但建议同类型）
        if (collection.Count > 0)
        {
            foreach (object element in source)
            {
                if (element != null)
                    return element.GetType();
            }
            // 所有元素均为null → 非泛型集合默认元素类型为object
            return typeof(object);
        }

        // 情况3：空非泛型集合（如new ArrayList()）→ 无法确定具体类型，默认允许添加object类型（ArrayList特性）
        return typeof(object);
    }


    /// <summary>
    /// 检查集合是否不允许重复元素，且val已存在（如HashSet.Contains返回true则不允许添加）
    /// </summary>
    [UnconditionalSuppressMessage("System.Diagnostics.CodeAnalysis", "IL2075", Justification = "Reflection-based code; library is not trim/AOT targeted.")]
    public static bool IsDuplicateNotAllowed(IEnumerable source, Type elementType, dynamic val)
    {
        Type sourceType = source.GetType();
        if (sourceType.IsGenericType)
        {
            Type setInterfaceType = typeof(ISet<>).MakeGenericType(elementType);
            if (setInterfaceType.IsAssignableFrom(sourceType))
            {
                MethodInfo? containsMethod = sourceType.GetMethod(
                    nameof(HashSet<int>.Contains),
                    new Type[] { elementType });
                if (containsMethod != null)
                {
                    bool isExists = (bool)(containsMethod.Invoke(source, new object[] { val }) ?? false);
                    if (isExists)
                        return true;
                }
            }
        }
        return false;
    }

}