/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Utils.Text
*文件名： WildcardMatcher
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：通配符匹配工具，支持 * 通配符（不区分大小写）
*
*****************************************************************************/
namespace LuBan.AIAgent.Utils.Text;

/// <summary>
/// 通配符匹配工具，支持 * 通配符（不区分大小写）
/// </summary>
public static class WildcardMatcher
{
    /// <summary>
    /// 判断值是否匹配通配符模式
    /// </summary>
    /// <param name="pattern">通配符模式，* 匹配任意字符</param>
    /// <param name="value">待匹配的值</param>
    /// <returns>匹配时返回 true，否则返回 false</returns>
    public static bool Match(string pattern, string value)
    {
        if (string.IsNullOrEmpty(pattern) || pattern == "*")
            return true;
        if (string.IsNullOrEmpty(value))
            return false;

        pattern = pattern.ToLowerInvariant();
        value = value.ToLowerInvariant();

        var parts = pattern.Split('*');
        if (parts.Length == 1)
            return value == pattern;

        var pos = 0;
        for (var i = 0; i < parts.Length; i++)
        {
            var part = parts[i];
            if (part.Length == 0) continue;

            var idx = value.IndexOf(part, pos, StringComparison.Ordinal);
            if (idx < 0) return false;
            if (i == 0 && idx != 0) return false;
            pos = idx + part.Length;
        }

        var lastPart = parts[^1];
        if (lastPart.Length > 0 && !value.EndsWith(lastPart, StringComparison.Ordinal))
            return false;

        return true;
    }
}
