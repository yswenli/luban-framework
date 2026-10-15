/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Plugins.Lifecycle
*文件名： PluginManager
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/15
*描述：插件管理器 - 扫描/启停/卸载与 7 步卸载流程（PRD §3.4 / §3.5 / §3.12）
*
*****************************************************************************/
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using LuBan.AIAgent.Plugins.Loading;

namespace LuBan.AIAgent.Plugins.Lifecycle;

/// <summary>
/// 插件管理器：插件发现、状态机、加载/卸载编排（PRD §3.4 / §3.5 / §3.12）。
/// </summary>
/// <remarks>
/// <para>
/// 崩溃隔离（PRD §6 #20）：每一次进入插件代码（<c>InitializeAsync</c> / <c>ShutdownAsync</c> /
/// <c>ContributeSkills</c>）都包裹在 <c>try/catch</c> 中，插件异常只让该插件进入
/// <see cref="PluginState.Error"/>，不带崩宿主。
/// </para>
/// <para>
/// 卸载严格遵循 §3.12 七步顺序，且第 6 步以「多轮 GC + 弱引用验证」判定是否真正回收；
/// 未通过则降级为 <see cref="PluginState.Disabled"/> 并记录诊断日志。
/// </para>
/// </remarks>
public sealed class PluginManager
{
    private readonly PluginLoader _loader;
    private readonly PluginDependencyResolver _dependencyResolver;
    private readonly BuiltinPluginCatalog _builtinCatalog;
    private readonly IPluginStateRepository _stateRepository;
    private readonly IServiceProvider _services;
    private readonly PluginManagerOptions _options;

    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly Dictionary<string, PluginEntry> _entries = new(StringComparer.OrdinalIgnoreCase);

    private IPluginHost? _sharedHost;

    /// <summary>
    /// 创建插件管理器。
    /// </summary>
    /// <param name="loader">插件加载器。</param>
    /// <param name="dependencyResolver">依赖解析器。</param>
    /// <param name="builtinCatalog">内置插件目录。</param>
    /// <param name="stateRepository">状态仓储。</param>
    /// <param name="services">宿主服务容器（根容器，用于延迟解析 <see cref="IPluginHost"/>）。</param>
    /// <param name="options">管理器选项。</param>
    public PluginManager(
        PluginLoader loader,
        PluginDependencyResolver dependencyResolver,
        BuiltinPluginCatalog builtinCatalog,
        IPluginStateRepository stateRepository,
        IServiceProvider services,
        PluginManagerOptions? options = null)
    {
        _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        _dependencyResolver = dependencyResolver ?? throw new ArgumentNullException(nameof(dependencyResolver));
        _builtinCatalog = builtinCatalog ?? throw new ArgumentNullException(nameof(builtinCatalog));
        _stateRepository = stateRepository ?? throw new ArgumentNullException(nameof(stateRepository));
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _options = options ?? new PluginManagerOptions();
    }

    /// <summary>
    /// 插件状态发生变化时触发（宿主据此刷新侧栏 / 管理器界面）。
    /// </summary>
    public event EventHandler<PluginChangedEventArgs>? Changed;

    /// <summary>
    /// 扫描插件目录与内置插件，登记为 <see cref="PluginState.Installed"/> 或恢复已持久化状态。
    /// </summary>
    /// <param name="roots">插件根目录列表；为空时按 PRD §3.3 取应用级插件根目录。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>扫描后登记的插件数量。</returns>
    public async Task<int> ScanAsync(IEnumerable<string>? roots = null, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // 内置插件优先登记（不来自 plugin.json，不参与 ALC 隔离）
            // 与目录插件一致：读取持久化状态，支持「禁用后重启仍禁用」与开机恢复（PRD §3.5）
            foreach (var builtin in _builtinCatalog.All)
            {
                if (string.IsNullOrWhiteSpace(builtin.Id) || _entries.ContainsKey(builtin.Id))
                    continue;

                var builtinRecord = await _stateRepository.GetAsync(builtin.Id, cancellationToken).ConfigureAwait(false);

                _entries[builtin.Id] = new PluginEntry
                {
                    Id = builtin.Id,
                    IsBuiltin = true,
                    BuiltinPlugin = builtin,
                    State = builtinRecord?.State switch
                    {
                        PluginState.Disabled => PluginState.Disabled,
                        PluginState.Error => PluginState.Error,
                        _ => PluginState.Installed
                    },
                    RequiresRestart = builtinRecord?.RequiresRestart ?? false,
                    LastError = builtinRecord?.LastError,
                    ShouldEnableOnStartup = builtinRecord?.State == PluginState.Enabled
                };
            }

            var rootList = (roots ?? PluginLocation.ResolveDirectories()).Where(r => !string.IsNullOrWhiteSpace(r)).ToList();

            foreach (var root in rootList)
            {
                if (!Directory.Exists(root))
                    continue;

                foreach (var directory in Directory.EnumerateDirectories(root))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var manifest = _loader.ReadManifest(directory);
                    if (manifest is null || string.IsNullOrWhiteSpace(manifest.Id))
                        continue;

                    // 同名插件先登记者胜出（roots 顺序即优先级）
                    if (_entries.ContainsKey(manifest.Id))
                        continue;

                    var record = await _stateRepository.GetAsync(manifest.Id, cancellationToken).ConfigureAwait(false);

                    _entries[manifest.Id] = new PluginEntry
                    {
                        Id = manifest.Id,
                        IsBuiltin = false,
                        Directory = directory,
                        Manifest = manifest,
                        State = record?.State switch
                        {
                            PluginState.Disabled => PluginState.Disabled,
                            PluginState.Error => PluginState.Error,
                            _ => PluginState.Installed
                        },
                        RequiresRestart = record?.RequiresRestart ?? false,
                        LastError = record?.LastError,
                        ShouldEnableOnStartup = record?.State == PluginState.Enabled
                    };
                }
            }

            return _entries.Count;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// 开机恢复：把上次处于 <see cref="PluginState.Enabled"/> 的插件重新启用（PRD §3.5）。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>成功启用的插件数量。</returns>
    public async Task<int> RestoreEnabledAsync(CancellationToken cancellationToken = default)
    {
        var pending = _entries.Values
            .Where(entry => entry.ShouldEnableOnStartup && entry.State != PluginState.Enabled)
            .Select(entry => entry.Id)
            .ToList();

        var succeeded = 0;
        foreach (var pluginId in pending)
        {
            if (await EnableAsync(pluginId, cancellationToken).ConfigureAwait(false))
                succeeded++;
        }

        return succeeded;
    }

    /// <summary>
    /// 取全部插件描述（供宿主管理器界面展示，PRD §3.9）。
    /// </summary>
    /// <returns>插件描述列表。</returns>
    public IReadOnlyList<PluginDescriptor> GetPlugins()
        => _entries.Values
            .Select(entry => entry.ToDescriptor())
            .OrderByDescending(descriptor => descriptor.State == PluginState.Enabled)
            .ThenBy(descriptor => descriptor.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

    /// <summary>
    /// 取单个插件描述。
    /// </summary>
    /// <param name="pluginId">插件标识。</param>
    /// <returns>插件描述；未登记返回 <c>null</c>。</returns>
    public PluginDescriptor? GetPlugin(string pluginId)
        => _entries.TryGetValue(pluginId ?? string.Empty, out var entry) ? entry.ToDescriptor() : null;

    /// <summary>
    /// 启用插件：加载（ALC + 实例）→ 注册 UI → 插件初始化 → 注册 Skill / MCP（PRD §3.4 第 3–7 步）。
    /// </summary>
    /// <param name="pluginId">插件标识。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>是否启用成功。</returns>
    public async Task<bool> EnableAsync(string pluginId, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!_entries.TryGetValue(pluginId ?? string.Empty, out var entry))
            {
                Logger.Warn($"启用插件失败：未登记的插件 {pluginId}");
                return false;
            }

            if (entry.State == PluginState.Enabled)
                return true;

            var host = ResolveHost();
            if (host is null)
            {
                await MarkErrorAsync(entry, "宿主未提供 IPluginHost，无法启用插件。", cancellationToken).ConfigureAwait(false);
                return false;
            }

            // 依赖先行：被依赖的插件必须处于 Enabled
            if (!entry.IsBuiltin && !await EnsureDependenciesEnabledAsync(entry, cancellationToken).ConfigureAwait(false))
            {
                await MarkErrorAsync(entry, "依赖插件未启用。", cancellationToken).ConfigureAwait(false);
                return false;
            }

            return entry.IsBuiltin
                ? await ActivateBuiltinAsync(entry, host, cancellationToken).ConfigureAwait(false)
                : await ActivateDirectoryPluginAsync(entry, host, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// 禁用插件：执行 §3.12 七步卸载流程，随后置 <see cref="PluginState.Disabled"/>。
    /// </summary>
    /// <param name="pluginId">插件标识。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>是否禁用成功。</returns>
    public async Task<bool> DisableAsync(string pluginId, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!_entries.TryGetValue(pluginId ?? string.Empty, out var entry))
                return false;

            var unloaded = await UnloadAsync(entry, cancellationToken).ConfigureAwait(false);

            entry.State = PluginState.Disabled;
            entry.RequiresRestart = !unloaded;
            entry.LastError = unloaded ? null : "程序集卸载验证未通过（已降级为禁用，需重启彻底生效）。";
            entry.ShouldEnableOnStartup = false;

            await PersistAsync(entry, cancellationToken).ConfigureAwait(false);
            RaiseChanged(entry, PluginChangedKind.Disabled);
            return true;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// 卸载插件：先禁用（走完 7 步），再删除插件目录与状态记录。
    /// </summary>
    /// <param name="pluginId">插件标识。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>是否卸载成功。</returns>
    public async Task<bool> UninstallAsync(string pluginId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginId);

        if (!_entries.TryGetValue(pluginId, out var entry) || entry.IsBuiltin)
            return false;

        if (!await DisableAsync(pluginId, cancellationToken).ConfigureAwait(false))
            return false;

        var directory = entry.Directory;

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _entries.Remove(pluginId);
        }
        finally
        {
            _gate.Release();
        }

        if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
        {
            try
            {
                Directory.Delete(directory, recursive: true);
            }
            catch (Exception ex)
            {
                Logger.Warn($"删除插件目录失败：{directory}", ex);
            }
        }

        await _stateRepository.DeleteAsync(pluginId, cancellationToken).ConfigureAwait(false);
        RaiseChanged(entry, PluginChangedKind.Uninstalled);
        return true;
    }

    /// <summary>
    /// 升级（热替换）插件：对旧版本执行完整卸载流程，<b>卸载验证通过才</b>复制新包并重新启用（PRD §3.5 / §6 #21）。
    /// </summary>
    /// <param name="pluginId">插件标识。</param>
    /// <param name="sourceDirectory">新版本插件源目录（含 plugin.json，id 必须与 <paramref name="pluginId"/> 一致）。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>是否升级成功。</returns>
    /// <remarks>
    /// 卸载验证未通过时<b>中止安装</b>并返回 <c>false</c>：不允许在降级态下继续加载新版本，
    /// 否则每升级一次就会多泄漏一份程序集。
    /// </remarks>
    public async Task<bool> UpgradeAsync(
        string pluginId,
        string sourceDirectory,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginId);

        if (string.IsNullOrWhiteSpace(sourceDirectory) || !Directory.Exists(sourceDirectory))
            return false;

        PluginEntry entry;
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!_entries.TryGetValue(pluginId, out var found) || found.IsBuiltin)
            {
                Logger.Warn($"升级插件失败：未登记的插件或内置插件 {pluginId}");
                return false;
            }

            entry = found;
        }
        finally
        {
            _gate.Release();
        }

        // 新版本清单先行校验（避免先卸载了旧版本、却发现新包根本不可用）
        var manifest = _loader.ReadManifest(sourceDirectory);
        if (manifest is null || string.IsNullOrWhiteSpace(manifest.Id))
            return false;

        if (!string.Equals(manifest.Id, pluginId, StringComparison.OrdinalIgnoreCase))
        {
            Logger.Warn($"升级插件失败：源目录清单 id（{manifest.Id}）与目标插件（{pluginId}）不一致。");
            return false;
        }

        var validation = _loader.Validate(manifest, sourceDirectory);
        if (!validation.IsValid)
        {
            Logger.Warn($"升级插件失败（{pluginId}）：{validation.ToSummary()}");
            return false;
        }

        var wasEnabled = entry.State == PluginState.Enabled;

        // ① 完整卸载流程（旧版本处于启用态或仍持有 ALC 时）
        if (wasEnabled || entry.Loaded is not null)
        {
            if (!await DisableAsync(pluginId, cancellationToken).ConfigureAwait(false))
                return false;

            // ② 卸载验证未通过 → 中止安装（PRD v0.18：不允许在降级态下加载新版本）
            if (entry.RequiresRestart)
            {
                Logger.Warn(
                    $"升级插件中止（{pluginId}）：旧版本程序集未能完全释放，请重启应用后再安装新版本。");
                return false;
            }
        }

        // ③ 复制新包（先清空旧目录，避免旧版本依赖残留）
        var target = entry.Directory ?? Path.Combine(PluginLocation.BuiltinRoot, pluginId);

        ManifestValidationResult? newValidation;
        try
        {
            if (Directory.Exists(target))
                Directory.Delete(target, recursive: true);

            CopyDirectory(sourceDirectory, target);
            newValidation = _loader.Validate(
                _loader.ReadManifest(target) ?? manifest,
                target);
        }
        catch (Exception ex)
        {
            Logger.Error($"升级插件失败（{pluginId}）：复制插件包出错。", ex);
            return false;
        }

        if (newValidation is not null && !newValidation.IsValid)
        {
            Logger.Warn($"升级插件失败（{pluginId}）：新目录校验失败（{newValidation.ToSummary()}）。");
            return false;
        }

        // ④ 刷新登记（新目录 + 新清单 + 回到 Installed）
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            entry.Directory = target;
            entry.Manifest = _loader.ReadManifest(target) ?? manifest;
            entry.Loaded = null;
            entry.Scope = null;
            entry.State = PluginState.Installed;
            entry.RequiresRestart = false;
            entry.LastError = null;
            entry.ShouldEnableOnStartup = false;
            entry.RegisteredSidebarEntries.Clear();
            entry.RegisteredSkillIds.Clear();
            entry.RegisteredMcpServers.Clear();

            await PersistAsync(entry, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }

        RaiseChanged(entry, PluginChangedKind.Upgraded);

        // ⑤ 升级前处于启用态 → 重新启用新版本
        if (wasEnabled && !await EnableAsync(pluginId, cancellationToken).ConfigureAwait(false))
        {
            Logger.Warn($"升级插件后重新启用失败：{pluginId}");
            return false;
        }

        Logger.Info($"插件 {pluginId} 已升级至 {entry.Manifest?.Version}。");
        return true;
    }

    /// <summary>
    /// 安装插件：把源目录复制到插件根目录（应用级）后登记（PRD §3.3）。
    /// </summary>
    /// <param name="sourceDirectory">插件源目录（含 plugin.json）。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>安装后的插件标识；失败返回 <c>null</c>。</returns>
    public async Task<string?> InstallAsync(string sourceDirectory, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourceDirectory) || !Directory.Exists(sourceDirectory))
            return null;

        var manifest = _loader.ReadManifest(sourceDirectory);
        if (manifest is null || string.IsNullOrWhiteSpace(manifest.Id))
            return null;

        var validation = _loader.Validate(manifest, sourceDirectory);
        if (!validation.IsValid)
        {
            Logger.Warn($"安装插件失败（{manifest.Id}）：{validation.ToSummary()}");
            return null;
        }

        var target = Path.Combine(PluginLocation.BuiltinRoot, manifest.Id);

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (Directory.Exists(target))
                Directory.Delete(target, recursive: true);

            CopyDirectory(sourceDirectory, target);

            _entries[manifest.Id] = new PluginEntry
            {
                Id = manifest.Id,
                IsBuiltin = false,
                Directory = target,
                Manifest = manifest,
                State = PluginState.Installed
            };
        }
        catch (Exception ex)
        {
            Logger.Error($"安装插件失败：{manifest.Id}", ex);
            return null;
        }
        finally
        {
            _gate.Release();
        }

        var record = new PluginStateRecord
        {
            Id = manifest.Id,
            Version = manifest.Version,
            State = PluginState.Installed
        };
        await _stateRepository.SaveAsync(record, cancellationToken).ConfigureAwait(false);

        if (_entries.TryGetValue(manifest.Id, out var installed))
            RaiseChanged(installed, PluginChangedKind.Installed);

        return manifest.Id;
    }

    #region 激活

    /// <summary>
    /// 激活目录插件（加载 → UI → 初始化 → Skill / MCP）。
    /// </summary>
    private async Task<bool> ActivateDirectoryPluginAsync(
        PluginEntry entry,
        IPluginHost host,
        CancellationToken cancellationToken)
    {
        LoadedPlugin? loaded = null;
        try
        {
            // 第 3–4 步：可回收 ALC + 入口程序集 + IPlugin 实例
            loaded = _loader.Load(entry.Directory!);
            entry.Manifest ??= loaded.Manifest;
        }
        catch (PluginLoadException ex)
        {
            Logger.Error($"加载插件失败：{entry.Id}", ex);
            entry.State = PluginState.Error;
            entry.LastError = ex.Message;
            await PersistAsync(entry, cancellationToken).ConfigureAwait(false);
            RaiseChanged(entry, PluginChangedKind.Error);
            return false;
        }

        entry.Loaded = loaded;
        entry.State = PluginState.Installed;

        var scope = new PluginHostScope(host, entry.Id);
        entry.Scope = scope;

        // 第 6 步：先按清单登记侧栏条目，保证 UI 有入口
        RegisterManifestUi(entry, scope);

        // 第 5 步：插件主程序初始化（隔离异常）
        try
        {
            await loaded.Plugin.InitializeAsync(scope, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.Error($"插件初始化失败：{entry.Id}", ex);
            await RollbackActivationAsync(entry, scope, cancellationToken).ConfigureAwait(false);
            entry.State = PluginState.Error;
            entry.LastError = ex.Message;
            await PersistAsync(entry, cancellationToken).ConfigureAwait(false);
            RaiseChanged(entry, PluginChangedKind.Error);
            return false;
        }

        // 插件可能在初始化中自行注册条目（§3.4 第 5 步），此处并入记账集合
        MergeTrackedContributions(entry, scope);

        // 第 7 步：注册 Skill（§4.10）与 MCP（§4.11）
        RegisterSkills(entry, scope, loaded.Plugin);
        RegisterMcpServers(entry, scope);

        entry.State = PluginState.Enabled;
        entry.ShouldEnableOnStartup = true;
        entry.LastError = null;
        entry.RequiresRestart = false;

        await PersistAsync(entry, cancellationToken).ConfigureAwait(false);
        RaiseChanged(entry, PluginChangedKind.Enabled);

        Logger.Info($"[插件 {entry.Id}] 已启用（版本 {entry.Manifest?.Version}）。");
        return true;
    }

    /// <summary>
    /// 激活内置插件（无 ALC，只做初始化与贡献登记）。
    /// </summary>
    private async Task<bool> ActivateBuiltinAsync(
        PluginEntry entry,
        IPluginHost host,
        CancellationToken cancellationToken)
    {
        var plugin = entry.BuiltinPlugin!;
        var scope = new PluginHostScope(host, entry.Id);
        entry.Scope = scope;

        try
        {
            await plugin.InitializeAsync(scope, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.Error($"内置插件初始化失败：{entry.Id}", ex);
            entry.State = PluginState.Error;
            entry.LastError = ex.Message;
            RaiseChanged(entry, PluginChangedKind.Error);
            return false;
        }

        MergeTrackedContributions(entry, scope);
        RegisterSkills(entry, scope, plugin);

        entry.State = PluginState.Enabled;
        entry.ShouldEnableOnStartup = true;
        entry.LastError = null;

        await PersistAsync(entry, cancellationToken).ConfigureAwait(false);
        RaiseChanged(entry, PluginChangedKind.Enabled);
        return true;
    }

    /// <summary>
    /// 注册清单声明的侧栏条目。
    /// </summary>
    private static void RegisterManifestUi(PluginEntry entry, IPluginHost scope)
    {
        foreach (var contribution in entry.Manifest?.UiContributions ?? new List<PluginSidebarEntry>())
        {
            try
            {
                scope.RegisterSidebarEntry(contribution);
            }
            catch (Exception ex)
            {
                Logger.Warn($"[插件 {entry.Id}] 注册侧栏条目失败：{contribution.Id}", ex);
            }
        }
    }

    /// <summary>
    /// 把插件经包装自行注册的贡献并入条目记账集合（供卸载流程精确清理）。
    /// </summary>
    private static void MergeTrackedContributions(PluginEntry entry, PluginHostScope scope)
    {
        foreach (var entryId in scope.RegisteredSidebarEntries)
            entry.RegisteredSidebarEntries.Add(entryId);

        foreach (var skillId in scope.RegisteredSkillIds)
            entry.RegisteredSkillIds.Add(skillId);

        foreach (var serverName in scope.RegisteredMcpServers)
            entry.RegisteredMcpServers.Add(serverName);
    }

    /// <summary>
    /// 注册插件贡献的 Skill，并记录 id 以便卸载时反注册。
    /// </summary>
    private static void RegisterSkills(PluginEntry entry, IPluginHost scope, IPlugin plugin)
    {
        IEnumerable<SkillDefinition> definitions;
        try
        {
            definitions = plugin.ContributeSkills() ?? Array.Empty<SkillDefinition>();
        }
        catch (Exception ex)
        {
            Logger.Error($"[插件 {entry.Id}] 读取技能贡献失败", ex);
            return;
        }

        // 合并「怎么说」的两处来源：清单声明 + 运行时贡献（清单优先，按 id 去重）
        var merged = new Dictionary<string, SkillDefinition>(StringComparer.OrdinalIgnoreCase);
        foreach (var definition in entry.Manifest?.Skills ?? new List<SkillDefinition>())
        {
            if (!string.IsNullOrWhiteSpace(definition.Id))
                merged[definition.Id] = definition;
        }

        foreach (var definition in definitions)
        {
            if (!string.IsNullOrWhiteSpace(definition.Id))
                merged.TryAdd(definition.Id, definition);
        }

        foreach (var (skillId, definition) in merged)
        {
            try
            {
                scope.RegisterSkill(definition);
                entry.RegisteredSkillIds.Add(skillId);
            }
            catch (Exception ex)
            {
                Logger.Error($"[插件 {entry.Id}] 注册技能失败：{skillId}", ex);
            }
        }
    }

    /// <summary>
    /// 注册插件启用的内置 MCP 服务器，并记录名称以便卸载时反注册。
    /// </summary>
    private static void RegisterMcpServers(PluginEntry entry, IPluginHost scope)
    {
        foreach (var server in entry.Manifest?.EnabledMcpServers ?? Enumerable.Empty<PluginMcpServerDeclaration>())
        {
            try
            {
                scope.RegisterMcpServer(server.Name);
                entry.RegisteredMcpServers.Add(server.Name);
            }
            catch (Exception ex)
            {
                Logger.Error($"[插件 {entry.Id}] 注册 MCP 服务器失败：{server.Name}", ex);
            }
        }
    }

    /// <summary>
    /// 初始化失败时回滚本次激活产生的贡献（UI / Skill / MCP）并回收 ALC。
    /// </summary>
    private static async Task RollbackActivationAsync(
        PluginEntry entry,
        IPluginHost scope,
        CancellationToken cancellationToken)
    {
        UnregisterContributions(entry, scope);

        var loaded = entry.Loaded;
        entry.Loaded = null;
        entry.Scope = null;
        if (loaded is null)
            return;

        try
        {
            await loaded.Plugin.ShutdownAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.Warn($"[插件 {entry.Id}] 回滚时关闭插件出错", ex);
        }

        try
        {
            loaded.LoadContext.Unload();
        }
        catch (Exception ex)
        {
            Logger.Warn($"[插件 {entry.Id}] 回滚时卸载 ALC 出错", ex);
        }
    }

    #endregion

    #region 卸载（§3.12 七步）

    /// <summary>
    /// 执行 PRD §3.12 的七步卸载流程。
    /// </summary>
    /// <param name="entry">插件条目。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>程序集是否确认回收（内置插件恒为 <c>true</c>）。</returns>
    private async Task<bool> UnloadAsync(PluginEntry entry, CancellationToken cancellationToken)
    {
        var host = ResolveHost();
        var scope = entry.Scope ?? (host is null ? null : new PluginHostScope(host, entry.Id));

        // 第 1 步：通知插件关闭（退订事件、停定时器、释句柄、取消后台任务）。
        // 注意：本方法运行在 async 状态机上，任何跨越 await 存活的对 LoadedPlugin/ALC 的局部引用
        // 都会让第 6 步的弱引用验证永远失败，故所有强引用都收敛在同步辅助方法内部。
        if (entry.Loaded is not null || entry.BuiltinPlugin is not null)
            await ShutdownPluginAsync(entry, cancellationToken).ConfigureAwait(false);

        // 第 2 步：注销 UI（侧栏条目 + 面板）—— 遍历「实际注册过的」集合，而非仅清单声明
        if (scope is not null)
        {
            foreach (var entryId in entry.RegisteredSidebarEntries.ToArray())
            {
                SafeHostAction(() => scope.UnregisterSidebarEntry(entryId), entry.Id, "注销侧栏条目");
                SafeHostAction(() => scope.UnregisterPanel(entryId), entry.Id, "注销面板");
            }

            entry.RegisteredSidebarEntries.Clear();
        }

        // 第 3 步：注销 Skill / MCP —— 必须先于 ALC 卸载
        // 第 4 步：删除工作区文件式 Skill / MCP —— 由宿主 Unregister* 实现内完成（写文件 + 即时重载）
        if (scope is not null)
            UnregisterContributions(entry, scope);

        entry.Scope = null;

        // 内置插件无 ALC，流程到此结束
        if (entry.IsBuiltin || entry.Loaded is null)
        {
            entry.Loaded = null;
            return true;
        }

        // 第 5 步：丢弃全部强引用 → 卸载 ALC（在独立同步方法内完成，确保帧及时出栈）
        var probe = DetachAndUnload(entry);

        // 第 6 步：多轮 GC + 弱引用验证
        var collected = await VerifyCollectedAsync(entry.Id, probe, cancellationToken).ConfigureAwait(false);

        // 第 7 步：未通过则降级并记诊断日志（由调用方落库为 Disabled + RequiresRestart）
        if (!collected)
        {
            Logger.Warn(
                $"[插件 {entry.Id}] 程序集卸载验证未通过：仍有外部强引用，已降级为禁用；" +
                "请检查是否有未注销的事件订阅、缓存或静态引用（PRD §3.12 第 7 步）。");
        }

        return collected;
    }

    /// <summary>
    /// 第 5 步：捕获弱引用、丢弃 <see cref="PluginEntry.Loaded"/> 强引用并卸载 ALC。
    /// </summary>
    /// <remarks>
    /// 必须是<b>非 async 且标记 <see cref="MethodImplOptions.NoInlining"/></b> 的独立方法：
    /// 一旦本方法返回，其栈帧即失效，<c>loaded</c> / <c>loadContext</c> 等局部强引用随帧消失，
    /// 随后在调用方执行的 GC 验证才有意义（否则弱引用恒为存活）。
    /// </remarks>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static UnloadProbe DetachAndUnload(PluginEntry entry)
    {
        var loaded = entry.Loaded;
        if (loaded is null)
            return new UnloadProbe();

        var probe = new UnloadProbe
        {
            LoadContextRef = new WeakReference(loaded.LoadContext),
            PluginRef = new WeakReference(loaded.Plugin),
            AssemblyRef = new WeakReference(loaded.Assembly),
            PluginTypeRef = new WeakReference(loaded.PluginType)
        };

        entry.Loaded = null;

        try
        {
            loaded.LoadContext.Unload();
        }
        catch (Exception ex)
        {
            Logger.Warn($"[插件 {entry.Id}] ALC 卸载调用失败", ex);
        }

        return probe;
    }

    /// <summary>
    /// 第 1 步：调用插件 <c>ShutdownAsync</c>（带超时与异常隔离）。
    /// </summary>
    /// <remarks>
    /// 内置插件没有 <see cref="PluginEntry.Loaded"/>（不来自 ALC），故实例从
    /// <see cref="PluginEntry.BuiltinPlugin"/> 取；两类插件都必须收到关闭通知（PRD §3.5 第 1 步）。
    /// </remarks>
    private async Task ShutdownPluginAsync(
        PluginEntry entry,
        CancellationToken cancellationToken)
    {
        var plugin = entry.Loaded?.Plugin ?? entry.BuiltinPlugin;
        if (plugin is null)
            return;

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(_options.ShutdownTimeoutSeconds));

        var stopwatch = Stopwatch.StartNew();
        try
        {
            await plugin.ShutdownAsync(timeout.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (timeout.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            Logger.Warn($"[插件 {entry.Id}] ShutdownAsync 超时（>{_options.ShutdownTimeoutSeconds}s），继续卸载流程。");
        }
        catch (Exception ex)
        {
            Logger.Error($"[插件 {entry.Id}] ShutdownAsync 抛出异常，继续卸载流程", ex);
        }
        finally
        {
            stopwatch.Stop();
            if (stopwatch.ElapsedMilliseconds > 500)
                Logger.Debug($"[插件 {entry.Id}] ShutdownAsync 耗时 {stopwatch.ElapsedMilliseconds}ms");
        }
    }

    /// <summary>
    /// 反注册插件贡献的 Skill 与 MCP 服务器。
    /// </summary>
    private static void UnregisterContributions(PluginEntry entry, IPluginHost scope)
    {
        foreach (var skillId in entry.RegisteredSkillIds.ToList())
        {
            SafeHostAction(() => scope.UnregisterSkill(skillId), entry.Id, $"注销技能 {skillId}");
        }
        entry.RegisteredSkillIds.Clear();

        foreach (var serverName in entry.RegisteredMcpServers.ToList())
        {
            SafeHostAction(() => scope.UnregisterMcpServer(serverName), entry.Id, $"注销 MCP {serverName}");
        }
        entry.RegisteredMcpServers.Clear();
    }

    /// <summary>
    /// 第 6 步：多轮 GC + 弱引用验证（ALC / 实例 / 程序集 / 类型均需死亡才算回收）。
    /// </summary>
    private async Task<bool> VerifyCollectedAsync(
        string pluginId,
        UnloadProbe probe,
        CancellationToken cancellationToken)
    {
        var references = probe.All();
        var rounds = Math.Max(1, _options.UnloadVerificationRounds);

        for (var round = 0; round < rounds; round++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            if (references.All(reference => !reference.IsAlive))
            {
                Logger.Debug($"[插件 {pluginId}] 程序集已回收（第 {round + 1} 轮 GC）。");
                return true;
            }

            await Task.Delay(50, cancellationToken).ConfigureAwait(false);
        }

        return false;
    }

    #endregion

    #region 辅助

    /// <summary>
    /// 递归确保依赖插件已启用。
    /// </summary>
    private async Task<bool> EnsureDependenciesEnabledAsync(PluginEntry entry, CancellationToken cancellationToken)
    {
        foreach (var dependency in entry.Manifest?.Dependencies ?? new List<string>())
        {
            if (string.IsNullOrWhiteSpace(dependency))
                continue;

            if (!_entries.TryGetValue(dependency, out var dependencyEntry))
            {
                Logger.Warn($"[插件 {entry.Id}] 依赖缺失：{dependency}");
                return false;
            }

            if (dependencyEntry.State != PluginState.Enabled)
            {
                Logger.Warn($"[插件 {entry.Id}] 依赖插件未启用，请先启用：{dependency}");
                return false;
            }
        }

        await Task.CompletedTask.ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// 延迟解析宿主能力接口。
    /// </summary>
    private IPluginHost? ResolveHost() => _sharedHost ??= _services.GetService<IPluginHost>();

    /// <summary>
    /// 标记插件为 Error 并持久化。
    /// </summary>
    private async Task MarkErrorAsync(PluginEntry entry, string message, CancellationToken cancellationToken)
    {
        entry.State = PluginState.Error;
        entry.LastError = message;
        await PersistAsync(entry, cancellationToken).ConfigureAwait(false);
        RaiseChanged(entry, PluginChangedKind.Error);
    }

    /// <summary>
    /// 持久化插件状态。
    /// </summary>
    /// <remarks>
    /// 内置插件同样持久化（PRD §3.5 未对内置插件作例外）：否则「禁用后重启仍禁用」「开机恢复」在
    /// 内置插件上失效，且 <see cref="ActivateBuiltinAsync"/> 的持久化写入会变成死代码。
    /// </remarks>
    private async Task PersistAsync(PluginEntry entry, CancellationToken cancellationToken)
    {
        var existing = await _stateRepository.GetAsync(entry.Id, cancellationToken).ConfigureAwait(false);

        var record = new PluginStateRecord
        {
            Id = entry.Id,
            Version = entry.Manifest?.Version ?? entry.BuiltinPlugin?.Version,
            State = entry.State,
            InstalledAt = existing?.InstalledAt ?? DateTimeOffset.Now,
            LastError = entry.LastError,
            RequiresRestart = entry.RequiresRestart
        };

        await _stateRepository.SaveAsync(record, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 触发状态变更事件（事件订阅方异常不影响管理器）。
    /// </summary>
    private void RaiseChanged(PluginEntry entry, PluginChangedKind kind)
    {
        try
        {
            Changed?.Invoke(this, new PluginChangedEventArgs(entry.ToDescriptor(), kind));
        }
        catch (Exception ex)
        {
            Logger.Warn($"插件状态变更事件处理失败：{entry.Id}", ex);
        }
    }

    /// <summary>
    /// 安全执行宿主侧操作（吞掉异常并记录）。
    /// </summary>
    private static void SafeHostAction(Action action, string pluginId, string description)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            Logger.Warn($"[插件 {pluginId}] {description} 失败", ex);
        }
    }

    /// <summary>
    /// 递归复制目录。
    /// </summary>
    private static void CopyDirectory(string source, string target)
    {
        Directory.CreateDirectory(target);

        foreach (var file in Directory.GetFiles(source))
            File.Copy(file, Path.Combine(target, Path.GetFileName(file)), overwrite: true);

        foreach (var directory in Directory.GetDirectories(source))
            CopyDirectory(directory, Path.Combine(target, Path.GetFileName(directory)));
    }

    #endregion

    /// <summary>
    /// 卸载探针：仅持弱引用，用于第 6 步验证程序集是否真正被回收。
    /// </summary>
    private sealed class UnloadProbe
    {
        public WeakReference? LoadContextRef { get; set; }
        public WeakReference? PluginRef { get; set; }
        public WeakReference? AssemblyRef { get; set; }
        public WeakReference? PluginTypeRef { get; set; }

        /// <summary>
        /// 取全部非空弱引用。
        /// </summary>
        public WeakReference[] All()
        {
            var result = new List<WeakReference>(4);
            if (LoadContextRef is not null) result.Add(LoadContextRef);
            if (PluginRef is not null) result.Add(PluginRef);
            if (AssemblyRef is not null) result.Add(AssemblyRef);
            if (PluginTypeRef is not null) result.Add(PluginTypeRef);
            return result.ToArray();
        }
    }

    /// <summary>
    /// 插件条目：登记信息 + 运行期句柄。
    /// </summary>
    private sealed class PluginEntry
    {
        public string Id { get; init; } = string.Empty;
        public bool IsBuiltin { get; init; }
        public string? Directory { get; set; }
        public PluginManifest? Manifest { get; set; }
        public IPlugin? BuiltinPlugin { get; init; }
        public LoadedPlugin? Loaded { get; set; }
        public PluginHostScope? Scope { get; set; }
        public PluginState State { get; set; } = PluginState.Installed;
        public string? LastError { get; set; }
        public bool RequiresRestart { get; set; }
        public bool ShouldEnableOnStartup { get; set; }
        public HashSet<string> RegisteredSidebarEntries { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> RegisteredSkillIds { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> RegisteredMcpServers { get; } = new(StringComparer.OrdinalIgnoreCase);

        public PluginDescriptor ToDescriptor() => new()
        {
            Id = Id,
            Name = Manifest?.Name ?? BuiltinPlugin?.Name ?? Id,
            Version = Manifest?.Version ?? BuiltinPlugin?.Version ?? string.Empty,
            Author = Manifest?.Author,
            Description = Manifest?.Description ?? BuiltinPlugin?.Description,
            State = State,
            IsBuiltin = IsBuiltin,
            Directory = Directory,
            LastError = LastError,
            RequiresRestart = RequiresRestart,
            Manifest = Manifest,
            Permissions = Manifest?.Permissions ?? new PluginPermissions(),
            AssemblyLocation = Loaded?.Assembly.Location
        };
    }
}

/// <summary>
/// 插件描述（宿主管理器界面展示用，PRD §3.9）。
/// </summary>
public sealed class PluginDescriptor
{
    /// <summary>插件标识。</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>展示名。</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>版本。</summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>作者。</summary>
    public string? Author { get; init; }

    /// <summary>简介。</summary>
    public string? Description { get; init; }

    /// <summary>当前状态。</summary>
    public PluginState State { get; init; }

    /// <summary>是否内置插件。</summary>
    public bool IsBuiltin { get; init; }

    /// <summary>插件目录（内置插件为 <c>null</c>）。</summary>
    public string? Directory { get; init; }

    /// <summary>最近一次错误信息。</summary>
    public string? LastError { get; init; }

    /// <summary>是否需要重启才能彻底生效。</summary>
    public bool RequiresRestart { get; init; }

    /// <summary>清单全量信息。</summary>
    public PluginManifest? Manifest { get; init; }

    /// <summary>权限声明。</summary>
    public PluginPermissions Permissions { get; init; } = new();

    /// <summary>实际加载的程序集位置（ALC 加载后填写）。</summary>
    public string? AssemblyLocation { get; init; }
}

/// <summary>
/// 插件变更事件参数。
/// </summary>
public sealed class PluginChangedEventArgs : EventArgs
{
    /// <summary>
    /// 创建事件参数。
    /// </summary>
    /// <param name="plugin">插件描述（变更后的快照）。</param>
    /// <param name="kind">变更类型。</param>
    public PluginChangedEventArgs(PluginDescriptor plugin, PluginChangedKind kind)
    {
        Plugin = plugin;
        Kind = kind;
    }

    /// <summary>插件描述（变更后的快照）。</summary>
    public PluginDescriptor Plugin { get; }

    /// <summary>变更类型。</summary>
    public PluginChangedKind Kind { get; }
}

/// <summary>
/// 插件变更类型。
/// </summary>
public enum PluginChangedKind
{
    /// <summary>已安装。</summary>
    Installed,

    /// <summary>已启用。</summary>
    Enabled,

    /// <summary>已禁用。</summary>
    Disabled,

    /// <summary>已卸载。</summary>
    Uninstalled,

    /// <summary>已升级（热替换成功）。</summary>
    Upgraded,

    /// <summary>进入错误态。</summary>
    Error
}

/// <summary>
/// 插件管理器选项。
/// </summary>
public sealed class PluginManagerOptions
{
    /// <summary>
    /// 插件 <c>ShutdownAsync</c> 超时秒数（超过后继续卸载流程，默认 5 秒）。
    /// </summary>
    public int ShutdownTimeoutSeconds { get; set; } = 5;

    /// <summary>
    /// 卸载后 GC 验证轮数（默认 8 轮）。
    /// </summary>
    public int UnloadVerificationRounds { get; set; } = 8;
}