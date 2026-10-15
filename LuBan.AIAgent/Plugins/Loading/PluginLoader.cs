/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Plugins.Loading
*文件名： PluginLoader
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/15
*描述：插件加载器 - 清单→校验→ALC→程序集→IPlugin 实例（PRD §3.4 加载流程第 1-4 步）
*
*****************************************************************************/
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;

namespace LuBan.AIAgent.Plugins.Loading;

/// <summary>
/// 插件加载器：执行 PRD §3.4 加载流程第 1–4 步。
/// </summary>
/// <remarks>
/// 第 5–7 步（InitializeAsync、注册 UI、注册 Skill / MCP）由 <c>PluginManager</c> 承担，
/// 以便把「加载」与「激活」解耦：加载失败可安全回滚，激活失败可保留文件并置 <c>Error</c>。
/// </remarks>
public sealed class PluginLoader
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private readonly ManifestValidator _validator;
    private readonly PluginFactory _factory;

    /// <summary>
    /// 创建插件加载器。
    /// </summary>
    /// <param name="validator">清单校验器。</param>
    /// <param name="factory">插件实例工厂。</param>
    public PluginLoader(ManifestValidator validator, PluginFactory factory)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <summary>
    /// 读取插件目录下的 <c>plugin.json</c>。
    /// </summary>
    /// <param name="pluginDirectory">插件目录。</param>
    /// <returns>清单对象；文件缺失或解析失败时返回 <c>null</c>。</returns>
    public PluginManifest? ReadManifest(string pluginDirectory)
    {
        if (string.IsNullOrWhiteSpace(pluginDirectory))
            return null;

        var manifestPath = PluginLocation.GetManifestPath(pluginDirectory);
        if (!File.Exists(manifestPath))
        {
            Logger.Warn($"插件清单不存在：{manifestPath}");
            return null;
        }

        try
        {
            var json = File.ReadAllText(manifestPath);
            return JsonSerializer.Deserialize<PluginManifest>(json, JsonOpts);
        }
        catch (Exception ex)
        {
            Logger.Error($"插件清单解析失败：{manifestPath}", ex);
            return null;
        }
    }

    /// <summary>
    /// 校验清单。
    /// </summary>
    /// <param name="manifest">清单对象。</param>
    /// <param name="pluginDirectory">插件目录。</param>
    /// <returns>校验结果。</returns>
    public ManifestValidationResult Validate(PluginManifest? manifest, string pluginDirectory)
        => _validator.Validate(manifest, pluginDirectory);

    /// <summary>
    /// 加载插件：清单 → 校验 → 可回收 ALC → 入口程序集 → <see cref="IPlugin"/> 实例。
    /// </summary>
    /// <param name="pluginDirectory">插件目录。</param>
    /// <returns>加载结果。</returns>
    /// <exception cref="PluginLoadException">任一步骤失败。</exception>
    public LoadedPlugin Load(string pluginDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginDirectory);

        var manifest = ReadManifest(pluginDirectory)
            ?? throw new PluginLoadException($"插件清单缺失或无法解析：{pluginDirectory}");

        var validation = Validate(manifest, pluginDirectory);
        if (!validation.IsValid)
            throw new PluginLoadException(validation.ToSummary(), manifest.Id);

        foreach (var warning in validation.Warnings)
            Logger.Warn($"[插件 {manifest.Id}] 清单警告：{warning}");

        var entryAssemblyPath = PluginLocation.ResolveEntryAssemblyPath(pluginDirectory, manifest.EntryAssembly)
            ?? throw new PluginLoadException($"入口程序集不存在：{manifest.EntryAssembly}", manifest.Id);

        // ALC 创建失败无需卸载，直接抛出。
        var loadContext = new PluginLoadContext(manifest.Id, entryAssemblyPath, collectible: true);

        try
        {
            var assembly = loadContext.LoadFromAssemblyPath(entryAssemblyPath);

            var pluginType = _factory.ResolvePluginType(assembly, manifest.Id)
                ?? throw new PluginLoadException(
                    $"入口程序集中未找到唯一的 IPlugin 实现（且无法按 id 消歧）：{manifest.EntryAssembly}",
                    manifest.Id);

            var plugin = _factory.Instantiate(pluginType);

            return new LoadedPlugin(manifest, pluginDirectory, loadContext, assembly, pluginType, plugin);
        }
        catch (PluginLoadException)
        {
            TryUnload(loadContext);
            throw;
        }
        catch (Exception ex)
        {
            TryUnload(loadContext);
            throw new PluginLoadException($"加载插件 {manifest.Id} 失败：{ex.Message}", manifest.Id, ex);
        }
    }

    /// <summary>
    /// 加载失败时尽力回收 ALC，避免留下不可卸载的上下文。
    /// </summary>
    private static void TryUnload(AssemblyLoadContext loadContext)
    {
        try
        {
            loadContext.Unload();
        }
        catch (Exception ex)
        {
            Logger.Warn($"加载失败后回收插件 ALC 出错：{loadContext.Name}", ex);
        }
    }
}

/// <summary>
/// 加载完成的插件：持有 ALC、程序集与实例。
/// </summary>
/// <remarks>
/// 调用方在卸载时必须<b>先丢弃本对象全部引用</b>，再调用 <see cref="AssemblyLoadContext.Unload"/>，
/// 否则 <see cref="LoadContext"/> / <see cref="Assembly"/> / <see cref="Plugin"/> 均是强引用，
/// 会导致程序集无法回收（PRD §3.12）。
/// </remarks>
public sealed class LoadedPlugin
{
    /// <summary>
    /// 创建加载结果。
    /// </summary>
    public LoadedPlugin(
        PluginManifest manifest,
        string directory,
        PluginLoadContext loadContext,
        Assembly assembly,
        Type pluginType,
        IPlugin plugin)
    {
        Manifest = manifest;
        Directory = directory;
        LoadContext = loadContext;
        Assembly = assembly;
        PluginType = pluginType;
        Plugin = plugin;
    }

    /// <summary>插件清单。</summary>
    public PluginManifest Manifest { get; }

    /// <summary>插件目录。</summary>
    public string Directory { get; }

    /// <summary>插件加载上下文（可回收）。</summary>
    public PluginLoadContext LoadContext { get; }

    /// <summary>入口程序集。</summary>
    public Assembly Assembly { get; }

    /// <summary>插件类型。</summary>
    public Type PluginType { get; }

    /// <summary>插件实例。</summary>
    public IPlugin Plugin { get; }
}

/// <summary>
/// 插件加载异常。
/// </summary>
public sealed class PluginLoadException : Exception
{
    /// <summary>
    /// 创建插件加载异常。
    /// </summary>
    /// <param name="message">错误描述。</param>
    public PluginLoadException(string message) : base(message)
    {
    }

    /// <summary>
    /// 创建插件加载异常。
    /// </summary>
    /// <param name="message">错误描述。</param>
    /// <param name="pluginId">插件标识。</param>
    public PluginLoadException(string message, string? pluginId) : base(message)
    {
        PluginId = pluginId;
    }

    /// <summary>
    /// 创建插件加载异常。
    /// </summary>
    /// <param name="message">错误描述。</param>
    /// <param name="pluginId">插件标识。</param>
    /// <param name="innerException">内部异常。</param>
    public PluginLoadException(string message, string? pluginId, Exception innerException)
        : base(message, innerException)
    {
        PluginId = pluginId;
    }

    /// <summary>
    /// 插件标识（可空）。
    /// </summary>
    public string? PluginId { get; }
}