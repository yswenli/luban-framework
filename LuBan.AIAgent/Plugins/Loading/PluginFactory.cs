/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Plugins.Loading
*文件名： PluginFactory
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/15
*描述：插件实例工厂 - 在插件程序集中定位 IPlugin 实现并实例化
*
*****************************************************************************/
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace LuBan.AIAgent.Plugins.Loading;

/// <summary>
/// 插件实例工厂：在入口程序集中定位 <see cref="IPlugin"/> 实现并实例化（PRD §3.4 加载流程第 4 步）。
/// </summary>
/// <remarks>
/// 注意：本类<b>不缓存</b> <see cref="Type"/>，否则会经反射元数据长期持有插件 ALC 内类型，
/// 导致插件无法卸载（PRD §3.12）。
/// </remarks>
public sealed class PluginFactory
{
    /// <summary>
    /// 发现程序集中全部可实例化的 <see cref="IPlugin"/> 实现。
    /// </summary>
    /// <param name="assembly">插件入口程序集。</param>
    /// <returns>候选类型列表。</returns>
    public IReadOnlyList<Type> DiscoverPluginTypes(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        Type[] types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            types = ex.Types.Where(t => t is not null).Cast<Type>().ToArray();
        }

        return types
            .Where(type => type is { IsAbstract: false, IsInterface: false, IsPublic: true })
            .Where(type => typeof(IPlugin).IsAssignableFrom(type))
            .ToList();
    }

    /// <summary>
    /// 在程序集中解析目标插件类型。
    /// </summary>
    /// <param name="assembly">插件入口程序集。</param>
    /// <param name="expectedPluginId">清单声明的插件 id（用于在多个候选中消歧）。</param>
    /// <returns>插件类型；无法唯一确定时返回 <c>null</c>。</returns>
    public Type? ResolvePluginType(Assembly assembly, string? expectedPluginId)
    {
        var candidates = DiscoverPluginTypes(assembly);

        if (candidates.Count == 0)
            return null;

        if (candidates.Count == 1)
            return candidates[0];

        if (string.IsNullOrWhiteSpace(expectedPluginId))
            return null;

        // 多候选时按 Id 消歧（此处会临时实例化，仅为探测，随后即丢弃）。
        foreach (var candidate in candidates)
        {
            if (!TryReadId(candidate, out var id))
                continue;

            if (string.Equals(id, expectedPluginId, StringComparison.OrdinalIgnoreCase))
                return candidate;
        }

        return null;
    }

    /// <summary>
    /// 实例化插件类型。
    /// </summary>
    /// <param name="pluginType">插件类型。</param>
    /// <returns>插件实例。</returns>
    /// <exception cref="InvalidOperationException">无法实例化时抛出。</exception>
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2067",
        Justification = "插件程序集由运行时从外部目录加载，不参与宿主裁剪；IsTrimmable 只作用于 LuBan.AIAgent 自身代码。")]
    public IPlugin Instantiate(Type pluginType)
    {
        ArgumentNullException.ThrowIfNull(pluginType);

        try
        {
            if (Activator.CreateInstance(pluginType) is IPlugin plugin)
                return plugin;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"实例化插件类型失败：{pluginType.FullName}", ex);
        }

        throw new InvalidOperationException($"类型未实现 IPlugin：{pluginType.FullName}");
    }

    /// <summary>
    /// 探测插件类型声明的 Id（临时实例化，失败即视为不可探测）。
    /// </summary>
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2067",
        Justification = "同 Instantiate：目标类型来自运行时加载的插件程序集，不在宿主裁剪范围内。")]
    private static bool TryReadId(Type pluginType, out string? id)
    {
        id = null;
        try
        {
            if (Activator.CreateInstance(pluginType) is IPlugin plugin)
            {
                id = plugin.Id;
                return !string.IsNullOrWhiteSpace(id);
            }
        }
        catch
        {
            // 探测失败不影响后续判定
        }

        return false;
    }
}