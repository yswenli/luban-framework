/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Tools.LocalMemory
*文件名： LocalMemoryToolPlugin
*版本号： V1.0.0.0
*唯一标识：a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/4
*描述：本地长期记忆工具插件，支持保存、搜索、列出和删除记忆
*
*=================================================
*修改标记
*修改时间：2026/8/4
*修改人： yswenli
*版本号： V1.0.0.0
*描述：本地长期记忆工具插件
*
*****************************************************************************/

namespace LuBan.AIAgent.Tools.LocalMemory;

/// <summary>
/// 本地长期记忆工具插件
/// </summary>
public class LocalMemoryToolPlugin : ILuBanToolPlugin
{
    private readonly LocalMemoryOptions _options;

    /// <summary>
    /// 创建 LocalMemoryToolPlugin 实例
    /// </summary>
    public LocalMemoryToolPlugin(IOptions<LuBanAgentOptions> options)
    {
        _options = options.Value.Tools.LocalMemory;
    }

    /// <inheritdoc />
    public string GroupName => "localmemory";

    /// <inheritdoc />
    public string? Description => "本地长期记忆工具，支持保存、搜索、列出和删除跨会话的事实与偏好";

    /// <inheritdoc />
    public IReadOnlyList<AIFunction> GetTools(DiContainer sp, ToolGroupOptions? toolsOptions = null)
    {
        var memoryOptions = toolsOptions?.LocalMemory ?? _options;
        var confirmationService = sp.GetRequiredService<IToolConfirmationService>();
        var group = new LocalMemoryToolGroup(sp.GetRequiredService<ILocalMemoryService>(), memoryOptions, confirmationService);
        return new List<AIFunction>
        {
            // 显式命名：避免暴露成 Save/Search/List/Delete 这类过于通用的名字
            AIFunctionFactoryHelper.Create(group, nameof(LocalMemoryToolGroup.SaveAsync), name: LocalMemoryToolGroup.SaveToolName),
            AIFunctionFactoryHelper.Create(group, nameof(LocalMemoryToolGroup.SearchAsync), name: LocalMemoryToolGroup.SearchToolName),
            AIFunctionFactoryHelper.Create(group, nameof(LocalMemoryToolGroup.ListAsync), name: LocalMemoryToolGroup.ListToolName),
            AIFunctionFactoryHelper.Create(group, nameof(LocalMemoryToolGroup.DeleteAsync), name: LocalMemoryToolGroup.DeleteToolName)
        };
    }

    /// <inheritdoc />
    public bool IsEnabled(LuBanAgentOptions options) => options.Tools.LocalMemory.Enabled;
}

/// <summary>
/// 本地长期记忆工具分组
/// </summary>
public class LocalMemoryToolGroup
{
    /// <summary>保存工具的确认名（与模型可见名一致）。</summary>
    public const string SaveToolName = "MemorySave";

    /// <summary>搜索工具的确认名（与模型可见名一致）。</summary>
    public const string SearchToolName = "MemorySearch";

    /// <summary>列出工具的确认名（与模型可见名一致）。</summary>
    public const string ListToolName = "MemoryList";

    /// <summary>删除工具的确认名（与模型可见名一致）。</summary>
    public const string DeleteToolName = "MemoryDelete";

    // 四个记忆工具统一用模型可见名做确认/策略判定，而不是 nameof(XxxAsync)：
    // 框架的免确认、硬确认、只读、子代理禁止四张名单都按字符串匹配，
    // 而 SearchAsync / DeleteAsync 这类名字与其他工具组重名（如 WikiToolGroup.SearchAsync），
    // 按方法名匹配会连带放行或拒绝别的工具。LocalMemory 是自有工具组，可自由选名。

    private readonly ILocalMemoryService _service;
    private readonly LocalMemoryOptions _options;
    private readonly IToolConfirmationService _confirmationService;

    /// <summary>
    /// 创建 LocalMemoryToolGroup 实例
    /// </summary>
    public LocalMemoryToolGroup(ILocalMemoryService service, LocalMemoryOptions options, IToolConfirmationService confirmationService)
    {
        _service = service;
        _options = options;
        _confirmationService = confirmationService;
    }

    /// <summary>
    /// 保存一条记忆到本地长期记忆库
    /// </summary>
    /// <param name="content">要保存的内容</param>
    /// <param name="category">记忆类别，如 fact、preference、todo、project</param>
    [Description("保存一条记忆到本地长期记忆库")]
    public async Task<ToolResult<string>> SaveAsync(string content, string category = "general")
    {
        // 保存是低风险操作，由框架内置免确认名单（MemorySave）直接放行，不打断「记住刚才的内容」；
        // 但仍经 EvaluateAsync，以保留 Plan 模式「只记录计划、不写库」的语义
        // 与宿主策略闸门（PolicyGuard）的拦截能力。
        var outcome = await _confirmationService.EvaluateAsync(SaveToolName, null,
            new Dictionary<string, object?> { ["content"] = content, ["category"] = category });
        if (outcome == EnumConfirmationOutcome.Planned)
            return ToolResult.Plan<string>();
        if (outcome != EnumConfirmationOutcome.Allowed)
            return ToolResult.Denied<string>();

        if (string.IsNullOrWhiteSpace(content))
            return ToolResult.Fail<string>("内容不能为空");

        try
        {
            var entry = await _service.SaveAsync(content, category);
            return ToolResult.Ok(entry.Id, $"已保存记忆，类别: {entry.Category}");
        }
        catch (Exception ex)
        {
            Logger.Error("保存本地记忆失败", ex, content);
            return ToolResult.Fail<string>($"保存失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 基于语义相似度搜索本地长期记忆
    /// </summary>
    /// <param name="query">查询文本</param>
    /// <param name="category">可选类别过滤</param>
    /// <param name="topK">返回条数，留空则使用配置的 DefaultTopK</param>
    [Description("基于语义相似度搜索本地长期记忆")]
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026", 
        Justification = "JSON 序列化仅用于简单结果类型，已通过 JsonSerializerOptions 处理")]
    /// <summary>
    /// 按语义相似度检索本地长期记忆，返回命中条目的 JSON。
    /// </summary>
    /// <param name="query">查询文本，不能为空。</param>
    /// <param name="category">可选的类别过滤条件。</param>
    /// <param name="topK">返回条数上限，留空则取配置的默认值。</param>
    /// <returns>成功时正文为记忆数组 JSON，失败时携带错误信息。</returns>
    public async Task<ToolResult<string>> SearchAsync(string query, string? category = null, int? topK = null)
    {
        // 读记忆无副作用，但仍过确认门：Plan 模式下按只读工具放行，
        // Default 模式由内置免确认名单（MemorySearch）直接放行，宿主策略闸门可拦截
        var outcome = await _confirmationService.EvaluateAsync(SearchToolName, null,
            new Dictionary<string, object?> { ["query"] = query, ["category"] = category });
        if (outcome == EnumConfirmationOutcome.Planned)
            return ToolResult.Plan<string>();
        if (outcome != EnumConfirmationOutcome.Allowed)
            return ToolResult.Denied<string>();

        if (string.IsNullOrWhiteSpace(query))
            return ToolResult.Fail<string>("查询不能为空");

        try
        {
            // 未显式指定时回落到配置值：此前写死 5，导致 Tools:LocalMemory:DefaultTopK 配置永不生效
            var effectiveTopK = topK ?? _options.DefaultTopK;
            var results = await _service.SearchAsync(query, category, effectiveTopK);
            if (results.Count == 0)
                return ToolResult.Ok<string>("未找到相关记忆");

            var json = results.Select(r => new
            {
                r.Id,
                r.Content,
                r.Category,
                r.Score,
                r.CreatedAt,
                r.UpdatedAt
            }).ToJson();
            return ToolResult.Ok(json, $"找到 {results.Count} 条相关记忆");
        }
        catch (Exception ex)
        {
            Logger.Error("搜索本地记忆失败", ex, query);
            return ToolResult.Fail<string>($"搜索失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 列出本地长期记忆条目
    /// </summary>
    /// <param name="category">可选类别过滤</param>
    /// <param name="limit">最大条数</param>
    [Description("列出本地长期记忆条目")]
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026", 
        Justification = "JSON 序列化仅用于简单结果类型，已通过 JsonSerializerOptions 处理")]
    /// <summary>
    /// 列出本地长期记忆条目。
    /// </summary>
    /// <param name="category">可选的类别过滤条件。</param>
    /// <param name="limit">返回的最大条数。</param>
    /// <returns>成功时正文为记忆数组 JSON，失败时携带错误信息。</returns>
    public async Task<ToolResult<string>> ListAsync(string? category = null, int limit = 100)
    {
        // 同 SearchAsync：只读但过确认门（Default 由内置免确认名单 MemoryList 放行）
        var outcome = await _confirmationService.EvaluateAsync(ListToolName, null,
            new Dictionary<string, object?> { ["category"] = category, ["limit"] = limit });
        if (outcome == EnumConfirmationOutcome.Planned)
            return ToolResult.Plan<string>();
        if (outcome != EnumConfirmationOutcome.Allowed)
            return ToolResult.Denied<string>();

        try
        {
            var entries = await _service.ListAsync(category, limit);
            if (entries.Count == 0)
                return ToolResult.Ok<string>("本地记忆库为空");

            var json = entries.Select(e => new
            {
                e.Id,
                e.Content,
                e.Category,
                e.CreatedAt,
                e.UpdatedAt
            }).ToJson();
            return ToolResult.Ok(json, $"共 {entries.Count} 条记忆");
        }
        catch (Exception ex)
        {
            Logger.Error("列出本地记忆失败", ex);
            return ToolResult.Fail<string>($"列出失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 删除指定本地记忆条目
    /// </summary>
    /// <param name="id">记忆 ID</param>
    [Description("删除指定本地记忆条目")]
    public async Task<ToolResult<string>> DeleteAsync(string id)
    {
        var outcome = await _confirmationService.EvaluateAsync(DeleteToolName, null,
            new Dictionary<string, object?> { ["id"] = id });
        if (outcome == EnumConfirmationOutcome.Planned)
            return ToolResult.Plan<string>();
        if (outcome != EnumConfirmationOutcome.Allowed)
            return ToolResult.Denied<string>();

        if (string.IsNullOrWhiteSpace(id))
            return ToolResult.Fail<string>("ID 不能为空");

        try
        {
            var ok = await _service.DeleteAsync(id);
            return ok
                ? ToolResult.Ok<string>("已删除记忆")
                : ToolResult.Fail<string>("未找到指定记忆");
        }
        catch (Exception ex)
        {
            Logger.Error("删除本地记忆失败", ex, id);
            return ToolResult.Fail<string>($"删除失败: {ex.Message}");
        }
    }
}
