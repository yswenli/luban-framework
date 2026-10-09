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

    /// <summary>Plan 模式。只读操作照常执行；其余操作不执行，仅通过 OnPlannedAction 收集为计划项，待用户确认后另行发起执行。</summary>
    Plan = 1,

    /// <summary>AcceptEdits 模式。放行编辑类操作（有目标路径且非删除类），非编辑类（脚本）与删除类仍需确认。</summary>
    AcceptEdits = 2,

    /// <summary>BypassPermissions 模式。跳过所有工具确认（需二次确认后生效）。</summary>
    BypassPermissions = 3
}

/// <summary>
/// 工具调用确认的评估结果。相比 bool 多出一档 <see cref="Planned"/>，
/// 用于区分"用户拒绝"与"Plan 模式下已记录计划但未执行"，避免向 LLM 传递错误语义。
/// </summary>
public enum EnumConfirmationOutcome
{
    /// <summary>允许执行。</summary>
    Allowed = 0,

    /// <summary>拒绝执行（用户拒绝、按 ESC 或未设置确认回调）。</summary>
    Denied = 1,

    /// <summary>Plan 模式：已记录为计划项，本次不执行。</summary>
    Planned = 2
}

/// <summary>
/// 工具调用确认上下文，持有当前会话的确认回调、路径检查器和取消令牌。
/// 由宿主层在每轮对话开始时设置、结束时清理。
/// </summary>
public class ToolConfirmationContext
{
    /// <summary>
    /// 工具调用确认回调函数。回调接收工具名称和参数，返回用户的选择结果。
    /// 异步签名允许宿主（CLI TUI）在回调内 await 用户确认（TaskCompletionSource），
    /// 避免占用 agent 线程同步阻塞。
    /// </summary>
    public Func<string, IReadOnlyDictionary<string, object?>, Task<ConfirmResult>>? Callback { get; set; }

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

    private readonly object _allowedLock = new();

    private HashSet<string> AllowedThisTurn { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 本会话已允许的工具名集合（用户选择"本会话全部允许"且工具无路径语义时记忆）。
    /// 与 <see cref="AllowedThisTurn"/> 不同，此集合跨轮存活，由 <see cref="ResetSession"/> 清除。
    /// </summary>
    private HashSet<string> AllowedThisSession { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 本轮已授权的路径根集合（用户选择"本轮全部允许"时按路径所在根记忆）。
    /// 命中根及其子路径在本轮内免确认。
    /// </summary>
    private List<string> AllowedRootsThisTurn { get; } = new();

    /// <summary>
    /// 判断本轮是否已允许指定工具（用户选择过"本轮全部允许"，无路径语义的工具按工具名记忆）。
    /// 线程安全：主代理写入与并行子代理读取可能并发。
    /// </summary>
    /// <param name="toolName">工具名称。</param>
    /// <returns>本轮已允许返回 true。</returns>
    public bool IsAllowedThisTurn(string toolName)
    {
        lock (_allowedLock)
        {
            return AllowedThisTurn.Contains(toolName);
        }
    }

    /// <summary>
    /// 将工具加入本轮已允许集合（用户选择"本轮全部允许"且工具无路径语义时由服务调用）。
    /// </summary>
    /// <param name="toolName">工具名称。</param>
    public void AllowThisTurn(string toolName)
    {
        lock (_allowedLock)
        {
            AllowedThisTurn.Add(toolName);
        }
    }

    /// <summary>
    /// 判断路径是否落在本轮已授权的某个根之下（含相等）。
    /// </summary>
    /// <param name="path">要检查的路径。</param>
    /// <returns>命中返回 true。</returns>
    public bool IsRootAllowedThisTurn(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        string normalized;
        try
        {
            normalized = Path.GetFullPath(path)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        catch
        {
            return false;
        }

        lock (_allowedLock)
        {
            foreach (var root in AllowedRootsThisTurn)
            {
                if (normalized.Equals(root, StringComparison.OrdinalIgnoreCase) ||
                    normalized.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 记录本轮已授权的路径根（用户选择"本轮全部允许"且工具带路径时由服务调用）。
    /// </summary>
    /// <param name="root">要授权的根路径。</param>
    public void AllowRootThisTurn(string root)
    {
        if (string.IsNullOrWhiteSpace(root))
            return;

        string normalized;
        try
        {
            normalized = Path.GetFullPath(root)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        catch
        {
            return;
        }

        lock (_allowedLock)
        {
            if (!AllowedRootsThisTurn.Contains(normalized, StringComparer.OrdinalIgnoreCase))
                AllowedRootsThisTurn.Add(normalized);
        }
    }

    private List<string> AllowedRootsThisSession { get; } = new();

    /// <summary>
    /// 判断本会话是否已允许指定工具（用户选择过"本会话全部允许"，无路径语义的工具按工具名记忆）。
    /// 与"本轮"不同，本会话授权跨轮存活，直到 <see cref="ResetSession"/> 被调用。
    /// </summary>
    /// <param name="toolName">工具名称。</param>
    /// <returns>本会话已允许返回 true。</returns>
    public bool IsAllowedThisSession(string toolName)
    {
        lock (_allowedLock)
        {
            return AllowedThisSession.Contains(toolName);
        }
    }

    /// <summary>
    /// 将工具加入本会话已允许集合（用户选择"本会话全部允许"且工具无路径语义时由服务调用）。
    /// </summary>
    /// <param name="toolName">工具名称。</param>
    public void AllowThisSession(string toolName)
    {
        lock (_allowedLock)
        {
            AllowedThisSession.Add(toolName);
        }
    }

    /// <summary>
    /// 判断路径是否落在本会话已授权的某个根之下（含相等）。
    /// </summary>
    /// <param name="path">要检查的路径。</param>
    /// <returns>命中返回 true。</returns>
    public bool IsRootAllowedThisSession(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        string normalized;
        try
        {
            normalized = Path.GetFullPath(path)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        catch
        {
            return false;
        }

        lock (_allowedLock)
        {
            foreach (var root in AllowedRootsThisSession)
            {
                if (normalized.Equals(root, StringComparison.OrdinalIgnoreCase) ||
                    normalized.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 记录本会话已授权的路径根（用户选择"本会话全部允许"且工具带路径时由服务调用）。
    /// </summary>
    /// <param name="root">要授权的根路径。</param>
    public void AllowRootThisSession(string root)
    {
        if (string.IsNullOrWhiteSpace(root))
            return;

        string normalized;
        try
        {
            normalized = Path.GetFullPath(root)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        catch
        {
            return;
        }

        lock (_allowedLock)
        {
            if (!AllowedRootsThisSession.Contains(normalized, StringComparer.OrdinalIgnoreCase))
                AllowedRootsThisSession.Add(normalized);
        }
    }

    /// <summary>
    /// 清空本会话授权集合（工具名与路径根）。供宿主要在会话内主动撤销时调用；
    /// <see cref="Reset"/> 不清除此处状态，以保证本会话授权跨轮存活。
    /// </summary>
    public void ResetSession()
    {
        lock (_allowedLock)
        {
            AllowedThisSession.Clear();
            AllowedRootsThisSession.Clear();
        }
    }

    /// <summary>
    /// Plan 模式计划项回调。Plan 模式下每个危险工具调用不立即确认，
    /// 而是通过此回调收集为 PlannedAction 列表，退出 Plan 时批量确认。
    /// </summary>
    public Action<string, IReadOnlyDictionary<string, object?>>? OnPlannedAction { get; set; }

    /// <summary>
    /// 用户明确拒绝本轮工具调用时的通知回调。宿主订阅后应立即取消本轮令牌以终止对话：
    /// 仅当用户确认回调返回 false、且本轮取消令牌尚未取消时触发（Esc 取消不触发）。
    /// 子代理代确认拒绝不触发（那不是用户的选择）。
    /// </summary>
    public Action<string, IReadOnlyDictionary<string, object?>>? OnUserDenied { get; set; }

    /// <summary>
    /// 重置上下文到初始状态（每轮对话结束时调用）。
    /// </summary>
    public void Reset()
    {
        Callback = null;
        WorkspacePathChecker = null;
        CancellationToken = default;
        Mode = ToolPermissionMode.Default;
        lock (_allowedLock)
        {
            AllowedThisTurn.Clear();
            AllowedRootsThisTurn.Clear();
        }
        OnPlannedAction = null;
        OnUserDenied = null;
    }

    private static readonly AsyncLocal<int> SubAgentScopeDepth = new();

    /// <summary>
    /// 当前调用链子代理作用域是否启用"全放行但守硬约束"策略（A2A 进程内执行器开启）。
    /// 为 true 时子代理不弹窗、按本轮已允许集合代确认；为 false 时沿用默认代确认语义。
    /// 与 <see cref="SubAgentScopeDepth"/> 同为 AsyncLocal，嵌套作用域取最内层值。
    /// </summary>
    private static readonly AsyncLocal<bool> SubAgentAllowAllInScope = new();

    /// <summary>
    /// 当前调用链是否处于编排子代理执行作用域。
    /// 子代理不向用户弹确认，由主代码按本轮已允许集合代确认。
    /// </summary>
    public static bool IsInSubAgentScope => SubAgentScopeDepth.Value > 0;

    /// <summary>
    /// 当前子代理作用域是否启用"全放行但守硬约束"策略。默认 false（行为不变）；
    /// 仅 A2A 进程内执行器通过 <see cref="EnterSubAgentScope(bool)"/> 显式开启。
    /// </summary>
    public static bool IsSubAgentAllowAll => SubAgentScopeDepth.Value > 0 && SubAgentAllowAllInScope.Value;

    /// <summary>
    /// 进入子代理执行作用域（DagScheduler 在节点执行期调用）；Dispose 退出。
    /// AsyncLocal 保证并行节点各自独立，互不串扰。
    /// 返回的令牌必须按 LIFO 顺序释放（重复释放会被忽略）。
    /// </summary>
    /// <returns>作用域令牌。</returns>
    public static IDisposable EnterSubAgentScope() => EnterSubAgentScope(allowAll: false);

    /// <summary>
    /// 进入子代理执行作用域，并指定是否启用"全放行但守硬约束"策略。
    /// 默认 <c>false</c> 保持既有语义；A2A 进程内执行器传 <c>true</c> 以启用 A3 语义。
    /// </summary>
    /// <param name="allowAll">是否启用"全放行但守硬约束"（删除类/工作区外仍硬拒）。</param>
    /// <returns>作用域令牌。</returns>
    public static IDisposable EnterSubAgentScope(bool allowAll)
    {
        var previousDepth = SubAgentScopeDepth.Value;
        var previousAllowAll = SubAgentAllowAllInScope.Value;
        SubAgentScopeDepth.Value = previousDepth + 1;
        SubAgentAllowAllInScope.Value = allowAll;
        return new SubAgentScopeToken(previousDepth, previousAllowAll);
    }

    /// <summary>
    /// 子代理作用域令牌。必须按 LIFO 顺序释放；重复 Dispose 不再改写深度，避免作用域泄漏。
    /// </summary>
    private sealed class SubAgentScopeToken(int previousDepth, bool previousAllowAll) : IDisposable
    {
        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
                return;
            SubAgentScopeDepth.Value = previousDepth;
            SubAgentAllowAllInScope.Value = previousAllowAll;
        }
    }
}

/// <summary>
/// 工具调用确认服务，用于判断危险工具是否需要人工确认。
/// </summary>
public interface IToolConfirmationService
{
    /// <summary>
    /// 统一评估一次工具调用：按当前权限模式分发，再套用路径/危险度规则。
    /// 所有工具插件应优先调用此方法，以便 Plan 模式能与"用户拒绝"区分开。
    /// </summary>
    /// <param name="toolName">工具名称。</param>
    /// <param name="path">操作目标路径；无路径语义的工具（脚本）传 null。</param>
    /// <param name="arguments">工具参数。</param>
    /// <returns>评估结果：<see cref="EnumConfirmationOutcome.Allowed"/> 允许、<see cref="EnumConfirmationOutcome.Denied"/> 拒绝、<see cref="EnumConfirmationOutcome.Planned"/> 已记录计划未执行。</returns>
    Task<EnumConfirmationOutcome> EvaluateAsync(string toolName, string? path, IReadOnlyDictionary<string, object?> arguments);

    /// <summary>
    /// 请求对指定工具调用进行确认。
    /// </summary>
    /// <param name="toolName">工具名称。</param>
    /// <param name="arguments">工具参数。</param>
    /// <returns>是否允许执行该工具调用的 Task。</returns>
    Task<bool> RequestConfirmation(string toolName, IReadOnlyDictionary<string, object?> arguments);

    /// <summary>
    /// 基于路径的工具调用确认。
    /// </summary>
    /// <param name="toolName">工具名称。</param>
    /// <param name="path">操作目标路径。</param>
    /// <param name="arguments">工具参数。</param>
    /// <returns>是否允许执行该工具调用的 Task。</returns>
    Task<bool> TryConfirmByPath(string toolName, string path, IReadOnlyDictionary<string, object?> arguments);

    /// <summary>
    /// 免确认名单：命中则在需要人工确认的环节直接放行，不打断用户。
    /// 可读写，宿主可增删以筛选受信任工具（如特定 MCP 工具）。
    /// 只免除"询问用户"，不改变 Plan 模式语义。
    /// 默认值来自配置 <c>LuBanAgent:Confirmation:AutoConfirmTools</c>，缺省为空。
    /// </summary>
    HashSet<string> AutoConfirmTools { get; set; }

    /// <summary>
    /// 删除类工具集合：无论路径是否在工作区内、无论何种放行策略都必须确认。可读写；
    /// 默认值来自配置 <c>LuBanAgent:Confirmation:AlwaysConfirmTools</c>。
    /// </summary>
    HashSet<string> AlwaysConfirmTools { get; set; }

    /// <summary>
    /// 只读工具集合：Plan 模式下无副作用，直接放行以保证 Agent 能读取上下文产出计划。可读写；
    /// 默认值来自配置 <c>LuBanAgent:Confirmation:ReadOnlyTools</c>。
    /// </summary>
    HashSet<string> ReadOnlyTools { get; set; }

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
    private readonly PathGuard? _pathGuard;

    /// <summary>
    /// 内置默认：删除类工具名，任何放行策略下都必须确认。
    /// </summary>
    private static readonly string[] DefaultAlwaysConfirmTools =
    [
        "DeleteFileAsync", "DeleteDirectoryAsync",
        "DeletePageAsync", // wiki 删除工具（WikiToolGroup.DeletePageAsync），删除均需确认
    ];

    /// <summary>
    /// 内置默认：只读工具名。Plan 模式下，仅当目标路径落在已授权范围（工作区/本轮或本会话已授权根）内时直接放行，
    /// 以便 Agent 读取上下文产出计划；越界只读仍记录为计划项，避免 Plan 模式下无授权读取区外内容。
    /// 该名单仅在 Plan 分支生效，Default / AcceptEdits 不再据此免确认（越界仍走确认）。
    /// 注意：任何会写库或有其它副作用的工具都不应加入此名单（例如 CompactContextAsync）。
    /// </summary>
    private static readonly string[] DefaultReadOnlyTools =
    [
        "ReadFileAsync", "ListDirectoryAsync", "GetWorkspaceOverviewAsync",
        "SearchFilesAsync", "GrepAsync", "GetFileInfoAsync",
        // Git 只读工具（Plan 模式放行）
        "GitStatusAsync", "GitDiffAsync", "GitLogAsync", "GitShowAsync",
        "GitBranchListAsync", "GitRemoteListAsync", "GitBlameAsync", "GitStashListAsync",
        // MCP 资源只读工具
        "mcp_list_resources", "mcp_read_resource",
    ];

    /// <summary>
    /// 创建 ToolConfirmationService 实例
    /// </summary>
    /// <param name="context">确认上下文（由 DI 容器注入的单例）</param>
    /// <param name="options">Agent 配置（可选）。缺省或对应数组留空时使用内置默认工具名集合</param>
    /// <param name="pathGuard">路径守卫（可选）。用于承接"本会话"级授权：命中即免确认，并在用户选择本会话授权时写入。</param>
    public ToolConfirmationService(
        ToolConfirmationContext context,
        IOptions<LuBanAgentOptions>? options = null,
        PathGuard? pathGuard = null)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _pathGuard = pathGuard;

        // 配置非空即整体替换内置默认（宿主可据此筛选），留空则回退默认；
        // 运行期仍可通过下方读写属性继续调整
        var confirmation = options?.Value.Confirmation;
        AlwaysConfirmTools = ToLookup(confirmation?.AlwaysConfirmTools, DefaultAlwaysConfirmTools);
        ReadOnlyTools = ToLookup(confirmation?.ReadOnlyTools, DefaultReadOnlyTools);

        // 免确认名单默认空：不做任何隐式放行，宿主显式配置或运行期添加才生效
        AutoConfirmTools = new HashSet<string>(confirmation?.AutoConfirmTools ?? [], StringComparer.Ordinal);
    }

    /// <summary>
    /// 把配置数组转为查找集合；配置为空时回退到内置默认。
    /// </summary>
    /// <param name="configured">配置提供的工具名数组。</param>
    /// <param name="fallback">内置默认工具名数组。</param>
    /// <returns>用于查找的集合。</returns>
    private static HashSet<string> ToLookup(string[]? configured, string[] fallback)
        => new(configured is { Length: > 0 } ? configured : fallback, StringComparer.Ordinal);

    /// <inheritdoc/>
    public HashSet<string> AutoConfirmTools { get; set; }

    /// <inheritdoc/>
    public HashSet<string> AlwaysConfirmTools { get; set; }

    /// <inheritdoc/>
    public HashSet<string> ReadOnlyTools { get; set; }

    /// <inheritdoc/>
    public async Task<EnumConfirmationOutcome> EvaluateAsync(
        string toolName, string? path, IReadOnlyDictionary<string, object?> arguments)
    {
        // ESC 已触发，自动拒绝所有工具调用
        if (_context.CancellationToken.IsCancellationRequested)
        {
            return EnumConfirmationOutcome.Denied;
        }

        // 子代理"全放行但守硬约束"作用域（A2A 进程内执行器 opt-in，默认关）：
        // 视为本轮全部已允许；工作区外路径为硬约束，直接拒绝（子代理无 UI 可确认），工作区内一律放行。
        if (ToolConfirmationContext.IsSubAgentAllowAll)
        {
            if (AlwaysConfirmTools.Contains(toolName))
                return EnumConfirmationOutcome.Denied;
            if (!string.IsNullOrEmpty(path) && !IsWithinWorkspace(path))
                return EnumConfirmationOutcome.Denied;
            return EnumConfirmationOutcome.Allowed;
        }

        // ── 模式分发（所有工具统一经此，避免脚本类工具绕过权限模式）──
        switch (_context.Mode)
        {
            case ToolPermissionMode.BypassPermissions:
                return EnumConfirmationOutcome.Allowed;

            case ToolPermissionMode.Plan:
                // 只读操作无副作用，但仅限已授权范围内（工作区/本轮或本会话已授权根）直接放行，
                // 以便 Agent 读取上下文产出计划；越界只读仍记录为计划项，避免无授权读取区外内容。
                if (ReadOnlyTools.Contains(toolName) &&
                    (string.IsNullOrEmpty(path) || IsAuthorizedPath(path)))
                {
                    return EnumConfirmationOutcome.Allowed;
                }

                // 其余操作仅记录为计划项，本次不执行
                _context.OnPlannedAction?.Invoke(toolName, arguments);
                return EnumConfirmationOutcome.Planned;

            case ToolPermissionMode.AcceptEdits:
                // 编辑类操作（工作区授权范围内的有路径且非删除类）直接放行；
                // 空串等同无路径语义，不能当作编辑类放行
                if (!string.IsNullOrEmpty(path) && !AlwaysConfirmTools.Contains(toolName) && IsAuthorizedPath(path))
                {
                    return EnumConfirmationOutcome.Allowed;
                }

                // 非编辑类（脚本）、删除类与越界路径走 Default 路径确认
                break;

            default: // ToolPermissionMode.Default
                break;
        }

        // ── Default 路径 ──

        // 本轮/本会话已允许的工具跳过确认（无路径语义的工具按工具名记忆）
        if (_context.IsAllowedThisTurn(toolName) || _context.IsAllowedThisSession(toolName))
        {
            return EnumConfirmationOutcome.Allowed;
        }

        // 删除类工具：无论路径是否在工作区内都必须确认，且"本轮/本会话全部允许"对删除类不记忆
        if (AlwaysConfirmTools.Contains(toolName))
        {
            return await AskUserAsync(toolName, path, arguments, suppressGrant: true).ConfigureAwait(false);
        }

        // 工作区授权范围内（工作区根 / 本轮或本会话已授权的根）：免确认
        if (IsAuthorizedPath(path))
        {
            return EnumConfirmationOutcome.Allowed;
        }

        // 越界或无路径语义的工具（脚本）：需要确认
        return await AskUserAsync(toolName, path, arguments).ConfigureAwait(false);
    }

    /// <summary>
    /// 判断路径是否落在当前已授权范围内：本轮/本会话已授权的根、PathGuard 工作区根，或工作区检查器认定的工作区内。
    /// </summary>
    /// <param name="path">要检查的路径。</param>
    /// <returns>已授权返回 true。</returns>
    private bool IsAuthorizedPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        if (_context.IsRootAllowedThisTurn(path) || _context.IsRootAllowedThisSession(path))
            return true;

        if (_pathGuard != null && _pathGuard.IsAllowed(path))
            return true;

        return IsWithinWorkspace(path);
    }

    /// <summary>
    /// 依据用户的授权结果记忆放行范围。无路径语义的工具按工具名记忆，其余按路径所在根记忆。
    /// 对文件路径取父目录为根，对目录路径取自身为根。
    /// 删除类工具的"本轮/本会话全部允许"不记忆（<paramref name="suppressGrant"/>），避免后续删除免确认。
    /// </summary>
    /// <param name="toolName">工具名称。</param>
    /// <param name="path">操作目标路径（可为 null）。</param>
    /// <param name="session">true 记入本会话，false 记入本轮。</param>
    /// <param name="suppressGrant">true 时不写入任何记忆（删除类每次都必须确认）。</param>
    private void RecordGrant(string toolName, string? path, bool session, bool suppressGrant = false)
    {
        if (suppressGrant)
            return;

        if (string.IsNullOrWhiteSpace(path))
        {
            if (session)
                _context.AllowThisSession(toolName);
            else
                _context.AllowThisTurn(toolName);
            return;
        }

        var root = ResolveGrantRoot(path!);

        // 授权根落到盘符根（如 D:\）：范围过大，不记忆，避免一次授权放行整盘
        if (IsDriveRoot(root))
            return;

        if (session)
            _context.AllowRootThisSession(root);
        else
            _context.AllowRootThisTurn(root);
    }

    /// <summary>
    /// 判断路径是否为盘符根（无父目录，如 <c>D:\</c>）。
    /// </summary>
    /// <param name="path">要检查的路径。</param>
    /// <returns>是盘符根返回 true。</returns>
    private static bool IsDriveRoot(string path)
    {
        try
        {
            var full = Path.GetFullPath(path);
            return string.IsNullOrEmpty(Path.GetDirectoryName(full));
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 计算授权记忆所用的根：目录路径取自身，文件路径取父目录；解析失败时回退原路径。
    /// </summary>
    /// <param name="path">操作目标路径。</param>
    /// <returns>用于记忆的根路径。</returns>
    private static string ResolveGrantRoot(string path)
    {
        try
        {
            var full = Path.GetFullPath(path);
            if (Directory.Exists(full))
                return full;

            var parent = Path.GetDirectoryName(full);
            return string.IsNullOrEmpty(parent) ? full : parent;
        }
        catch
        {
            return path;
        }
    }

    /// <summary>
    /// 调用宿主确认回调。未设置回调时默认拒绝。
    /// </summary>
    /// <param name="toolName">工具名称。</param>
    /// <param name="path">操作目标路径（可为 null）。</param>
    /// <param name="arguments">工具参数。</param>
    /// <param name="suppressGrant">true 时"本轮/本会话全部允许"不写入记忆（删除类每次都必须确认）。</param>
    /// <returns>允许或拒绝。</returns>
    private async Task<EnumConfirmationOutcome> AskUserAsync(
        string toolName, string? path, IReadOnlyDictionary<string, object?> arguments, bool suppressGrant = false)
    {
        // 免确认名单：命中则直接放行，不打断用户
        if (AutoConfirmTools.Contains(toolName))
        {
            return EnumConfirmationOutcome.Allowed;
        }

        // 子代理作用域：不向用户弹确认，改由主代码按已允许集合代确认
        if (ToolConfirmationContext.IsInSubAgentScope)
        {
            return (_context.IsAllowedThisTurn(toolName) || _context.IsAllowedThisSession(toolName))
                ? EnumConfirmationOutcome.Allowed
                : EnumConfirmationOutcome.Denied;
        }

        var callback = _context.Callback;
        if (callback == null)
        {
            return EnumConfirmationOutcome.Denied;
        }

        var result = await callback(toolName, arguments).ConfigureAwait(false);
        if (result is ConfirmResult.Allow)
        {
            return EnumConfirmationOutcome.Allowed;
        }

        if (result is ConfirmResult.AllowAll)
        {
            RecordGrant(toolName, path, session: false, suppressGrant);
            return EnumConfirmationOutcome.Allowed;
        }

        if (result is ConfirmResult.AllowSession)
        {
            RecordGrant(toolName, path, session: true, suppressGrant);
            return EnumConfirmationOutcome.Allowed;
        }

        // 用户明确拒绝（token 未取消）：通知宿主终止本轮
        if (!_context.CancellationToken.IsCancellationRequested)
        {
            _context.OnUserDenied?.Invoke(toolName, arguments);
        }

        return EnumConfirmationOutcome.Denied;
    }

    /// <inheritdoc/>
    public async Task<bool> RequestConfirmation(string toolName, IReadOnlyDictionary<string, object?> arguments)
        => await EvaluateAsync(toolName, null, arguments).ConfigureAwait(false) == EnumConfirmationOutcome.Allowed;

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

    /// <inheritdoc/>
    public async Task<bool> TryConfirmByPath(string toolName, string path, IReadOnlyDictionary<string, object?> arguments)
        => await EvaluateAsync(toolName, path, arguments).ConfigureAwait(false) == EnumConfirmationOutcome.Allowed;

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
