namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 工具插件注册表
/// </summary>
public class ToolPluginRegistry
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly List<ILuBanToolPlugin> _plugins = new();
    private readonly LuBanAgentOptions _options;

    public ToolPluginRegistry(
        IEnumerable<ILuBanToolPlugin> plugins,
        IOptions<LuBanAgentOptions> options)
    {
        _plugins.AddRange(plugins);
        _options = options.Value;
    }

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

    public IReadOnlyList<ILuBanToolPlugin> GetPlugins(IEnumerable<string>? groupNames = null)
    {
        var enabled = GetEnabledPlugins();
        if (groupNames == null)
            return enabled;

        var set = new HashSet<string>(groupNames, StringComparer.OrdinalIgnoreCase);
        return enabled.Where(p => set.Contains(p.GroupName)).ToList();
    }

    public IReadOnlyList<AIFunction> GetAllFunctions(IServiceProvider sp, IEnumerable<string>? groupNames = null)
        => GetPlugins(groupNames)
            .SelectMany(p => p.GetTools(sp))
            .ToList();
}
