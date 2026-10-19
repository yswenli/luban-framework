/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Plugins.Security
*文件名： PluginPermissionGuard
*版本号： V1.0.0.0
*唯一标识：新建
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/16
*描述：运行期出口门禁 - 越界即拒绝并审计（PRD §3.7 / §4.8 第 5 条）
*
*****************************************************************************/
namespace LuBan.AIAgent.Plugins.Security;

/// <summary>
/// 运行期出口门禁：网络 / 进程 / 凭据 / Git 四类出口在真正发起前调用本类型判定（PRD §3.7）。
/// </summary>
/// <remarks>
/// <para>
/// 判定依据是<b>当前权限环境</b>（<see cref="PluginScopeContext.Current"/>）：
/// <list type="bullet">
/// <item>无环境（<c>null</c>）→ 宿主自身调用，<b>一律放行</b>；</item>
/// <item>有环境 → 以该插件声明的权限判定，越界则审计 + 抛
/// <see cref="PluginPermissionDeniedException"/>（默认拒绝，PRD §3.7）。</item>
/// </list>
/// </para>
/// <para>
/// <see cref="PluginSecurityOptions.EnforceAtRuntime"/> 为 <c>false</c> 时只审计不拦截，
/// 供宿主排查兼容性问题；生产环境应保持 <c>true</c>。
/// </para>
/// </remarks>
public sealed class PluginPermissionGuard
{
    private readonly PluginPermissionEvaluator _evaluator;
    private readonly object _sync = new();
    private readonly Queue<PluginPermissionDenial> _recent = new();

    /// <summary>
    /// 创建门禁。
    /// </summary>
    /// <param name="evaluator">权限判定器；为空时取默认白名单。</param>
    public PluginPermissionGuard(PluginPermissionEvaluator? evaluator = null)
    {
        _evaluator = evaluator ?? new PluginPermissionEvaluator();
    }

    /// <summary>
    /// 进程级兜底门禁：供手工构造（如测试、无容器场景）使用；DI 场景应注入实例。
    /// </summary>
    public static PluginPermissionGuard Default { get; } = new();

    /// <summary>判定器（含生效的白名单策略）。</summary>
    public PluginPermissionEvaluator Evaluator => _evaluator;

    /// <summary>当前权限环境。</summary>
    public PluginPermissionScope? Current => PluginScopeContext.Current;

    /// <summary>最近被拒绝的访问（新→旧无关，按发生顺序），供 UI/诊断读取。</summary>
    public IReadOnlyList<PluginPermissionDenial> RecentDenials
    {
        get
        {
            lock (_sync)
                return _recent.ToArray();
        }
    }

    // ---------------- 纯判定（不抛异常） ----------------

    /// <summary>判定当前调用是否有权访问指定主机。</summary>
    /// <param name="host">目标主机。</param>
    /// <returns>允许返回 <c>true</c>。</returns>
    public bool IsNetworkAllowed(string? host) => Evaluate(scope => _evaluator.IsNetworkAllowed(scope.Permissions, host));

    /// <summary>判定当前调用是否有权执行指定命令。</summary>
    /// <param name="command">命令名。</param>
    /// <returns>允许返回 <c>true</c>。</returns>
    public bool IsProcessAllowed(string? command) => Evaluate(scope => _evaluator.IsProcessAllowed(scope.Permissions, command));

    /// <summary>判定当前调用是否有权读取指定凭据。</summary>
    /// <param name="key">凭据键。</param>
    /// <returns>允许返回 <c>true</c>。</returns>
    public bool IsCredentialAllowed(string? key) => Evaluate(scope => _evaluator.IsCredentialAllowed(scope.Permissions, key));

    /// <summary>判定当前调用是否有权执行指定 Git 操作。</summary>
    /// <param name="operation">操作名（如 <c>clone</c> / <c>push</c> / <c>add</c> / <c>commit</c> / <c>rev-parse</c>）。</param>
    /// <returns>允许返回 <c>true</c>。</returns>
    public bool IsGitAllowed(string? operation) => Evaluate(scope => _evaluator.IsGitAllowed(scope.Permissions, operation));

    // ---------------- 出口门禁（越界即拒绝） ----------------

    /// <summary>网络出口门禁：越界则审计并拒绝。</summary>
    /// <param name="host">目标主机。</param>
    /// <param name="context">调用点描述（用于审计与异常信息）。</param>
    public void RequireNetwork(string? host, string context)
        => Require("network", host, context, scope => _evaluator.IsNetworkAllowed(scope.Permissions, host));

    /// <summary>进程出口门禁：越界则审计并拒绝。</summary>
    /// <param name="command">命令名。</param>
    /// <param name="context">调用点描述。</param>
    public void RequireProcess(string? command, string context)
        => Require("process", command, context, scope => _evaluator.IsProcessAllowed(scope.Permissions, command));

    /// <summary>凭据出口门禁：越界则审计并拒绝（PRD §3.7：未声明则无法读取）。</summary>
    /// <param name="key">凭据键。</param>
    /// <param name="context">调用点描述。</param>
    public void RequireCredential(string? key, string context)
        => Require("credential", key, context, scope => _evaluator.IsCredentialAllowed(scope.Permissions, key));

    /// <summary>Git 出口门禁：越界则审计并拒绝。</summary>
    /// <param name="operation">操作名。</param>
    /// <param name="context">调用点描述。</param>
    public void RequireGit(string? operation, string context)
        => Require("git", operation, context, scope => _evaluator.IsGitAllowed(scope.Permissions, operation));

    private bool Evaluate(Func<PluginPermissionScope, bool> predicate)
    {
        var scope = PluginScopeContext.Current;

        // 无权限环境 = 宿主自身调用，不受插件权限约束
        if (scope is null)
            return true;

        // 仅审计不拦截模式：记录但放行
        return !_evaluator.Options.EnforceAtRuntime || predicate(scope);
    }

    private void Require(string dimension, string? target, string context, Func<PluginPermissionScope, bool> predicate)
    {
        var scope = PluginScopeContext.Current;
        if (scope is null)
            return;

        if (predicate(scope))
            return;

        var denial = Record(scope.PluginId, dimension, target, context);

        if (_evaluator.Options.EnforceAtRuntime)
            throw new PluginPermissionDeniedException(denial);
    }

    private PluginPermissionDenial Record(string pluginId, string dimension, string? target, string context)
    {
        var denial = new PluginPermissionDenial(pluginId, dimension, target ?? string.Empty, context ?? string.Empty);

        lock (_sync)
        {
            _recent.Enqueue(denial);

            var max = Math.Max(1, _evaluator.Options.MaxAuditRecords);
            while (_recent.Count > max)
                _recent.Dequeue();
        }

        if (_evaluator.Options.AuditDenials)
        {
            Logger.Warn($"[插件 {pluginId}] 权限越界被拦截（{dimension}）：{denial.Target}；调用点：{denial.Context}");
        }

        return denial;
    }
}

/// <summary>
/// 一次被拒绝的越界访问记录（PRD §3.7「越界访问被拦截并记录」）。
/// </summary>
/// <param name="PluginId">发起访问的插件标识。</param>
/// <param name="Dimension">权限维度（<c>network</c> / <c>process</c> / <c>credential</c> / <c>git</c>）。</param>
/// <param name="Target">被访问的目标（主机 / 命令 / 凭据键 / 操作名）。</param>
/// <param name="Context">调用点描述。</param>
public sealed record PluginPermissionDenial(
    string PluginId,
    string Dimension,
    string Target,
    string Context)
{
    /// <summary>发生时间（本地时区）。</summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.Now;
}

/// <summary>
/// 插件越界访问被拒绝时抛出的异常（PRD §3.7 / §4.8 第 5 条）。
/// </summary>
/// <remarks>
/// 插件侧应捕获本异常并提示用户，而非重试或绕过；宿主侧可用于诊断与审计。
/// </remarks>
public sealed class PluginPermissionDeniedException : Exception
{
    /// <summary>
    /// 创建异常。
    /// </summary>
    /// <param name="denial">拒绝记录。</param>
    public PluginPermissionDeniedException(PluginPermissionDenial denial)
        : base($"[插件 {denial.PluginId}] 权限越界：{denial.Dimension} 未授权访问 {denial.Target}（调用点：{denial.Context}）。" +
               "插件必须先在 plugin.json 的 permissions 中声明该维度；声明为通配 * 时，实际可访问范围以宿主白名单为上限。")
    {
        Denial = denial ?? throw new ArgumentNullException(nameof(denial));
    }

    /// <summary>拒绝记录。</summary>
    public PluginPermissionDenial Denial { get; }

    /// <summary>发起访问的插件标识。</summary>
    public string PluginId => Denial.PluginId;

    /// <summary>权限维度。</summary>
    public string Dimension => Denial.Dimension;

    /// <summary>被访问的目标。</summary>
    public string Target => Denial.Target;

    /// <summary>调用点描述。</summary>
    public string Context => Denial.Context;
}