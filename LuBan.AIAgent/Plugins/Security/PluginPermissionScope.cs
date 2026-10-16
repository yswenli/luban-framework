/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Plugins.Security
*文件名： PluginPermissionScope
*版本号： V1.0.0.0
*唯一标识：新建
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/16
*描述：一次插件调用所处的权限环境（插件 id + 声明权限）
*
*****************************************************************************/
namespace LuBan.AIAgent.Plugins.Security;

/// <summary>
/// 权限环境：描述「当前这段代码是以哪个插件的身份在跑」以及该插件已声明的权限。
/// </summary>
/// <remarks>
/// <para>
/// 与 <see cref="PluginScopeContext"/> 配合构成运行期出口门禁的判定依据（PRD §3.7「宿主在网络出口处校验」）。
/// 无环境时（<see cref="PluginScopeContext.Current"/> 为 <c>null</c>）视为宿主自身调用，一律放行——
/// 宿主自身的 git/网络能力不受插件权限约束。
/// </para>
/// <para>
/// 插件声明的权限（<see cref="Permissions"/>）为<b>空</b>时，意味着该插件未获任何授权，
/// 其发起的出口调用将被全部拒绝（默认拒绝）。
/// </para>
/// </remarks>
public sealed class PluginPermissionScope
{
    /// <summary>
    /// 创建权限环境。
    /// </summary>
    /// <param name="pluginId">插件标识。</param>
    /// <param name="permissions">插件声明的权限；为空时视为无授权。</param>
    public PluginPermissionScope(string pluginId, PluginPermissions? permissions)
    {
        PluginId = pluginId ?? string.Empty;
        Permissions = permissions ?? new PluginPermissions();
    }

    /// <summary>插件标识。</summary>
    public string PluginId { get; }

    /// <summary>插件声明的权限（运行期判定依据）。</summary>
    public PluginPermissions Permissions { get; }
}