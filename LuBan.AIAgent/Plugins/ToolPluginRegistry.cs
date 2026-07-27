/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Plugins
*文件名： ToolPluginRegistry
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：工具插件注册表
*
*****************************************************************************/


namespace LuBan.AIAgent.Plugins;

/// <summary>
/// 工具插件注册表
/// </summary>
public class ToolPluginRegistry
{
    private readonly IEnumerable<ILuBanToolPlugin> _plugins;
    private readonly LuBanAgentOptions _options;

    /// <summary>
    /// 创建 ToolPluginRegistry 实例
    /// </summary>
    /// <param name="plugins">所有注册的工具插件</param>
    /// <param name="options">配置选项</param>
    public ToolPluginRegistry(
        IEnumerable<ILuBanToolPlugin> plugins,
        IOptions<LuBanAgentOptions> options)
    {
        _plugins = plugins;
        _options = options.Value;
    }

    /// <summary>
    /// 获取所有启用的插件
    /// </summary>
    /// <returns>启用的插件列表</returns>
    public IReadOnlyList<ILuBanToolPlugin> GetEnabledPlugins()
        => _plugins.Where(p => p.IsEnabled(_options)).ToList();

    /// <summary>
    /// 根据分组名称获取插件
    /// </summary>
    /// <param name="groupNames">分组名称列表，null 表示全部启用</param>
    /// <returns>插件列表</returns>
    public IReadOnlyList<ILuBanToolPlugin> GetPlugins(IEnumerable<string>? groupNames = null)
    {
        var enabled = GetEnabledPlugins();
        if (groupNames == null)
            return enabled;

        var set = new HashSet<string>(groupNames, StringComparer.OrdinalIgnoreCase);
        return enabled.Where(p => set.Contains(p.GroupName)).ToList();
    }

    /// <summary>
    /// 收集所有启用的工具函数
    /// </summary>
    /// <param name="sp">服务提供者</param>
    /// <param name="groupNames">分组名称列表，null 表示全部启用</param>
    /// <returns>工具函数列表</returns>
    public IReadOnlyList<AIFunction> GetAllFunctions(IServiceProvider sp, IEnumerable<string>? groupNames = null)
        => GetPlugins(groupNames)
            .SelectMany(p => p.GetTools(sp))
            .ToList();
}
