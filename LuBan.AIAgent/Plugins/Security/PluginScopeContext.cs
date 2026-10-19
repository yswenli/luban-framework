/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Plugins.Security
*文件名： PluginScopeContext
*版本号： V1.0.0.0
*唯一标识：新建
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/16
*描述：权限环境的异步环境传递（AsyncLocal）
*
*****************************************************************************/
using LuBan.AIAgent.Plugins.Base;

namespace LuBan.AIAgent.Plugins.Security;

/// <summary>
/// 权限环境的异步上下文：用 <see cref="AsyncLocal{T}"/> 承载「当前调用以哪个插件身份发起」。
/// </summary>
/// <remarks>
/// <para>
/// 为何用环境上下文而非显式传参：出口位于连接器（如产品侧的 <c>MyConnectorClient</c> /
/// <c>MyMCPClient</c>），其调用方既可能是宿主也可能是插件代码，逐层透传会污染全部签名。
/// 环境上下文让出口以「当前是谁在调」这一事实做判定（PRD §3.7 宿主在网络出口处校验）。
/// </para>
/// <para>
/// <b>纪律</b>：<see cref="Enter"/> 进入的环境<b>只覆盖当前异步流程</b>；宿主要在调用插件代码前
/// <c>using</c> 进入，在退出时自动还原（可嵌套）。宿主自身入口（无环境）不受约束。
/// </para>
/// <para>
/// 插件在宿主未包裹的场景（如 UI 面板事件回调）需要自行进入权限环境，可用
/// <see cref="PluginBase.EnterPermissionScope"/>。
/// </para>
/// </remarks>
public static class PluginScopeContext
{
    private static readonly AsyncLocal<PluginPermissionScope?> Ambient = new();

    /// <summary>
    /// 当前权限环境；为 <c>null</c> 表示宿主自身调用（放行）。
    /// </summary>
    public static PluginPermissionScope? Current => Ambient.Value;

    /// <summary>
    /// 进入指定权限环境，返回退出时还原前一个环境的令牌（可嵌套）。
    /// </summary>
    /// <param name="scope">权限环境；为 <c>null</c> 时等价于进入「宿主调用」。</param>
    /// <returns>退出令牌，<c>Dispose</c> 后还原外层环境。</returns>
    public static IDisposable Enter(PluginPermissionScope? scope)
    {
        var previous = Ambient.Value;
        Ambient.Value = scope;
        return new ScopeToken(previous);
    }

    /// <summary>
    /// 进入指定插件的权限环境。
    /// </summary>
    /// <param name="pluginId">插件标识。</param>
    /// <param name="permissions">插件声明的权限。</param>
    /// <returns>退出令牌。</returns>
    public static IDisposable Enter(string pluginId, PluginPermissions? permissions)
        => Enter(new PluginPermissionScope(pluginId, permissions));

    private sealed class ScopeToken : IDisposable
    {
        private readonly PluginPermissionScope? _previous;
        private bool _disposed;

        public ScopeToken(PluginPermissionScope? previous) => _previous = previous;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Ambient.Value = _previous;
        }
    }
}