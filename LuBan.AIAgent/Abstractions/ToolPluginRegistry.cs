/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Abstractions
*文件名： ToolPluginRegistry
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：工具插件注册表实现
*
*****************************************************************************/
namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 工具插件注册表
/// </summary>
public class ToolPluginRegistry
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly List<ILuBanToolPlugin> _plugins = new();
    private readonly LuBanAgentOptions _options;

    /// <summary>
    /// 创建 ToolPluginRegistry 实例
    /// </summary>
    /// <param name="plugins">插件集合</param>
    /// <param name="options">配置选项</param>
    public ToolPluginRegistry(
        IEnumerable<ILuBanToolPlugin> plugins,
        IOptions<LuBanAgentOptions> options)
    {
        _plugins.AddRange(plugins);
        _options = options.Value;
    }

    /// <summary>
    /// 获取所有已启用的插件
    /// </summary>
    /// <returns>已启用插件列表</returns>
    public IReadOnlyList<ILuBanToolPlugin> GetEnabledPlugins()
    {
        _lock.EnterReadLock();
        try
        {
            return _plugins.Where(p => p.IsEnabled(_options)).ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// 按工具组名称筛选已启用的插件
    /// </summary>
    /// <param name="groupNames">工具组名称列表，null 表示全部启用</param>
    /// <returns>筛选后的插件列表</returns>
    public IReadOnlyList<ILuBanToolPlugin> GetPlugins(IEnumerable<string>? groupNames = null)
    {
        var enabled = GetEnabledPlugins();
        if (groupNames == null)
            return enabled;

        var set = new HashSet<string>(groupNames, StringComparer.OrdinalIgnoreCase);
        return enabled.Where(p => set.Contains(p.GroupName)).ToList();
    }

    /// <summary>
    /// 获取所有插件的 AIFunction 集合
    /// </summary>
    /// <param name="sp">服务提供者</param>
    /// <param name="groupNames">工具组名称列表，null 表示全部启用</param>
    /// <returns>AIFunction 列表</returns>
    public IReadOnlyList<AIFunction> GetAllFunctions(IServiceProvider sp, IEnumerable<string>? groupNames = null)
        => GetPlugins(groupNames)
            .SelectMany(p => p.GetTools(sp))
            .ToList();
}
