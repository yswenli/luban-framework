/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.ApprovalFlow
*文件名： FlowBuilder
*版本号： V1.0.0.0
*唯一标识：101d1262-bf30-41c6-97d7-c8175d43d396
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/30 13:50:48
*描述：审批流构建器：负责定义的加载与缓存。
*
*=================================================
*修改标记
*修改时间：2025/10/30 13:50:48
*修改人： yswenli
*版本号： V1.0.0.0
*描述：审批流构建器：负责定义的加载与缓存。
*
*****************************************************************************/

namespace LuBan.ApprovalFlow;

/// <summary>
/// 审批流构建器：负责定义的加载与缓存。
/// </summary>
public class FlowBuilder
{
    private readonly ConcurrentDictionary<string, FlowExecutor> _executorCache = new();

    /// <summary>
    /// 从 JSON 文件加载审批流定义并写入内存缓存（若存在同编码则更新）。
    /// </summary>
    /// <param name="jsonPath">JSON 文件路径。</param>
    /// <returns>解析后的流程执行器。</returns>
    private async Task<FlowExecutor> LoadFromJsonAsync(string jsonPath)
    {
        if (string.IsNullOrWhiteSpace(jsonPath)) throw new ArgumentException("JSON 文件路径为空", nameof(jsonPath));
        if (!File.Exists(jsonPath)) throw new FileNotFoundException("未找到审批流配置文件", jsonPath);
        string raw;
        try
        {
            raw = await File.ReadAllTextAsync(jsonPath);
        }
        catch (Exception ex)
        {
            throw new IOException($"读取审批流配置文件失败: {jsonPath}", ex);
        }
        GraphFlowDefinition? def = SerializeUtil.Deserialize<GraphFlowDefinition>(raw);
        if (def is null) throw new InvalidOperationException("审批流配置解析失败");
        var keyCode = !string.IsNullOrWhiteSpace(def.Key) ? def.Key! : Path.GetFileNameWithoutExtension(jsonPath);
        return await UpsertExecutorAsync(def, keyCode);
    }

    /// <summary>
    /// 从原始 JSON 字符串加载审批流定义并写入内存缓存（若存在同编码则更新）。
    /// </summary>
    /// <param name="json">原始 JSON 内容。</param>
    /// <param name="key">编码提示（当定义未提供 <c>Key</c> 时使用）。</param>
    /// <returns>解析后的 <see cref="GraphFlowDefinition"/>。</returns>
    private async Task<FlowExecutor> LoadFromJsonStringAsync(string json, string? key = null)
    {
        if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException("JSON 内容为空", nameof(json));
        GraphFlowDefinition? def = SerializeUtil.Deserialize<GraphFlowDefinition>(json);
        if (def is null) throw new InvalidOperationException("审批流配置解析失败");
        var keyCode = !string.IsNullOrWhiteSpace(def.Key) ? def.Key! : (key ?? Guid.NewGuid().ToString("N"));
        return await UpsertExecutorAsync(def, keyCode);
    }

    /// <summary>
    /// 插入或更新审批流定义到内存缓存
    /// </summary>
    /// <param name="def">审批流定义。</param>
    /// <param name="key">编码提示（当定义未提供 <c>Key</c> 时使用）。</param>
    /// <returns>缓存后的 <see cref="GraphFlowDefinition"/>。</returns>
    private async Task<FlowExecutor> UpsertExecutorAsync(GraphFlowDefinition def, string? key = null)
    {
        ArgumentNullException.ThrowIfNull(def);
        var keyCode = !string.IsNullOrWhiteSpace(def.Key) ? def.Key! : (key ?? Guid.NewGuid().ToString("N"));

        var updated = _executorCache.AddOrUpdate(
            keyCode,
            k => new FlowExecutor(this, def),
            (k, existing) =>
            {
                existing.FlowDefinition = def;
                return existing;
            });
        return await Task.FromResult(updated);
    }


    /// <summary>
    /// 从 JSON 文件加载定义并创建流程执行器。
    /// </summary>
    /// <param name="jsonPath">JSON 文件路径。</param>
    /// <returns>执行器与解析后的流程定义二元组。</returns>
    public async Task<FlowExecutor> CreateExecutorFromJsonFileAsync(string jsonPath)
    {
        var executor = await LoadFromJsonAsync(jsonPath);
        return executor;
    }

    /// <summary>
    /// 从原始 JSON 字符串加载定义并创建流程执行器。
    /// </summary>
    /// <param name="json">原始 JSON 内容。</param>
    /// <param name="key">编码提示（当定义未提供 <c>Key</c> 时使用）。</param>
    /// <returns>执行器与解析后的流程定义二元组。</returns>
    public async Task<FlowExecutor> CreateExecutorFromJsonStringAsync(string json, string? key = null)
    {
        var executor = await LoadFromJsonStringAsync(json, key);
        return executor;
    }

    /// <summary>
    /// 直接使用给定的定义创建流程执行器，并写入缓存（若存在同编码则更新）。
    /// </summary>
    /// <param name="def">流程定义对象。</param>
    /// <param name="key">编码提示（当定义未提供 <c>Key</c> 时使用）。</param>
    /// <returns>执行器与最终缓存中的流程定义二元组。</returns>
    public async Task<FlowExecutor> CreateExecutorFromDefinitionAsync(GraphFlowDefinition def, string? key = null)
    {
        var executor = await UpsertExecutorAsync(def, key);
        return executor;
    }

    /// <summary>
    /// 创建一个流程执行器实例，使用当前构建器及已注册的节点处理器链。
    /// </summary>
    /// <returns>流程执行器实例。</returns>
    public FlowExecutor CreateExecutor()
    {
        return new FlowExecutor(this);
    }

    /// <summary>
    /// 通过流程 Key 尝试获取已缓存的执行器实例。
    /// </summary>
    /// <param name="key">流程编码 Key。</param>
    /// <param name="executor">输出执行器实例；若不存在则为 null。</param>
    /// <returns>是否找到了对应的执行器。</returns>
    public bool TryGetExecutor(string key, out FlowExecutor? executor)
    {
        executor = null;
        if (string.IsNullOrWhiteSpace(key)) return false;
        return _executorCache.TryGetValue(key, out executor);
    }

    /// <summary>
    /// 通过流程 Key 获取已缓存的执行器实例。
    /// </summary>
    /// <param name="key">流程编码 Key。</param>
    /// <returns>执行器实例；若不存在则为 null。</returns>
    public FlowExecutor? GetExecutor(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        _executorCache.TryGetValue(key, out var executor);
        return executor;
    }

    /// <summary>
    /// 绑定流程定义并创建执行器实例（不缓存）。
    /// </summary>
    /// <param name="definition">流程定义。</param>
    /// <returns>流程执行器实例。</returns>
    public FlowExecutor Bind(GraphFlowDefinition definition)
    {
        return new FlowExecutor(this, definition);
    }

    /// <summary>
    /// 绑定流程定义并构建执行器实例（不缓存）。
    /// </summary>
    /// <param name="definition">流程定义。</param>
    /// <returns>流程执行器实例。</returns>
    public FlowExecutor Build()
    {
        return new FlowExecutor(this);
    }
}