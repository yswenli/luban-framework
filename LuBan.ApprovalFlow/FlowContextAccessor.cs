namespace LuBan.ApprovalFlow;

/// <summary>
/// 统一访问与设置流程上下文中的常用键，避免散落字符串常量。
/// </summary>
public static class FlowContextAccessor
{
    /// <summary>
    /// 上一次推进的边文本标签键名（例如“通过”、“退回”）。
    /// </summary>
    public const string KeyEdgeText = "edgeText";
    /// <summary>
    /// 上一个节点（任务/用户）执行产生的返回值键名。
    /// </summary>
    public const string KeyLastResult = "lastResult";

    /// <summary>
    /// 设置推进边的文本标签（例如“通过”、“退回”）。
    /// </summary>
    /// <param name="ctx">上下文字典。</param>
    /// <param name="value">标签文本（可为空）。</param>
    public static void SetEdgeText(Dictionary<string, object> ctx, string? value)
    {
        if (ctx == null) return;
        ctx[KeyEdgeText] = value ?? string.Empty;
    }

    /// <summary>
    /// 获取推进边的文本标签。
    /// </summary>
    /// <param name="ctx">上下文字典（可为空）。</param>
    /// <returns>标签文本；不存在返回 <c>null</c>。</returns>
    public static string? GetEdgeText(Dictionary<string, object>? ctx)
    {
        if (ctx != null && ctx.TryGetValue(KeyEdgeText, out var v) && v != null)
        {
            return v.ToString();
        }
        return null;
    }

    /// <summary>
    /// 设置上一个节点执行结果。
    /// </summary>
    /// <param name="ctx">上下文字典。</param>
    /// <param name="value">方法返回值或业务结果（可为空）。</param>
    public static void SetLastResult(Dictionary<string, object> ctx, object? value)
    {
        if (ctx == null) return;
        ctx[KeyLastResult] = value!;
    }

    /// <summary>
    /// 获取上一个节点执行结果。
    /// </summary>
    /// <param name="ctx">上下文字典（可为空）。</param>
    /// <returns>方法返回值或业务结果；不存在返回 <c>null</c>。</returns>
    public static object? GetLastResult(Dictionary<string, object>? ctx)
    {
        if (ctx != null && ctx.TryGetValue(KeyLastResult, out var v))
        {
            return v;
        }
        return null;
    }
}