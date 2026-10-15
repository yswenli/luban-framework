/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Plugins.Loading
*文件名： PluginDependencyResolver
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/15
*描述：插件依赖解析 - 存在性校验 + 拓扑排序（PRD §3.2 / §3.4）
*
*****************************************************************************/
namespace LuBan.AIAgent.Plugins.Loading;

/// <summary>
/// 插件依赖解析器：对 <c>dependencies</c> 做存在性校验并产出拓扑有序的加载序列（PRD §3.2 / §3.4）。
/// </summary>
/// <remarks>
/// PRD 定案：<b>仅校验存在性</b>，不做版本约束解析；且禁止自引用（由
/// <see cref="ManifestValidator"/> 拦截）。存在环时整体报错，不产出顺序。
/// </remarks>
public sealed class PluginDependencyResolver
{
    /// <summary>
    /// 解析依赖顺序。
    /// </summary>
    /// <param name="manifests">全部待加载插件的清单。</param>
    /// <returns>解析结果（含有序加载序列与错误）。</returns>
    public DependencyResolutionResult Resolve(IEnumerable<PluginManifest> manifests)
    {
        ArgumentNullException.ThrowIfNull(manifests);

        var result = new DependencyResolutionResult();

        var byId = new Dictionary<string, PluginManifest>(StringComparer.OrdinalIgnoreCase);
        foreach (var manifest in manifests)
        {
            if (manifest is null || string.IsNullOrWhiteSpace(manifest.Id))
                continue;

            if (!byId.TryAdd(manifest.Id, manifest))
                result.Errors.Add($"存在重复的插件 id：{manifest.Id}");
        }

        // 存在性校验
        foreach (var (id, manifest) in byId)
        {
            foreach (var dependency in manifest.Dependencies)
            {
                if (string.IsNullOrWhiteSpace(dependency))
                    continue;

                if (!byId.ContainsKey(dependency))
                    result.Errors.Add($"插件 {id} 依赖的插件不存在：{dependency}");
            }
        }

        // 拓扑排序（DFS 三色法，检测环）
        var ordered = new List<PluginManifest>(byId.Count);
        var state = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase); // 0 未访问 1 访问中 2 已完成
        var path = new List<string>();

        foreach (var id in byId.Keys)
            Visit(id, byId, state, path, ordered, result);

        result.Ordered = ordered;
        return result;
    }

    /// <summary>
    /// 深度优先访问（后序入列，保证依赖先于被依赖者）。
    /// </summary>
    private static void Visit(
        string id,
        Dictionary<string, PluginManifest> byId,
        Dictionary<string, int> state,
        List<string> path,
        List<PluginManifest> ordered,
        DependencyResolutionResult result)
    {
        state.TryGetValue(id, out var current);

        if (current == 2)
            return;

        if (current == 1)
        {
            // 发现环：截取环路径用于报错
            var start = path.IndexOf(id);
            var cycle = start >= 0
                ? string.Join(" → ", path.Skip(start).Append(id))
                : id;

            result.Errors.Add($"插件依赖存在环：{cycle}");
            return;
        }

        state[id] = 1;
        path.Add(id);

        if (byId.TryGetValue(id, out var manifest))
        {
            foreach (var dependency in manifest.Dependencies)
            {
                if (!string.IsNullOrWhiteSpace(dependency) && byId.ContainsKey(dependency))
                    Visit(dependency, byId, state, path, ordered, result);
            }

            ordered.Add(manifest);
        }

        path.RemoveAt(path.Count - 1);
        state[id] = 2;
    }
}

/// <summary>
/// 依赖解析结果。
/// </summary>
public sealed class DependencyResolutionResult
{
    /// <summary>
    /// 拓扑有序的插件清单（依赖在前）。
    /// </summary>
    public IReadOnlyList<PluginManifest> Ordered { get; internal set; } = Array.Empty<PluginManifest>();

    /// <summary>
    /// 错误列表（缺失依赖 / 重复 id / 环）。
    /// </summary>
    public List<string> Errors { get; } = new();

    /// <summary>
    /// 是否存在解析错误。
    /// </summary>
    public bool HasErrors => Errors.Count > 0;
}