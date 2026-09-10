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

    /// <summary>AcceptEdits 模式。放行编辑类操作（有目标路径且非删除类），非编辑类（脚本/数据库/Redis）与删除类仍需确认。</summary>
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
    /// 工具调用确认回调函数。回调接收工具名称和参数，返回是否允许执行的 Task。
    /// 异步签名允许宿主（CLI TUI）在回调内 await 用户确认（TaskCompletionSource），
    /// 避免占用 agent 线程同步阻塞。
    /// </summary>
    public Func<string, IReadOnlyDictionary<string, object?>, Task<bool>>? Callback { get; set; }

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
    /// 统一评估一次工具调用：按当前权限模式分发，再套用路径/危险度规则。
    /// 所有工具插件应优先调用此方法，以便 Plan 模式能与"用户拒绝"区分开。
    /// </summary>
    /// <param name="toolName">工具名称。</param>
    /// <param name="path">操作目标路径；无路径语义的工具（脚本/数据库/Redis）传 null。</param>
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

    /// <summary>
    /// 内置默认：删除类工具名，任何放行策略下都必须确认。
    /// </summary>
    private static readonly string[] DefaultAlwaysConfirmTools =
    [
        "DeleteFileAsync", "DeleteDirectoryAsync",
    ];

    /// <summary>
    /// 内置默认：只读工具名，Plan 模式下直接放行以便 Agent 读取上下文产出计划。
    /// </summary>
    private static readonly string[] DefaultReadOnlyTools =
    [
        "ReadFileAsync", "ListDirectoryAsync", "GetWorkspaceOverviewAsync", "ExecuteQueryAsync",
    ];

    /// <summary>
    /// 创建 ToolConfirmationService 实例
    /// </summary>
    /// <param name="context">确认上下文（由 DI 容器注入的单例）</param>
    /// <param name="options">Agent 配置（可选）。缺省或对应数组留空时使用内置默认工具名集合</param>
    public ToolConfirmationService(ToolConfirmationContext context, IOptions<LuBanAgentOptions>? options = null)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));

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

        // ── 模式分发（所有工具统一经此，避免脚本/数据库/Redis 类工具绕过权限模式）──
        switch (_context.Mode)
        {
            case ToolPermissionMode.BypassPermissions:
                return EnumConfirmationOutcome.Allowed;

            case ToolPermissionMode.Plan:
                // 只读操作无副作用，放行以便 Agent 读取上下文产出计划
                if (ReadOnlyTools.Contains(toolName))
                {
                    return EnumConfirmationOutcome.Allowed;
                }

                // 其余操作仅记录为计划项，本次不执行
                _context.OnPlannedAction?.Invoke(toolName, arguments);
                return EnumConfirmationOutcome.Planned;

            case ToolPermissionMode.AcceptEdits:
                // 编辑类操作（有目标路径且非删除类）直接放行；
                // 空串等同无路径语义，不能当作编辑类放行
                if (!string.IsNullOrEmpty(path) && !AlwaysConfirmTools.Contains(toolName))
                {
                    return EnumConfirmationOutcome.Allowed;
                }

                // 非编辑类（脚本/数据库/Redis）与删除类走 Default 路径确认
                break;

            default: // ToolPermissionMode.Default
                break;
        }

        // ── Default 路径 ──

        // 本轮已允许的工具跳过确认
        if (_context.AllowedThisTurn.Contains(toolName))
        {
            return EnumConfirmationOutcome.Allowed;
        }

        // 删除类工具：无论路径是否在工作区内都必须确认
        if (AlwaysConfirmTools.Contains(toolName))
        {
            return await AskUserAsync(toolName, arguments).ConfigureAwait(false);
        }

        // 有路径的非删除类工具：工作区内免确认
        if (!string.IsNullOrEmpty(path) && IsWithinWorkspace(path))
        {
            return EnumConfirmationOutcome.Allowed;
        }

        // 工作区外，或无路径语义的工具（脚本/数据库/Redis）：需要确认
        return await AskUserAsync(toolName, arguments).ConfigureAwait(false);
    }

    /// <summary>
    /// 调用宿主确认回调。未设置回调时默认拒绝。
    /// </summary>
    /// <param name="toolName">工具名称。</param>
    /// <param name="arguments">工具参数。</param>
    /// <returns>允许或拒绝。</returns>
    private async Task<EnumConfirmationOutcome> AskUserAsync(
        string toolName, IReadOnlyDictionary<string, object?> arguments)
    {
        // 免确认名单：命中则直接放行，不打断用户
        if (AutoConfirmTools.Contains(toolName))
        {
            return EnumConfirmationOutcome.Allowed;
        }

        var callback = _context.Callback;
        if (callback == null)
        {
            return EnumConfirmationOutcome.Denied;
        }

        return await callback(toolName, arguments).ConfigureAwait(false)
            ? EnumConfirmationOutcome.Allowed
            : EnumConfirmationOutcome.Denied;
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
