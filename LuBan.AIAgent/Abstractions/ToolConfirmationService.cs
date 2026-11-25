/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
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
    /// 用户交互提问回调（<c>AskUserQuestion</c> 工具）。回调接收问题列表，返回用户作答/取消结果。
    /// 未装配时视为「宿主无交互通道」，工具返回不可用说明而不是抛异常。
    /// </summary>
    public Func<IReadOnlyList<UserQuestion>, CancellationToken, Task<UserQuestionResult>>? AskQuestions { get; set; }

    /// <summary>
    /// 宿主交互串行门：工具确认与用户提问共用。各宿主的交互 UI 都是单槽
    /// （WebHost 单信号、CLI 单阻塞块、Codex 单模态框），同一会话同一时刻只放行一个交互，
    /// 其余排队，避免后到请求覆盖先到、导致先到者等不到结果而挂起。
    /// </summary>
    /// <remarks>本对象随上下文存活，<see cref="Reset"/> 不重置它，避免把在途等待的门丢弃。</remarks>
    public SemaphoreSlim UiGate { get; } = new(1, 1);

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
    /// 子代理作用域内的拒绝不触发（只失败当前节点，是否终止由 DAG 自行决定）。
    /// </summary>
    public Action<string, IReadOnlyDictionary<string, object?>>? OnUserDenied { get; set; }

    /// <summary>
    /// 宿主工具访问策略（硬闸门）。返回 <see cref="ConfirmResult.Deny"/> 即拒绝该调用，
    /// 返回非 <c>null</c> 的其他值视为策略已放行；返回 <c>null</c> 表示无意见，按权限模式继续判定。
    /// </summary>
    /// <remarks>
    /// 与 <see cref="Callback"/> 的区别是<b>不需要人工交互</b>：它承载「安全白名单」这类纯策略判断。
    /// 拒绝优先于权限模式、免确认名单与子代理放行——避免「免人工确认」被误读成「免策略拒绝」；
    /// 放行只免除「询问用户」，Plan 模式仍只记录计划项、不执行。
    /// </remarks>
    public Func<string, IReadOnlyDictionary<string, object?>, ConfirmResult?>? PolicyGuard { get; set; }

    /// <summary>
    /// 重置上下文到初始状态（每轮对话结束时调用）。
    /// </summary>
    public void Reset()
    {
        Callback = null;
        PolicyGuard = null;
        AskQuestions = null;
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
    /// 为 true 时子代理不弹窗、按本轮已允许集合代确认；为 false 时与主代理一样逐项请求用户确认。
    /// 与 <see cref="SubAgentScopeDepth"/> 同为 AsyncLocal；嵌套时只放大不收窄：
    /// 外层已开启则内层仍为 true，内层无法把外层已放行的链路重新收紧为逐项确认。
    /// </summary>
    private static readonly AsyncLocal<bool> SubAgentAllowAllInScope = new();

    private static readonly AsyncLocal<ToolConfirmationOrigin?> OriginInScope = new();

    /// <summary>
    /// 当前调用链的确认来源（编排子代理节点）；主代理为 <c>null</c>。
    /// 宿主在确认回调内读取并展示，仅用于 UI 标注来源，不参与授权判定。
    /// </summary>
    public static ToolConfirmationOrigin? CurrentOrigin => OriginInScope.Value;

    /// <summary>
    /// 当前调用链是否处于编排子代理执行作用域。
    /// 该作用域只改变「用户拒绝后是否终止整轮」：子代理内被拒只让当前工具/节点失败；
    /// 需要人工授权时仍与其他工具一样弹确认，授权结果由同会话所有子代理共享。
    /// </summary>
    public static bool IsInSubAgentScope => SubAgentScopeDepth.Value > 0;

    /// <summary>
    /// 当前子代理作用域是否启用"全放行但守硬约束"策略。默认 false（行为不变）；
    /// 仅 A2A 进程内执行器通过 <c>EnterSubAgentScope(allowAll: true)</c> 显式开启。
    /// </summary>
    public static bool IsSubAgentAllowAll => SubAgentScopeDepth.Value > 0 && SubAgentAllowAllInScope.Value;

    /// <summary>
    /// 进入子代理执行作用域（子代理执行器在节点执行期调用）；Dispose 退出。
    /// AsyncLocal 保证并行节点各自独立，互不串扰。
    /// 返回的令牌必须按 LIFO 顺序释放（重复释放会被忽略）。
    /// </summary>
    /// <returns>作用域令牌。</returns>
    public static IDisposable EnterSubAgentScope() => EnterSubAgentScope(allowAll: false);

    /// <summary>
    /// 进入子代理执行作用域，并指定是否启用"全放行但守硬约束"策略与确认来源。
    /// 默认 <c>false</c>：需要人工授权时弹确认（与主代理一致），拒绝只失败当前工具调用；
    /// A2A 进程内执行器无人工确认通道，恒传 <c>true</c>（不弹窗、按已允许集合代确认，硬约束照常拒绝）。
    /// 嵌套时按"只放大不收窄"合并：<c>previousAllowAll || allowAll</c>，
    /// 即外层已全放行时内层传 <c>false</c> 也不会重新变成逐项确认。
    /// </summary>
    /// <param name="allowAll">是否启用"全放行但守硬约束"（删除类/工作区外仍硬拒）。</param>
    /// <param name="origin">确认来源（编排节点标识/角色/描述）；未知时为 null，仅用于宿主 UI 标注。</param>
    /// <returns>作用域令牌。</returns>
    public static IDisposable EnterSubAgentScope(bool allowAll, ToolConfirmationOrigin? origin = null)
    {
        var previousDepth = SubAgentScopeDepth.Value;
        var previousAllowAll = SubAgentAllowAllInScope.Value;
        var previousOrigin = OriginInScope.Value;
        SubAgentScopeDepth.Value = previousDepth + 1;
        // 只放大不收窄：外层已全放行时，内层再传 false 不得把链路降级为逐项确认，
        // 否则内层（如非 A2A 路径的子代理）会让外层已获授权的执行重新弹确认。
        SubAgentAllowAllInScope.Value = previousAllowAll || allowAll;
        OriginInScope.Value = origin;
        return new SubAgentScopeToken(previousDepth, previousAllowAll, previousOrigin);
    }

    /// <summary>
    /// 仅设置确认来源作用域（不改子代理语义）。供远端宿主在本地回放确认帧时恢复来源，
    /// 使本地确认回调能读到与发起端一致的来源；Dispose 还原进入前的来源。
    /// </summary>
    /// <param name="origin">确认来源；null 表示清除当前来源。</param>
    /// <returns>作用域令牌。</returns>
    public static IDisposable EnterOriginScope(ToolConfirmationOrigin? origin)
    {
        var previous = OriginInScope.Value;
        OriginInScope.Value = origin;
        return new OriginScopeToken(previous);
    }

    /// <summary>
    /// 子代理作用域令牌。必须按 LIFO 顺序释放；重复 Dispose 不再改写深度，避免作用域泄漏。
    /// </summary>
    private sealed class SubAgentScopeToken(
        int previousDepth, bool previousAllowAll, ToolConfirmationOrigin? previousOrigin) : IDisposable
    {
        private int _disposed;

        /// <summary>
        /// 退出子代理作用域，还原进入前的深度、放行策略与来源；重复调用不再改动。
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
                return;
            SubAgentScopeDepth.Value = previousDepth;
            SubAgentAllowAllInScope.Value = previousAllowAll;
            OriginInScope.Value = previousOrigin;
        }
    }

    /// <summary>
    /// 仅来源的轻量作用域令牌；Dispose 还原进入前的来源。
    /// </summary>
    private sealed class OriginScopeToken(ToolConfirmationOrigin? previous) : IDisposable
    {
        private int _disposed;

        /// <summary>还原进入前的确认来源；重复调用不再改动。</summary>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
                return;
            OriginInScope.Value = previous;
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
    /// 默认值 = 内置默认项（本地记忆 <c>MemorySave</c> / <c>MemorySearch</c> / <c>MemoryList</c>
    /// 与编排工具 <c>plan_task</c> / <c>run_orchestration</c>）
    /// ∪ 配置 <c>LuBanAgent:Confirmation:AutoConfirmTools</c>。
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
    /// 内置默认：免人工确认工具名。收录标准是「低风险，或用户无法就其内容作出有意义裁决的
    /// Agent 内部动作」；免确认不等于放开副作用——路径授权与删除类规则照常生效。
    /// 子代理作用域不在免确认判断中拦截，只影响「用户拒绝后是否终止整轮」与拒绝文案。
    /// 与配置 <c>LuBanAgent:Confirmation:AutoConfirmTools</c> 取并集，宿主只能追加、不能整体抹掉内置项。
    /// </summary>
    /// <remarks>
    /// 命名约定：默认放 <b>C# 方法名</b>（保留 <c>Async</c> 后缀），与工具组调用
    /// <see cref="IToolConfirmationService.EvaluateAsync"/> 时传入的名字一致。例外是两类工具组，
    /// 它们暴露给模型的名字与 C# 方法名不同、且显式用模型可见名做确认名：
    /// <list type="bullet">
    /// <item>编排工具：<see cref="Orchestration.OrchestrationToolGroup"/> 传 <c>plan_task</c> / <c>run_orchestration</c>。</item>
    /// <item>本地记忆工具：<see cref="Tools.LocalMemory.LocalMemoryToolGroup"/> 传
    /// <c>MemorySave</c> / <c>MemorySearch</c> / <c>MemoryList</c>——因为 <c>SaveAsync</c> / <c>SearchAsync</c> /
    /// <c>ListAsync</c> 与其他工具组重名，按方法名匹配会误伤别的工具。</item>
    /// <item>用户提问工具：模型可见名 <c>AskUserQuestion</c>（C# 方法名是 <c>AskUserQuestionAsync</c>）。</item>
    /// </list>
    /// 免确认只跳过「询问用户」，不改变 Plan 模式语义——Plan 分支在
    /// <see cref="EvaluateAsync"/> 中先于免确认判断，仍只记录计划项、不执行。
    /// </remarks>
    private static readonly string[] DefaultAutoConfirmTools =
    [
        // 本地长期记忆（LocalMemoryToolGroup 显式传模型可见名）：
        // 保存低风险，打断「记住刚才的内容」得不偿失；搜索/列出只读，更没有可裁决的内容
        "MemorySave", "MemorySearch", "MemoryList",
        // 编排工具：plan_task / run_orchestration 是 Agent 内部的规划与调度动作（无目标路径、不直接触碰文件），
        // 用户无法就其内容作出有意义的裁决；逐次确认只会让整条编排链路多两次无谓打断。
        "plan_task",
        "run_orchestration",
        // 交互提问（InteractionToolGroup，模型可见名 AskUserQuestion）：工具本身不调用 EvaluateAsync（无副作用的 UI 动作），
        // 这里登记是防御性的——若将来接入统一确认门，提问不应再被要求「先确认再询问」。
        // 必须写模型可见名：写方法名 AskUserQuestionAsync 永不命中。
        "AskUserQuestion",
    ];

    /// <summary>
    /// 内置默认：删除类工具名，任何放行策略下都必须确认。
    /// </summary>
    /// <remarks>
    /// 命名约定：默认放 <b>C# 方法名</b>（保留 <c>Async</c> 后缀），与各工具组调用
    /// <see cref="IToolConfirmationService.EvaluateAsync"/> 时传入的 <c>nameof(XxxAsync)</c> 对齐；
    /// 不是暴露给模型的工具名（M.E.AI 会把尾部 <c>Async</c> 裁掉）。
    /// 例外：本地记忆删除由 <see cref="Tools.LocalMemory.LocalMemoryToolGroup.DeleteToolName"/> 显式传
    /// <c>MemoryDelete</c>——<c>DeleteAsync</c> 与别的工具组重名，按方法名匹配会误伤。
    /// 新增工具组时务必按同一约定传名，否则名单永不命中。
    /// </remarks>
    private static readonly string[] DefaultAlwaysConfirmTools =
    [
        "DeleteFileAsync", "DeleteDirectoryAsync",
        "DeletePageAsync", // wiki 删除工具（WikiToolGroup.DeletePageAsync），删除均需确认
        // Skill 管理写删
        "WriteSkillAsync", "DeleteSkillAsync",
        // MCP 管理写删
        "WriteMcpServerAsync", "DeleteMcpServerAsync",
        // 本地记忆删除（LocalMemoryToolGroup.DeleteToolName = MemoryDelete）：写入免确认，但删除属破坏性操作
        "MemoryDelete",
    ];

    /// <summary>
    /// 内置默认：子代理作用域内不得委派的工具名（命中即拒绝）。
    /// </summary>
    /// <remarks>
    /// 子代理作用域覆盖两种进程内形态：编排子代理以 <c>allowAll: false</c> 进入（需要授权时照常弹确认），
    /// A2A 进程内执行器以 <c>allowAll: true</c> 进入（无人工确认通道，按已允许集合代确认）。
    /// 两者都不得委派记忆写入/删除：记忆是<b>以用户身份留下的跨会话长期资产</b>，并行节点写入后
    /// 用户难以逐一复核；即便有确认通道，子代理的确认也可能落在没人盯着的后台链路里。
    /// 读取类（<c>MemorySearch</c> / <c>MemoryList</c>）不受限。
    /// 该约束只在进程内生效——远端 A2A 宿主（<c>HttpAgentHost</c>）以自己的主代理身份执行，不继承调用方的作用域。
    /// 单独成表而不复用 <see cref="DefaultAlwaysConfirmTools"/>：后者可被宿主配置整体替换，
    /// 用它兜底会在宿主自定义名单时把这条保护一并抹掉。
    /// </remarks>
    private static readonly HashSet<string> DefaultSubAgentForbiddenTools = new(StringComparer.Ordinal)
    {
        "MemorySave", "MemoryDelete",
    };

    /// <summary>
    /// 内置默认：只读工具名。Plan 模式下，仅当目标路径落在已授权范围（工作区/本轮或本会话已授权根）内时直接放行，
    /// 以便 Agent 读取上下文产出计划；越界只读仍记录为计划项，避免 Plan 模式下无授权读取区外内容。
    /// 该名单仅在 Plan 分支生效，Default / AcceptEdits 不再据此免确认（越界仍走确认）。
    /// 注意：任何会写库或有其它副作用的工具都不应加入此名单（例如 CompactContextAsync）。
    /// </summary>
    private static readonly string[] DefaultReadOnlyTools =
    [
        "ReadFileAsync", "ReadSpreadsheetAsync", "ListDirectoryAsync", "GetWorkspaceOverviewAsync",
        "SearchFilesAsync", "GrepAsync", "GetFileInfoAsync",
        "LogStatsAsync", "LogGrepAsync", "LogSliceAsync",
        // Git 只读工具（Plan 模式放行）
        "GitStatusAsync", "GitDiffAsync", "GitLogAsync", "GitShowAsync",
        "GitBranchListAsync", "GitRemoteListAsync", "GitBlameAsync", "GitStashListAsync",
        // MCP 资源只读工具
        "mcp_list_resources", "mcp_read_resource",
        // Skill 消费只读工具
        "ListSkillsAsync", "UseSkillAsync",
        // 本地记忆只读工具（LocalMemoryToolGroup 显式传模型可见名）：
        // Plan 模式下读记忆无副作用，放行以便 Agent 取上下文产出计划
        "MemorySearch", "MemoryList",
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

        // 免确认名单 = 内置默认（低风险操作）+ 宿主配置；宿主可追加，但不能把内置项整体替换掉
        AutoConfirmTools = new HashSet<string>(
            DefaultAutoConfirmTools.Concat(confirmation?.AutoConfirmTools ?? []), StringComparer.Ordinal);
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

        // 宿主策略硬闸门：拒绝优先于后续一切判定（含子代理放行与免确认名单）。
        // 放行只是「不必询问用户」，因此不在此处提前返回，交给下面的模式分发决定是否执行。
        var guardVerdict = _context.PolicyGuard?.Invoke(toolName, arguments);
        if (guardVerdict == ConfirmResult.Deny)
        {
            return EnumConfirmationOutcome.Denied;
        }

        var guardAllowed = guardVerdict is not null;

        // 子代理不可委派：写入/删除记忆会以用户身份留下跨会话长期资产，并行节点写入后用户难以逐一复核，
        // 故在子代理作用域内一律拒绝。覆盖两种进程内作用域：编排子代理（有确认通道也不放行）与
        // A2A 全放行（无确认通道）。远端 A2A 宿主以主代理身份执行，不继承本作用域。
        // Plan 模式例外：本就不执行工具，按计划项记录，避免用一条「拒绝」掩盖用户想做的事。
        if (ToolConfirmationContext.IsInSubAgentScope && DefaultSubAgentForbiddenTools.Contains(toolName))
        {
            if (_context.Mode == ToolPermissionMode.Plan)
            {
                _context.OnPlannedAction?.Invoke(toolName, arguments);
                return EnumConfirmationOutcome.Planned;
            }

            return EnumConfirmationOutcome.Denied;
        }

        // 子代理"全放行但守硬约束"作用域（仅 A2A 进程内执行器恒开，其他子代理不启用）：
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

        // 宿主策略已放行（白名单类）：只替代「询问用户」；放在框架自有免确认规则之后，
        // 避免宿主白名单改变工作区授权语义。删除类与 Plan / 子代理分支上面已各自给出结论。
        if (guardAllowed)
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

        if (_context.Callback == null)
        {
            return EnumConfirmationOutcome.Denied;
        }

        // 并行子代理可能同时请求确认，而宿主确认 UI 均为单槽，故串行化：
        // 同一会话同一时刻只允许一个交互（确认或提问）在途，其余排队，避免先到请求被覆盖后永久等待。
        try
        {
            await _context.UiGate.WaitAsync(_context.CancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return EnumConfirmationOutcome.Denied;
        }

        try
        {
            // 排队期间本轮可能已被取消或回合已收尾：重新取回调与令牌，
            // 避免排队者醒来后调用过期回调（例如向已关闭的回合流注册确认）。
            var activeCallback = _context.Callback;
            if (activeCallback is null || _context.CancellationToken.IsCancellationRequested)
            {
                return EnumConfirmationOutcome.Denied;
            }

            var result = await activeCallback(toolName, arguments).ConfigureAwait(false);
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

            // 用户明确拒绝：主代理作用域通知宿主终止本轮；子代理作用域只让该次工具调用失败，
            // 子代理收到拒绝消息后自行收尾，避免单个节点被拒就终止整轮。
            if (!ToolConfirmationContext.IsInSubAgentScope
                && !_context.CancellationToken.IsCancellationRequested)
            {
                _context.OnUserDenied?.Invoke(toolName, arguments);
            }

            return EnumConfirmationOutcome.Denied;
        }
        finally
        {
            _context.UiGate.Release();
        }
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
