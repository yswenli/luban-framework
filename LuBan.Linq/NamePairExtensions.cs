/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：System.Linq.Dynamic
*文件名： NamePairExtensions.cs
*版本号： V1.0.0.0
*唯一标识：5d2fbfd3-c688-4825-b7a7-38003c476325
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：NamePairExtensions 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：NamePairExtensions 类
*
*****************************************************************************/
namespace System.Linq.Dynamic;

/// <summary>
/// 名称对扩展类
/// </summary>
public static class NamePairExtensions
{
    /// <summary>
    /// 根据名称对数组设置名称对
    /// </summary>
    /// <typeparam name="T">类型参数</typeparam>
    /// <param name="t">对象实例</param>
    /// <param name="namePairs">名称对数组</param>
    /// <returns>名称对列表</returns>
    [UnconditionalSuppressMessage("System.Diagnostics.CodeAnalysis", "IL2090", Justification = "Reflection-based code; library is not trim/AOT targeted.")]
    public static List<NamePair> GetNamePairsByArr<T>(this T t, params NamePair[] namePairs) where T : class, new()
    {
        List<NamePair> result = [];
        var type = typeof(T);
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        if (namePairs == null || namePairs.Length == 0)
        {
            foreach (var property in properties)
            {
                result.Add(new NamePair(property.Name, property.Name));
            }
        }
        else
        {
            foreach (var namePair in namePairs)
            {
                var property = properties.FirstOrDefault(x => x.Name.Equals(namePair.SourceName, StringComparison.OrdinalIgnoreCase));
                if (property != null)
                {
                    result.Add(new NamePair(property.Name, namePair.TargetName));
                }
            }
        }
        return result;
    }

    /// <summary>
    /// 根据名称对元组设置名称对，
    /// sysApiLog.SetNamePairs(("Id", "编号"), ("Url", "地址"))
    /// </summary>
    /// <typeparam name="T">类型参数</typeparam>
    /// <param name="t">对象实例</param>
    /// <param name="namePairs">名称对元组数组</param>
    /// <returns>名称对列表</returns>
    public static List<NamePair> GetNamePairs<T>(this T t, params ValueTuple<string, string>[] namePairs) where T : class, new()
    {
        if (namePairs == null || namePairs.Length == 0) return t.GetNamePairsByArr();
        return t.GetNamePairsByArr(namePairs.Select(x => new NamePair(x.Item1, x.Item2)).ToArray());
    }

    /// <summary>
    /// 根据表达式和名称对元组设置名称对,
    /// sysApiLog.GetNamePairs((q => q.Id, "编号"), (q => q.Url, "地址"))
    /// </summary>
    /// <typeparam name="T">类型参数</typeparam>
    /// <param name="t">对象实例</param>
    /// <param name="namePairs">表达式和名称对元组数组</param>
    /// <returns>名称对列表</returns>
    public static List<NamePair> GetNamePairs<T>(this T t, params ValueTuple<Expression<Func<T, object>>, string>[] namePairs) where T : class, new()
    {
        if (namePairs == null || namePairs.Length == 0) return t.GetNamePairsByArr();
        return t.GetNamePairsByArr(namePairs.Select(x => new NamePair(x.Item1.GetMemberName(), x.Item2)).ToArray());
    }
}