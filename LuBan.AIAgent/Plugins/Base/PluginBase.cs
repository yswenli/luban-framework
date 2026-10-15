/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Plugins.Base
*文件名： PluginBase
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/15
*描述：插件基类 - 提供带「卸载即失效」令牌的 UI 投递与安全日志
*
*****************************************************************************/
namespace LuBan.AIAgent.Plugins.Base;

/// <summary>
/// 插件基类：为具体插件提供宿主访问、安全日志与<b>带失效令牌</b>的 UI 投递（PRD §3.4 / §6 #22）。
/// </summary>
/// <remarks>
/// 隔离约定（PRD §6 #20 / #22）：
/// <list type="number">
/// <item>插件<b>不得直接调用</b> <c>Dispatcher.UIThread.Post</c>，一律走 <see cref="PostToUi"/>；</item>
/// <item><see cref="ShutdownAsync"/> 一执行，令牌即失效，此后排入 UI 队列的回调<b>直接丢弃</b>，
/// 避免卸载后回调触碰已卸载程序集导致宿主崩溃。</item>
/// </list>
/// </remarks>
public abstract class PluginBase : IPlugin
{
    private IPluginHost? _host;
    private volatile bool _invalidated = true;

    /// <inheritdoc />
    public abstract string Id { get; }

    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public virtual string Version =>
        GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0";

    /// <inheritdoc />
    public virtual string Description => string.Empty;

    /// <summary>
    /// 宿主能力接口；未初始化或已卸载时访问会抛异常。
    /// </summary>
    protected IPluginHost Host =>
        _host ?? throw new InvalidOperationException($"插件 {Id} 尚未初始化或已卸载。");

    /// <summary>
    /// 是否处于可用状态（已初始化且未卸载）。
    /// </summary>
    protected bool IsInitialized => _host is not null && !_invalidated;

    /// <inheritdoc />
    public async Task InitializeAsync(IPluginHost host, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(host);

        _host = host;
        _invalidated = false;

        await OnInitializeAsync(host, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task ShutdownAsync(CancellationToken cancellationToken = default)
    {
        // 令牌先失效：此后经 PostToUi 排入的回调一律丢弃（PRD §6 #22）。
        _invalidated = true;

        try
        {
            await OnShutdownAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _host = null;
        }
    }

    /// <inheritdoc />
    public virtual IEnumerable<SkillDefinition> ContributeSkills() => Array.Empty<SkillDefinition>();

    /// <summary>
    /// 初始化钩子：注册侧栏条目与面板工厂。异常由宿主捕获隔离（PRD §6 #20）。
    /// </summary>
    /// <param name="host">宿主能力接口。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    protected virtual Task OnInitializeAsync(IPluginHost host, CancellationToken cancellationToken)
        => Task.CompletedTask;

    /// <summary>
    /// 关闭钩子：退订事件、停止定时器、释放句柄、取消后台任务。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    protected virtual Task OnShutdownAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    /// <summary>
    /// 把回调投递到 UI 线程，内置「卸载即失效」令牌（PRD §6 #22）。
    /// </summary>
    /// <remarks>
    /// 卸载后令牌失效，回调即使已被宿主排入 UI 队列，执行时也会被直接丢弃；
    /// 回调本身的异常亦在此捕获，不会外溢到宿主消息循环（PRD §6 #20）。
    /// </remarks>
    /// <param name="action">回调。</param>
    protected void PostToUi(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (_invalidated)
            return;

        var host = _host;
        if (host is null)
            return;

        host.PostToUi(() =>
        {
            if (_invalidated)
                return;

            try
            {
                action();
            }
            catch (Exception ex)
            {
                SafeLogError("UI 投递回调执行失败", ex);
            }
        });
    }

    /// <summary>
    /// 写一条插件日志（吞掉异常，避免日志通道故障反噬插件）。
    /// </summary>
    /// <param name="message">日志内容。</param>
    protected void SafeLog(string message)
    {
        try
        {
            _host?.Log($"[{Id}] {message}");
        }
        catch
        {
            // 日志失败不影响插件主流程
        }
    }

    /// <summary>
    /// 写一条插件错误日志（吞掉异常）。
    /// </summary>
    /// <param name="message">错误描述。</param>
    /// <param name="exception">异常对象，可为空。</param>
    protected void SafeLogError(string message, Exception? exception = null)
    {
        try
        {
            _host?.LogError($"[{Id}] {message}", exception);
        }
        catch
        {
            // 日志失败不影响插件主流程
        }
    }
}