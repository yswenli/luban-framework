/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
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
using System.ComponentModel;
using System.Text.Json;
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.LocalMemory;
using Microsoft.Extensions.Options;

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
    public IReadOnlyList<AIFunction> GetTools(IServiceProvider sp)
    {
        var group = new LocalMemoryToolGroup(sp.GetRequiredService<ILocalMemoryService>(), _options);
        return new List<AIFunction>
        {
            AIFunctionFactoryHelper.Create(group, nameof(LocalMemoryToolGroup.SaveAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(LocalMemoryToolGroup.SearchAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(LocalMemoryToolGroup.ListAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(LocalMemoryToolGroup.DeleteAsync))
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
    private readonly ILocalMemoryService _service;
    private readonly LocalMemoryOptions _options;

    /// <summary>
    /// 创建 LocalMemoryToolGroup 实例
    /// </summary>
    public LocalMemoryToolGroup(ILocalMemoryService service, LocalMemoryOptions options)
    {
        _service = service;
        _options = options;
    }

    /// <summary>
    /// 保存一条记忆到本地长期记忆库
    /// </summary>
    /// <param name="content">要保存的内容</param>
    /// <param name="category">记忆类别，如 fact、preference、todo、project</param>
    [Description("保存一条记忆到本地长期记忆库")]
    public async Task<ToolResult<string>> SaveAsync(string content, string category = "general")
    {
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
    /// <param name="topK">返回条数</param>
    [Description("基于语义相似度搜索本地长期记忆")]
    public async Task<ToolResult<string>> SearchAsync(string query, string? category = null, int topK = 5)
    {
        if (string.IsNullOrWhiteSpace(query))
            return ToolResult.Fail<string>("查询不能为空");

        try
        {
            var results = await _service.SearchAsync(query, category, topK);
            if (results.Count == 0)
                return ToolResult.Ok<string>("未找到相关记忆");

            var json = JsonSerializer.Serialize(results.Select(r => new
            {
                r.Id,
                r.Content,
                r.Category,
                r.Score,
                r.CreatedAt,
                r.UpdatedAt
            }));
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
    public async Task<ToolResult<string>> ListAsync(string? category = null, int limit = 100)
    {
        try
        {
            var entries = await _service.ListAsync(category, limit);
            if (entries.Count == 0)
                return ToolResult.Ok<string>("本地记忆库为空");

            var json = JsonSerializer.Serialize(entries.Select(e => new
            {
                e.Id,
                e.Content,
                e.Category,
                e.CreatedAt,
                e.UpdatedAt
            }));
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
