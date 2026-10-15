/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Plugins.Lifecycle
*文件名： PluginLoadContext
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/15
*描述：可回收插件程序集加载上下文（ALC 隔离，PRD §3.4 方案 B）
*
*****************************************************************************/
using System.Reflection;
using System.Runtime.Loader;

namespace LuBan.AIAgent.Plugins.Lifecycle;

/// <summary>
/// 可回收（collectible）插件程序集加载上下文：实现按插件隔离的依赖解析与卸载（PRD §3.4 方案 B）。
/// </summary>
/// <remarks>
/// <para>
/// 关键约束：
/// <list type="number">
/// <item>共享框架集（契约、Avalonia、<c>LuBan.*</c>、<c>Microsoft.*</c>、<c>System.*</c>）一律回落默认 ALC，
/// 否则同一类型在两处产生两个 <c>Type</c> 实例，会导致 <c>IPlugin</c> 转换失败；</item>
/// <item>插件私有依赖（<c>lib/</c> 内的第三方库）由 <see cref="AssemblyDependencyResolver"/>
/// 解析进本 ALC，实现版本隔离；</item>
/// <item><c>isCollectible: true</c> 是本 ALC 可卸载的前提；卸载可行性还要求外部不留强引用（见 §3.12）。</item>
/// </list>
/// </para>
/// </remarks>
public sealed class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    /// <summary>
    /// 共享程序集前缀：命中则回落默认 ALC（禁止插件自带副本）。
    /// </summary>
    /// <remarks>
    /// 「契约程序集置于默认 ALC，不随插件包分发」（PRD §3.3），此处以「总是共享」强制该约定。
    /// </remarks>
    private static readonly string[] SharedPrefixes =
    {
        "LuBan.",
        "Microsoft.",
        "System.",
        "netstandard",
        "mscorlib",
        "Avalonia",
        "WindowsBase",
        "System.Runtime"
    };

    /// <summary>
    /// 共享程序集精确名（无命名空间前缀或需显式列出的）。
    /// </summary>
    private static readonly HashSet<string> SharedExactNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "LuBan.AIAgent",
        "LuBan.Common",
        "LuBan.Logging",
        "LuBan.DI",
        "LuBan.Orm",
        "LuBan.Threading",
        "Avalonia",
        "Telegram.Bot",
        "CommunityToolkit.Mvvm"
    };

    /// <summary>
    /// 插件标识。
    /// </summary>
    public string PluginId { get; }

    /// <summary>
    /// 插件目录（<c>plugins/&lt;plugin-id&gt;/</c>）。
    /// </summary>
    public string PluginDirectory { get; }

    /// <summary>
    /// 创建插件加载上下文。
    /// </summary>
    /// <param name="pluginId">插件标识。</param>
    /// <param name="mainAssemblyPath">入口程序集绝对路径（用于构造依赖解析器）。</param>
    /// <param name="collectible">是否可回收（默认 <c>true</c>）。</param>
    public PluginLoadContext(string pluginId, string mainAssemblyPath, bool collectible = true)
        : base($"Plugin:{pluginId}", isCollectible: collectible)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginId);
        ArgumentException.ThrowIfNullOrWhiteSpace(mainAssemblyPath);

        PluginId = pluginId;
        PluginDirectory = Path.GetDirectoryName(mainAssemblyPath) ?? string.Empty;
        _resolver = new AssemblyDependencyResolver(mainAssemblyPath);
    }

    /// <inheritdoc />
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var name = assemblyName.Name;
        if (string.IsNullOrEmpty(name))
            return null;

        // 共享集 → 返回 null 交由默认 ALC 处理，保证类型标识统一。
        if (IsShared(name))
            return null;

        var path = _resolver.ResolveAssemblyToPath(assemblyName);
        return path is null ? null : LoadFromAssemblyPath(path);
    }

    /// <inheritdoc />
    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var path = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return path is null ? IntPtr.Zero : LoadUnmanagedDllFromPath(path);
    }

    /// <summary>
    /// 判断是否属于必须共享（回落默认 ALC）的程序集。
    /// </summary>
    /// <param name="assemblyName">程序集简单名。</param>
    /// <returns>需要共享时返回 <c>true</c>。</returns>
    private static bool IsShared(string assemblyName)
    {
        if (SharedExactNames.Contains(assemblyName))
            return true;

        foreach (var prefix in SharedPrefixes)
        {
            if (assemblyName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}