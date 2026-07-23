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
/// Enumerable集合扩展方法
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// 非泛型扩展方法：向IEnumerable集合中添加dynamic val元素
    /// </summary>
    /// <param name="source">要操作的非泛型IEnumerable集合（需实现ICollection以支持添加，如List&lt;T&gt;、ArrayList）</param>
    /// <param name="val">要添加的动态类型元素（支持值类型、引用类型、可空类型）</param>
    /// <returns>是否添加成功（true=添加完成，false=集合不允许添加重复元素且val已存在）</returns>
    /// <exception cref="ArgumentNullException">source或val为null（val为null时仅允许元素类型支持null）</exception>
    /// <exception cref="NotSupportedException">集合不支持添加操作（未实现ICollection或IsReadOnly为true）</exception>
    /// <exception cref="ArgumentException">val类型与集合元素类型不兼容，或val为null但元素类型不支持null</exception>
    public static bool Add(this IEnumerable source, dynamic val)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source), "源IEnumerable集合不能为null");

        // 暂存val是否为null（后续结合元素类型判断合法性）
        bool isValNull = val == null;
        // 非泛型IEnumerable无添加方法，依赖ICollection（可修改集合的核心接口）
        if (source is not ICollection collection)
            throw new NotSupportedException("集合不支持添加操作（未实现System.Collections.ICollection接口，如仅通过yield return生成的枚举器）");

        Type elementType = ElementTypeHandler.GetElementType(source, collection);
        if (isValNull)
        {
            if (elementType.IsValueType && Nullable.GetUnderlyingType(elementType) == null)
                throw new ArgumentException(
                    $"集合元素类型 {elementType.FullName} 是不可为null的值类型，无法添加null元素",
                    nameof(val));
        }
        else
        {
            // 获取val的实际类型（排除dynamic包装）
            Type valType = val!.GetType();
            // 若类型不匹配，尝试显式转换（如int→long、string→object）
            if (valType != elementType && !valType.IsAssignableTo(elementType))
            {
                try
                {
                    val = Convert.ChangeType(val, elementType); // 转换后覆盖原val，确保类型匹配
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(
                        $"无法将添加元素类型 {valType.FullName}（值：{val}）转换为集合元素类型 {elementType.FullName}",
                        nameof(val),
                        ex);
                }
            }
        }
        // 检查集合是否实现IReadOnlyCollection<T>（泛型集合）且具有“Contains”方法（判断元素是否已存在）
        bool isDuplicateNotAllowed = ElementTypeHandler.IsDuplicateNotAllowed(source, elementType, val);
        if (isDuplicateNotAllowed)
            return false; // 元素已存在，添加失败
        collection.Add((object)val!); // 调用ICollection.Add添加元素（泛型集合会自动处理类型适配）
        return true; // 添加成功
    }



    /// <summary>
    /// 非泛型扩展方法：从IEnumerable集合中删除所有等于dynamic val的元素
    /// </summary>
    /// <param name="source">要操作的非泛型IEnumerable集合（需实现ICollection以支持删除，如List&lt;T&gt;、HashSet&lt;T&gt;）</param>
    /// <param name="val">要删除的动态类型元素（支持值类型、引用类型、可空类型）</param>
    /// <returns>是否成功删除至少一个元素（true=删除过元素，false=未找到要删除的元素或集合不支持删除）</returns>
    /// <exception cref="ArgumentNullException">source或val为null（val为null时仅允许元素类型支持null）</exception>
    /// <exception cref="NotSupportedException">集合不支持删除操作（未实现ICollection或IsReadOnly为true）</exception>
    public static bool Remove(this IEnumerable source, dynamic val)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source), "源IEnumerable集合不能为null");

        // 处理val为null的情况：先暂存null（后续结合元素类型判断合法性）
        bool isValNull = val == null;
        // 非泛型IEnumerable本身无删除方法，需依赖ICollection（大部分可修改集合均实现，如List<T>、ArrayList）
        if (source is not ICollection collection)
            throw new NotSupportedException("集合不支持删除操作（未实现System.Collections.ICollection接口）");

        Type elementType = ElementTypeHandler.GetElementType(source, collection);

        if (isValNull && elementType.IsValueType && Nullable.GetUnderlyingType(elementType) == null)
            throw new ArgumentException(
                $"元素类型 {elementType.FullName} 是不可为null的值类型，无法删除null值",
                nameof(val));

        Func<object, bool> isEqual = ReflectionUtil.CreateEqualityChecker(elementType, val, isValNull);

        int initialCount = collection.Count; // 记录初始元素数量（用于判断是否删除成功）

        object[] elementsCopy = new object[collection.Count];
        collection.CopyTo(elementsCopy, 0);

        foreach (object element in elementsCopy)
        {
            if (isEqual(element))
                collection.Remove(element); // 调用ICollection.Remove删除元素
        }

        // 判断是否删除过元素（初始数量 > 最终数量 → 至少删除一个）
        return collection.Count < initialCount;
    }

    /// <summary>
    /// 为 IEnumerable 提供通用 Where 筛选（按指定字段名和值进行相等匹配）
    /// </summary>
    /// <param name="enumerable">源 IQueryable 集合</param>
    /// <param name="filterField">筛选字段名（必须是元素类型的公共属性）</param>
    /// <param name="filterValue">筛选值（类型需与字段类型兼容）</param>
    /// <returns>筛选后的 IEnumerable 集合</returns>
    /// <exception cref="ArgumentNullException">enumerable 或 filterField 为 null</exception>
    /// <exception cref="ArgumentException">filterField 为空字符串，或不是元素类型的公共属性</exception>
    /// <exception cref="InvalidCastException">filterValue 类型与字段类型不兼容</exception>
    public static IEnumerable Where(this IEnumerable enumerable, string filterField, object filterValue)
    {
        return enumerable.AsQueryable().Where(filterField, filterValue);
    }

    /// <summary>
    /// 为 IEnumerable 提供通用 OrderBy 排序（按指定字段名和排序方向）
    /// </summary>
    /// <param name="enumerable">源 IEnumerable 集合</param>
    /// <param name="filterField">排序字段名（必须是元素类型的公共属性）</param>
    /// <param name="isAscending">是否升序排序（默认true）</param>
    /// <returns>排序后的 IEnumerable 集合</returns>
    /// <exception cref="ArgumentNullException">enumerable 或 filterField 为 null</exception>
    /// <exception cref="ArgumentException">filterField 为空字符串，或不是元素类型的公共属性</exception>
    public static IEnumerable OrderBy(this IEnumerable enumerable, string filterField, bool isAscending = true)
    {
        return enumerable.AsQueryable().OrderBy(filterField, isAscending);
    }

    /// <summary>
    /// 为 IEnumerable 提供通用 Select 投影（按指定字段名提取元素属性）
    /// </summary>
    /// <param name="enumerable">源 IEnumerable 集合</param>
    /// <param name="filterField">投影字段名（必须是元素类型的公共属性）</param>
    /// <returns>投影后的 IEnumerable 集合（元素类型为投影字段类型）</returns>
    /// <exception cref="ArgumentNullException">enumerable 或 filterField 为 null</exception>
    /// <exception cref="ArgumentException">filterField 为空字符串，或不是元素类型的公共属性</exception>
    public static IEnumerable Select(this IEnumerable enumerable, string filterField)
    {
        return enumerable.AsQueryable().Select(filterField);
    }

    /// <summary>
    /// 判断IEnumerable是否包含任意元素（即是否非空）
    /// </summary>
    /// <param name="source">非泛型数据源IEnumerable（元素类型运行时确定）</param>
    /// <returns>包含至少一个元素返回true，否则返回false</returns>
    /// <exception cref="ArgumentNullException">source为null</exception>
    /// <exception cref="InvalidOperationException">无法解析元素类型或调用泛型Any方法失败</exception>
    public static bool Any(this IEnumerable source)
    {
        return source.AsQueryable().Any();
    }


    /// <summary>
    /// 动态判断IEnumerable中是否存在满足“指定字段 == 值”的元素
    /// </summary>
    /// <param name="source">源IEnumerable（非泛型，元素类型运行时确定）</param>
    /// <param name="filterField">筛选字段名（必须是源元素类型的公共实例属性）</param>
    /// <param name="val">筛选值（类型需与筛选字段类型匹配或可转换）</param>
    /// <returns>存在匹配元素返回true，否则返回false</returns>
    /// <exception cref="ArgumentNullException">source、filterField为null</exception>
    /// <exception cref="ArgumentException">filterField无效，或字段类型与val类型不兼容</exception>
    /// <exception cref="InvalidOperationException">无法调用Queryable.Any方法</exception>
    public static bool Any(this IEnumerable source, string filterField, object val)
    {
        return source.AsQueryable().Any(filterField, val);
    }

    /// <summary>
    /// 动态判断IEnumerable中是否存在满足"指定字段 != 值"的元素
    /// </summary>
    /// <param name="source">源IEnumerable（非泛型，元素类型运行时确定）</param>
    /// <param name="filterField">筛选字段名（必须是源元素类型的公共实例属性）</param>
    /// <param name="val">筛选值（类型需与筛选字段类型匹配或可转换）</param>
    /// <returns>存在匹配元素返回true，否则返回false</returns>
    /// <exception cref="ArgumentNullException">source、filterField为null</exception>
    /// <exception cref="ArgumentException">filterField无效，或字段类型与val类型不兼容</exception>
    /// <exception cref="InvalidOperationException">无法调用Queryable.Any方法</exception>
    public static bool NotAny(this IEnumerable source, string filterField, object val)
    {
        return source.AsQueryable().NotAny(filterField, val);
    }

    /// <summary>
    /// 获取IEnumerable的第一个元素，以dynamic类型返回（兼容任意元素类型）
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static dynamic First(this IEnumerable source)
    {
        return source.AsQueryable().First();
    }



    /// <summary>
    /// 非泛型版本：获取IEnumerable的第一个元素，空集合返回null，以dynamic?类型返回（兼容值类型/引用类型）
    /// </summary>
    /// <param name="source">非泛型数据源IEnumerable（元素类型运行时确定）</param>
    /// <returns>数据源第一个元素（dynamic?类型），空集合返回null</returns>
    /// <exception cref="ArgumentNullException">source为null</exception>
    /// <exception cref="InvalidOperationException">元素类型无法解析或调用FirstOrDefault方法失败</exception>
    public static dynamic? FirstOrDefault(this IEnumerable source)
    {
        return source.AsQueryable().FirstOrDefault();
    }

}
