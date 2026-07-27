namespace LuBan.AIAgent.Services;

/// <summary>
/// 工具调用确认服务，用于判断危险工具是否需要人工确认。
/// </summary>
public static class ToolConfirmationService
{
    private static readonly AsyncLocal<Func<string, IReadOnlyDictionary<string, object?>, bool>?> _callback = new();

    /// <summary>
    /// 获取或设置工具调用确认回调函数。
    /// 回调接收工具名称和参数，返回是否允许执行。
    /// </summary>
    public static Func<string, IReadOnlyDictionary<string, object?>, bool>? ConfirmationCallback
    {
        get => _callback.Value;
        set => _callback.Value = value;
    }

    private static readonly HashSet<string> DangerousTools = new()
    {
        "WriteFileAsync", "DeleteFileAsync", "MoveFileAsync", "CopyFileAsync",
        "CreateDirectoryAsync", "DeleteDirectoryAsync",
        "RunShellAsync", "RunLuaAsync", "RunPythonAsync",
        "ExecuteNonQueryAsync", "ExecuteInsertAsync", "ExecuteUpdateAsync", "ExecuteDeleteAsync",
        "SetAsync", "DeleteAsync", "FlushDatabaseAsync",
    };

    /// <summary>
    /// 判断指定工具是否需要人工确认。
    /// </summary>
    /// <param name="toolName">工具名称。</param>
    /// <returns>若工具为危险操作则需要确认，否则返回 false。</returns>
    public static bool RequiresConfirmation(string toolName)
        => DangerousTools.Contains(toolName);

    /// <summary>
    /// 请求对指定工具调用进行确认。
    /// 若未设置确认回调，则默认允许执行。
    /// </summary>
    /// <param name="toolName">工具名称。</param>
    /// <param name="arguments">工具参数。</param>
    /// <returns>是否允许执行该工具调用。</returns>
    public static bool RequestConfirmation(string toolName, IReadOnlyDictionary<string, object?> arguments)
    {
        var callback = ConfirmationCallback;
        if (callback == null)
            return false;
        return callback(toolName, arguments);
    }

    /// <summary>
    /// 将工具参数格式化为可读的字符串表示。
    /// </summary>
    /// <param name="arguments">工具参数。</param>
    /// <param name="maxLength">单个参数值的最大显示长度，超长将截断。</param>
    /// <returns>格式化后的参数字符串。</returns>
    public static string FormatArguments(IReadOnlyDictionary<string, object?> arguments, int maxLength = 200)
    {
        if (arguments == null || arguments.Count == 0)
            return "  无参数";

        var formatted = new List<string>();
        foreach (var kvp in arguments)
        {
            var value = kvp.Value switch
            {
                string s when s.Length > maxLength => s.Substring(0, maxLength) + "...",
                null => "null",
                _ => kvp.Value?.ToString() ?? "null"
            };
            formatted.Add($"  {kvp.Key}: {value}");
        }

        return string.Join("\n", formatted);
    }
}
