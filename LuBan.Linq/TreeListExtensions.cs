/*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：System.Linq.Dynamic
*文件名： TreeListExtentions
*版本号： V1.0.0.0
*唯一标识：3a0499e4-68ef-4244-8707-17ecc8bef9df
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 18:21:32
* 描述：树列表拓展
*
*=================================================
*修改标记
* 修改时间：2023 / 12 / 4 18:21:32
* 修改人： yswenli
* 版本号： V1.0.0.0
*描述：树列表拓展
*
*****************************************************************************/

namespace System.Linq.Dynamic;

/// <summary>
/// 树列表拓展
/// </summary>
public static class TreeListExtensions
{
    /// <summary>
    /// 将列表转换为树形列表
    /// </summary>
    /// <typeparam name="Node"></typeparam>
    /// <param name="source"></param>
    /// <param name="idName"></param>
    /// <param name="childListName"></param>
    /// <param name="parentIdName"></param>
    /// <param name="rootValue"></param>
    /// <param name="maxLevel"></param>
    /// <returns></returns>
    public static List<TreeNode>? ToTreeList<TreeNode>(this IEnumerable<TreeNode> source,
        [NotNull] string idName,
        [NotNull] string childListName,
        [NotNull] string parentIdName,
        dynamic? rootValue = null,
        int maxLevel = 3)
        where TreeNode : class, new()
    {
        if (source == null) return null;
        
        var sourceList = source.ToList();
        var childrenByParentId = new Dictionary<object, List<TreeNode>>();
        
        foreach (var item in sourceList)
        {
            var parentId = item.GetPropertyValue(parentIdName);
            if (parentId != null)
            {
                if (!childrenByParentId.ContainsKey(parentId))
                {
                    childrenByParentId[parentId] = new List<TreeNode>();
                }
                childrenByParentId[parentId].Add(item);
            }
        }
        
        List<TreeNode> nodes;
        if (rootValue == null)
        {
            nodes = sourceList.Where(item => item.GetPropertyValue(parentIdName) == null).ToList();
        }
        else
        {
            object rootKey = (object)rootValue!;
            List<TreeNode>? foundNodes;
            if (childrenByParentId.TryGetValue(rootKey, out foundNodes) && foundNodes != null)
            {
                nodes = foundNodes;
            }
            else
            {
                nodes = new List<TreeNode>();
            }
        }
        
        if (nodes.Count > 0)
        {
            foreach (var node in nodes)
            {
                SetChildNote(node, sourceList, idName, childListName, parentIdName, childrenByParentId, 1, maxLevel);
            }
        }
        return nodes;
    }

    /// <summary>
    /// 设置子节点
    /// </summary>
    /// <typeparam name="Node"></typeparam>
    /// <param name="node"></param>
    /// <param name="source"></param>
    /// <param name="idName"></param>
    /// <param name="childListName"></param>
    /// <param name="parentIdName"></param>
    /// <param name="level"></param>
    /// <param name="maxLevel"></param>
    static void SetChildNote<TreeNode>(TreeNode node, List<TreeNode> source, string idName, string childListName, string parentIdName, Dictionary<object, List<TreeNode>> childrenByParentId, int level, int maxLevel = 3) where TreeNode : class, new()
    {
        var id = node.GetPropertyValue(idName);
        if (id == null) return;
        
        var childList = node.GetPropertyValue(childListName) as List<TreeNode>;
        childList ??= new List<TreeNode>();
        
        List<TreeNode>? children;
        if (childrenByParentId.TryGetValue(id!, out children))
        {
            if (children != null)
            {
                childList.AddRange(children);
            }
        }
        
        if (childList.Count > 0)
        {
            int nextLevel = level + 1;
            if (nextLevel <= maxLevel)
            {
                foreach (var child in childList)
                {
                    SetChildNote(child, source, idName, childListName, parentIdName, childrenByParentId, nextLevel, maxLevel);
                }
            }
            node.SetPropertyValue(childListName, childList);
        }
    }


    /// <summary>
    /// 将列表转换为树形列表
    /// </summary>
    /// <typeparam name="Node"></typeparam>
    /// <param name="source">源列表</param>
    /// <param name="idExpression">指定关键字段</param>
    /// <param name="childListExpression">指定列表属性字段</param>
    /// <param name="parentIdExpression">指定父级关键字段</param>
    /// <param name="rootValue">根节点值</param>
    /// <param name="maxLevel">最大层级</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static List<TreeNode>? ToTreeList<TreeNode>(this IEnumerable<TreeNode> source,
        Expression<Func<TreeNode, object>> idExpression,
        Expression<Func<TreeNode, IEnumerable<object>>> childListExpression,
        Expression<Func<TreeNode, object?>> parentIdExpression,
        dynamic? rootValue = null,
        int maxLevel = 3)
        where TreeNode : class, new()
    {
        if (source == null) return null;
        var idName = idExpression.Body.GetMemberName();
        var childListName = childListExpression.Body.GetMemberName();
        var parentIdName = parentIdExpression.Body.GetMemberName();
        if (idName.IsNullOrEmpty()) throw new ArgumentException("idExpression is null or empty");
        if (childListName.IsNullOrEmpty()) throw new ArgumentException("childListExpression is null or empty");
        if (parentIdName.IsNullOrEmpty()) throw new ArgumentException("parentIdExpression is null or empty");
        return ToTreeList(source, idName, childListName, parentIdName, rootValue, maxLevel);
    }

    /// <summary>
    /// 将列表转换为树形列表
    /// </summary>
    /// <typeparam name="Node"></typeparam>
    /// <param name="source">源列表</param>
    /// <param name="childListExpression">指定列表属性字段</param>
    /// <param name="parentIdExpression">指定父级关键字段</param>
    /// <param name="rootValue">根节点值</param>
    /// <param name="maxLevel">最大层级</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static List<TreeNode>? ToTreeList<TreeNode>(this IEnumerable<TreeNode> source,
        Expression<Func<TreeNode, IEnumerable<object>>> childListExpression,
        Expression<Func<TreeNode, object?>> parentIdExpression,
        dynamic? rootValue = null,
        int maxLevel = 3)
        where TreeNode : class, new()
    {
        if (source == null) return null;
        const string DefaultIdName = "Id";
        var childListName = childListExpression.Body.GetMemberName();
        var parentIdName = parentIdExpression.Body.GetMemberName();
        if (childListName.IsNullOrEmpty()) throw new ArgumentException("childListExpression is null or empty");
        if (parentIdName.IsNullOrEmpty()) throw new ArgumentException("parentIdExpression is null or empty");
        return ToTreeList(source, DefaultIdName, childListName, parentIdName, rootValue, maxLevel);
    }
}