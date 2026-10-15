/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Plugins.Lifecycle
*文件名： PluginHostScope
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/15
*描述：按插件包装宿主能力接口，回填 PluginId 并统一日志前缀
*
*****************************************************************************/
namespace LuBan.AIAgent.Plugins.Lifecycle;

/// <summary>
/// <see cref="IPluginHost"/> 的按插件包装：把宿主提供的共享实现包装为「当前插件专属」的实例。
/// </summary>
/// <remarks>
/// 目的有二：
/// <list type="number">
/// <item>回填 <see cref="PluginId"/>，让宿主侧实现知道调用来源；</item>
/// <item>给日志统一加 <c>[plugin-id]</c> 前缀，便于定位。</item>
/// </list>
/// 其余能力完全委托给宿主实现，框架不重复造轮子。
/// </remarks>
internal sealed class PluginHostScope : IPluginHost
{
    private readonly IPluginHost _inner;
    private readonly HashSet<string> _sidebarEntries = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _skills = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _mcpServers = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 创建插件专属宿主包装。
    /// </summary>
    /// <param name="inner">宿主提供的共享实现。</param>
    /// <param name="pluginId">当前插件标识。</param>
    public PluginHostScope(IPluginHost inner, string pluginId)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        PluginId = pluginId ?? string.Empty;
    }

    /// <summary>
    /// 本插件经本包装注册的侧栏条目 id（供卸载流程第 2 步精确清理）。
    /// </summary>
    /// <remarks>
    /// 插件可能在 <c>InitializeAsync</c> 中自行注册入口（PRD §3.4 第 5 步），而不仅是清单声明。
    /// 若只按清单清理，自行注册项会成为宿主持有的残留引用从而钉住 ALC，故必须逐项记账。
    /// </remarks>
    public IReadOnlyCollection<string> RegisteredSidebarEntries => _sidebarEntries;

    /// <summary>
    /// 本插件经本包装注册的技能 id（供卸载流程第 3 步精确反注册）。
    /// </summary>
    public IReadOnlyCollection<string> RegisteredSkillIds => _skills;

    /// <summary>
    /// 本插件经本包装注册的 MCP 服务器名（供卸载流程第 3 步精确反注册）。
    /// </summary>
    public IReadOnlyCollection<string> RegisteredMcpServers => _mcpServers;

    /// <inheritdoc />
    public string PluginId { get; }

    /// <inheritdoc />
    public IServiceProvider Services => _inner.Services;

    /// <inheritdoc />
    public void Log(string message) => _inner.Log($"[{PluginId}] {message}");

    /// <inheritdoc />
    public void LogError(string message, Exception? exception = null)
        => _inner.LogError($"[{PluginId}] {message}", exception);

    /// <inheritdoc />
    public string? GetConfig(string key) => _inner.GetConfig(key);

    /// <inheritdoc />
    public void RegisterPanel(string entryId, Func<IPluginPanel> factory)
        => _inner.RegisterPanel(entryId, factory);

    /// <inheritdoc />
    public void RegisterSidebarEntry(PluginSidebarEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        // 回填来源插件，宿主据此在卸载时精确清理。
        entry.PluginId = PluginId;
        _inner.RegisterSidebarEntry(entry);

        _sidebarEntries.Add(entry.Id);
    }

    /// <inheritdoc />
    public void UnregisterSidebarEntry(string entryId)
    {
        _inner.UnregisterSidebarEntry(entryId);
        _sidebarEntries.Remove(entryId);
    }

    /// <inheritdoc />
    public void UnregisterPanel(string entryId) => _inner.UnregisterPanel(entryId);

    /// <inheritdoc />
    public void RegisterCommand(PluginCommandDescriptor command) => _inner.RegisterCommand(command);

    /// <inheritdoc />
    public void RegisterSkill(SkillDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        _inner.RegisterSkill(definition);

        if (!string.IsNullOrWhiteSpace(definition.Id))
            _skills.Add(definition.Id);
    }

    /// <inheritdoc />
    public void UnregisterSkill(string skillId)
    {
        _inner.UnregisterSkill(skillId);
        _skills.Remove(skillId);
    }

    /// <inheritdoc />
    public void RegisterMcpServer(string name)
    {
        _inner.RegisterMcpServer(name);

        if (!string.IsNullOrWhiteSpace(name))
            _mcpServers.Add(name);
    }

    /// <inheritdoc />
    public void UnregisterMcpServer(string name)
    {
        _inner.UnregisterMcpServer(name);
        _mcpServers.Remove(name);
    }

    /// <inheritdoc />
    public void PostToUi(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _inner.PostToUi(action);
    }

    /// <inheritdoc />
    public IReadOnlyList<PluginSidebarEntry> GetSidebarEntries() => _inner.GetSidebarEntries();

    /// <inheritdoc />
    public IPluginPanel? GetPanel(string entryId) => _inner.GetPanel(entryId);
}