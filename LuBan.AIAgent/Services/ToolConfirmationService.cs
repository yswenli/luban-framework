using System.Collections.Generic;
using System.Threading;

namespace LuBan.AIAgent.Services;

public static class ToolConfirmationService
{
    private static readonly AsyncLocal<Func<string, IReadOnlyDictionary<string, object?>, bool>?> _callback = new();

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

    public static bool RequiresConfirmation(string toolName)
        => DangerousTools.Contains(toolName);

    public static bool RequestConfirmation(string toolName, IReadOnlyDictionary<string, object?> arguments)
    {
        var callback = ConfirmationCallback;
        if (callback == null)
            return false;
        return callback(toolName, arguments);
    }

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