/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Abstractions
*文件名： ToolConfirmationService
*版本号： V2.0.0.0
*唯一标识：5fbbb94e-0e73-49ae-9105-0910710e8209
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：工具调用确认服务（依赖注入模式）
*****************************************************************************/
namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// Agent 工具权限确认模式。由宿主层（CLI TUI）在每轮对话开始时设置，ToolConfirmationService 据此决定确认策略。
/// </summary>
public enum ToolPermissionMode
{
    /// <summary>默认模式。每个工具调用逐一确认。</summary>
    Default = 0,

    /// <summary>Plan 模式。Agent 先生成执行计划，用户逐项确认后再批量执行。</summary>
    Plan = 1,

    /// <summary>AcceptEdits 模式。接受所有编辑操作，仅确认非编辑类工具。</summary>
    AcceptEdits = 2,

    /// <summary>BypassPermissions 模式。跳过所有工具确认（需二次确认后生效）。</summary>
    BypassPermissions = 3
}

/// <summary>
/// 工具调用确认上下文，持有当前会话的确认回调、路径检查器和取消令牌。
/// 由宿主层在每轮对话开始时设置、结束时清理。
/// </summary>
public class ToolConfirmationContext
{
    /// <summary>
    /// 工具调用确认回调函数。回调接收工具名称和参数，返回是否允许执行。
    /// </summary>
    public Func<string, IReadOnlyDictionary<string, object?>, bool>? Callback { get; set; }

    /// <summary>
    /// 工作区路径检查回调函数。回调接收路径，返回该路径是否在当前工作区内。
    /// </summary>
    public Func<string, bool>? WorkspacePathChecker { get; set; }

    /// <summary>
    /// 取消令牌，用于响应 ESC 键中断。
    /// </summary>
    public CancellationToken CancellationToken { get; set; }

    /// <summary>
    /// 当前权限模式。由宿主层在每轮对话开始前设置，控制确认策略。
    /// </summary>
    public ToolPermissionMode Mode { get; set; } = ToolPermissionMode.Default;

    /// <summary>
    /// 本轮（当前 agent 交互回合内）已允许的工具名称集合。
    /// 用户选择"本轮全部允许"后，后续同类工具跳过确认直到本轮结束。
    /// <see cref="Reset"/> 时清空。
    /// </summary>
    public HashSet<string> AllowedThisTurn { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Plan 模式计划项回调。Plan 模式下每个危险工具调用不立即确认，
    /// 而是通过此回调收集为 PlannedAction 列表，退出 Plan 时批量确认。
    /// </summary>
    public Action<string, IReadOnlyDictionary<string, object?>>? OnPlannedAction { get; set; }

    /// <summary>
    /// 重置上下文到初始状态（每轮对话结束时调用）。
    /// </summary>
    public void Reset()
    {
        Callback = null;
        WorkspacePathChecker = null;
        CancellationToken = default;
        Mode = ToolPermissionMode.Default;
        AllowedThisTurn.Clear();
        OnPlannedAction = null;
    }
}

/// <summary>
/// 工具调用确认服务，用于判断危险工具是否需要人工确认。
/// </summary>
public interface IToolConfirmationService
{
    /// <summary>
    /// 请求对指定工具调用进行确认。
    /// </summary>
    /// <param name="toolName">工具名称。</param>
    /// <param name="arguments">工具参数。</param>
    /// <returns>是否允许执行该工具调用。</returns>
    bool RequestConfirmation(string toolName, IReadOnlyDictionary<string, object?> arguments);

    /// <summary>
    /// 基于路径的工具调用确认。
    /// </summary>
    /// <param name="toolName">工具名称。</param>
    /// <param name="path">操作目标路径。</param>
    /// <param name="arguments">工具参数。</param>
    /// <returns>是否允许执行该工具调用。</returns>
    bool TryConfirmByPath(string toolName, string path, IReadOnlyDictionary<string, object?> arguments);

    /// <summary>
    /// 判断指定工具是否需要人工确认。
    /// </summary>
    bool RequiresConfirmation(string toolName);

    /// <summary>
    /// 将工具参数格式化为可读的字符串表示。
    /// </summary>
    string FormatArguments(IReadOnlyDictionary<string, object?> arguments, int maxLength = 200);
}

/// <summary>
/// 工具调用确认服务实现
/// </summary>
public class ToolConfirmationService : IToolConfirmationService
{
    private readonly ToolConfirmationContext _context;

    /// <summary>
    /// 创建 ToolConfirmationService 实例
    /// </summary>
    /// <param name="context">确认上下文（由 DI 容器注入的单例）</param>
    public ToolConfirmationService(ToolConfirmationContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
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
    /// 删除类工具集合，无论路径是否在工作区内都必须确认。
    /// </summary>
    private static readonly HashSet<string> AlwaysConfirmTools = new()
    {
        "DeleteFileAsync", "DeleteDirectoryAsync",
    };

    /// <summary>
    /// 判断指定工具是否需要人工确认。
    /// </summary>
    /// <param name="toolName">工具名称。</param>
    /// <returns>若工具为危险操作则需要确认，否则返回 false。</returns>
    public bool RequiresConfirmation(string toolName)
        => DangerousTools.Contains(toolName);

    /// <summary>
    /// 请求对指定工具调用进行确认。
    /// 若未设置确认回调，则默认拒绝。
    /// 若取消令牌已被取消，则自动拒绝。
    /// </summary>
    /// <param name="toolName">工具名称。</param>
    /// <param name="arguments">工具参数。</param>
    /// <returns>是否允许执行该工具调用。</returns>
    public bool RequestConfirmation(string toolName, IReadOnlyDictionary<string, object?> arguments)
    {
        // ESC 已触发，自动拒绝所有工具调用
        if (_context.CancellationToken.IsCancellationRequested)
        {
            return false;
        }

        // 本轮已允许的工具跳过确认
        if (_context.AllowedThisTurn.Contains(toolName))
        {
            return true;
        }

        var callback = _context.Callback;
        if (callback == null)
            return false;
        return callback(toolName, arguments);
    }

    /// <summary>
    /// 判断路径是否在当前工作区内。
    /// </summary>
    /// <param name="path">要检查的路径。</param>
    /// <returns>若在工作区内返回 true，否则返回 false。</returns>
    private bool IsWithinWorkspace(string path)
    {
        var checker = _context.WorkspacePathChecker;
        if (checker == null)
            return false;
        try
        {
            return checker(path);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 基于路径的工具调用确认。
    /// 规则：
    ///   1. 删除类工具（DeleteFileAsync/DeleteDirectoryAsync）——无论路径是否在工作区内，都必须确认。
    ///   2. 非删除类工具——路径在工作区内时免确认，工作区外时必须确认。
    ///   3. 未设置确认回调时默认拒绝（返回 false）。
    /// </summary>
    /// <param name="toolName">工具名称。</param>
    /// <param name="path">操作目标路径。</param>
    /// <param name="arguments">工具参数。</param>
    /// <returns>是否允许执行该工具调用。</returns>
    public bool TryConfirmByPath(string toolName, string path, IReadOnlyDictionary<string, object?> arguments)
    {
        // ── 模式分发 ──
        switch (_context.Mode)
        {
            case ToolPermissionMode.BypassPermissions:
                return true; // 跳过所有确认

            case ToolPermissionMode.Plan:
                // Plan 模式：不立即确认，收集到计划列表
                _context.OnPlannedAction?.Invoke(toolName, arguments);
                return true; // Plan 模式下先允许（后续批量确认时可能拒绝）

            case ToolPermissionMode.AcceptEdits:
                // AcceptEdits：编辑类操作直接放行，非编辑类仍需确认
                if (!AlwaysConfirmTools.Contains(toolName))
                {
                    return true; // 非危险操作直接放行
                }
                break; // 删除类操作走 Default 路径

            default: // ToolPermissionMode.Default
                break;
        }

        // ── Default 路径（现有逻辑不变）──

        // 本轮已允许的工具跳过确认
        if (_context.AllowedThisTurn.Contains(toolName))
        {
            return true;
        }

        // 删除类工具：始终需要确认
        if (AlwaysConfirmTools.Contains(toolName))
            return RequestConfirmation(toolName, arguments);

        // 非删除类工具：工作区内免确认
        if (!string.IsNullOrEmpty(path) && IsWithinWorkspace(path))
            return true;

        // 工作区外：需要确认
        return RequestConfirmation(toolName, arguments);
    }

    /// <summary>
    /// 将工具参数格式化为可读的字符串表示。
    /// </summary>
    /// <param name="arguments">工具参数。</param>
    /// <param name="maxLength">单个参数值的最大显示长度，超长将截断。</param>
    /// <returns>格式化后的参数字符串。</returns>
    public string FormatArguments(IReadOnlyDictionary<string, object?> arguments, int maxLength = 200)
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
