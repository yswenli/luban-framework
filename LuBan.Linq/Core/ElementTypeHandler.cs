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
    public static bool IsDuplicateNotAllowed(IEnumerable source, Type elementType, dynamic val)
    {
        Type sourceType = source.GetType();
        if (sourceType == null) return false;
        if (sourceType.IsGenericType)
        {
            Type genericCollectionType = typeof(IReadOnlyCollection<>).MakeGenericType(elementType);
            if (genericCollectionType.IsAssignableFrom(sourceType))
            {
                // 获取泛型Contains方法（如bool Contains<T>(T item)）
                MethodInfo? containsMethod = sourceType.GetMethod(
                    nameof(HashSet<int>.Contains),
                    [elementType]);
                if (containsMethod != null)
                {
                    // 调用Contains判断元素是否已存在（val已确保类型匹配）
                    bool isExists = (bool)(containsMethod.Invoke(source, [val]) ?? false);
                    if (isExists)
                        return true; // 元素已存在，不允许重复添加
                }
            }
        }
        return false;
    }

}