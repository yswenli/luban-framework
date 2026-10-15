/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Plugins.Base
*文件名： BuiltinPluginCatalog
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/15
*描述：内置插件目录 - 宿主自带的进程内插件（不来自 plugin.json）
*
*****************************************************************************/
namespace LuBan.AIAgent.Plugins.Base;

/// <summary>
/// 内置插件目录：宿主自带的进程内插件（不来自 <c>plugins/</c> 目录，不参与 ALC 隔离）。
/// </summary>
/// <remarks>
/// 与 <see cref="LuBan.AIAgent.Skills.BuiltinSkillCatalog"/> 同构。以单例注入可避免宿主侧单例服务
/// 因捕获 scoped 服务而形成 captive dependency。内置插件同样是「插件」，由
/// <c>PluginManager</c> 与目录插件一并驱动生命周期，只是不受 ALC 卸载影响。
/// </remarks>
public sealed class BuiltinPluginCatalog
{
    private readonly Dictionary<string, IPlugin> _plugins;

    /// <summary>
    /// 创建内置插件目录。
    /// </summary>
    /// <param name="plugins">全部内置插件（DI 中的集合）。</param>
    public BuiltinPluginCatalog(IEnumerable<IPlugin> plugins)
    {
        _plugins = new Dictionary<string, IPlugin>(StringComparer.OrdinalIgnoreCase);
        foreach (var plugin in plugins)
        {
            if (string.IsNullOrWhiteSpace(plugin.Id))
                continue;

            _plugins[plugin.Id] = plugin;
        }
    }

    /// <summary>
    /// 全部内置插件。
    /// </summary>
    public IReadOnlyCollection<IPlugin> All => _plugins.Values;

    /// <summary>
    /// 内置插件数量。
    /// </summary>
    public int Count => _plugins.Count;

    /// <summary>
    /// 按 Id 获取内置插件。
    /// </summary>
    /// <param name="pluginId">插件标识，忽略大小写。</param>
    /// <returns>匹配的内置插件；未找到时返回 <c>null</c>。</returns>
    public IPlugin? Get(string pluginId)
        => _plugins.TryGetValue(pluginId, out var plugin) ? plugin : null;

    /// <summary>
    /// 是否为内置插件。
    /// </summary>
    /// <param name="pluginId">插件标识，忽略大小写。</param>
    /// <returns>内置时返回 <c>true</c>。</returns>
    public bool IsBuiltin(string pluginId) => _plugins.ContainsKey(pluginId);
}