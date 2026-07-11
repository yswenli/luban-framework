using System.Text.RegularExpressions;

namespace LuBan.ApprovalFlow.Core;

/// <summary>
/// 占位符解析器：用于解析字符串和字典中的变量占位符。
/// 支持 ${variable.path} 格式的占位符，从流程执行上下文中获取变量值。
/// </summary>
public class PlaceholderResolver
{
    /// <summary>
    /// 解析字符串中的占位符，替换为实际值。
    /// </summary>
    /// <param name="template">包含占位符的模板字符串。</param>
    /// <param name="context">流程执行上下文。</param>
    /// <returns>解析后的字符串。</returns>
    public static string Resolve(string template, FlowExecutionContext context)
    {
        if (string.IsNullOrEmpty(template))
            return template;

        // 匹配 ${...} 格式的占位符
        var pattern = @"\$\{([^}]+)\}";
        return Regex.Replace(template, pattern, match =>
        {
            var placeholder = match.Groups[1].Value;
            return ResolvePlaceholder(placeholder, context) ?? string.Empty;
        });
    }

    /// <summary>
    /// 解析单个占位符，从上下文中获取对应值。
    /// </summary>
    /// <param name="placeholder">占位符内容（不含${}）。</param>
    /// <param name="context">流程执行上下文。</param>
    /// <returns>解析后的值，未找到返回null。</returns>
    private static string? ResolvePlaceholder(string placeholder, FlowExecutionContext context)
    {
        var parts = placeholder.Split('.');
        if (parts.Length == 0) return null;

        if (parts.Length == 1)
        {
            // 单层变量：直接获取
            var value = context.GetValue(parts[0]);
            return value?.ToString();
        }
        else
        {
            // 嵌套变量：逐层获取
            var root = parts[0];
            var path = parts.Skip(1).ToArray();
            var value = context.GetNestedValue(root, path);
            return value?.ToString();
        }
    }

    /// <summary>
    /// 递归解析字典中所有字符串值的占位符。
    /// </summary>
    /// <param name="dict">要解析的字典。</param>
    /// <param name="context">流程执行上下文。</param>
    /// <returns>解析后的新字典。</returns>
    public static Dictionary<string, object> ResolveDictionary(
        Dictionary<string, object>? dict,
        FlowExecutionContext context)
    {
        if (dict == null) return new Dictionary<string, object>();

        var result = new Dictionary<string, object>();
        foreach (var kvp in dict)
        {
            if (kvp.Value is string strValue && strValue.Contains("${"))
            {
                // 字符串值包含占位符：解析替换
                result[kvp.Key] = Resolve(strValue, context);
            }
            else if (kvp.Value is Dictionary<string, object> nestedDict)
            {
                // 嵌套字典：递归解析
                result[kvp.Key] = ResolveDictionary(nestedDict, context);
            }
            else
            {
                // 其他类型：保持原值
                result[kvp.Key] = kvp.Value;
            }
        }
        return result;
    }

    /// <summary>
    /// 解析任意值中的占位符。
    /// </summary>
    /// <param name="value">要解析的值。</param>
    /// <param name="context">流程执行上下文。</param>
    /// <returns>解析后的值。</returns>
    public static object? ResolveValue(object? value, FlowExecutionContext context)
    {
        if (value == null) return null;

        if (value is string strValue && strValue.Contains("${"))
        {
            // 字符串包含占位符：解析替换
            var resolved = Resolve(strValue, context);
            return resolved;
        }

        if (value is Dictionary<string, object> dict)
        {
            // 字典类型：递归解析
            return ResolveDictionary(dict, context);
        }

        return value;
    }
}