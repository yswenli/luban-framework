/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Tools.Wiki
*文件名： WikiToolGroup
*版本号： V1.0.0.0
*唯一标识：8a2f7c14-6b93-4d5e-a1c8-3f0e2b7d9a41
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/20
*描述：wiki 工具实现
*
*****************************************************************************/
using System.ComponentModel;
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Wiki;

namespace LuBan.AIAgent.Tools.Wiki;

/// <summary>wiki 工具实现。</summary>
public class WikiToolGroup
{
    private readonly IWikiService _wiki;
    private readonly WikiToolOptions _options;
    private readonly IToolConfirmationService _confirmation;

    /// <summary>构造。</summary>
    public WikiToolGroup(IWikiService wiki, WikiToolOptions options, IToolConfirmationService confirmation)
    {
        _wiki = wiki;
        _options = options;
        _confirmation = confirmation;
    }

    /// <summary>读取 index.md。</summary>
    [Description("读取 wiki 的 index.md，了解已有页面清单。")]
    public async Task<ToolResult<string>> ReadIndexAsync(CancellationToken cancellationToken = default)
    {
        var index = await _wiki.ReadIndexAsync(cancellationToken);
        var text = index.Render();
        return ToolResult.Ok(text);
    }

    /// <summary>读取页面。</summary>
    [Description("读取 wiki 页面正文。参数 relativePath 为相对 wiki 根的路径，如 entities/张三.md。")]
    public async Task<ToolResult<string>> ReadPageAsync(
        [Description("相对 wiki 根的页面路径")] string relativePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var page = await _wiki.ReadPageAsync(relativePath, cancellationToken);
            return ToolResult.Ok(page.Body);
        }
        catch (Exception ex)
        {
            return ToolResult.Fail<string>(ex.Message);
        }
    }

    /// <summary>写入/更新页面。</summary>
    [Description("写入或更新一个 wiki 页面。服务会自动维护 index.md、log.md 与向量索引。")]
    public async Task<ToolResult<string>> SavePageAsync(
        [Description("相对 wiki 根的页面路径，如 entities/张三.md")] string relativePath,
        [Description("页面标题")] string title,
        [Description("页面类型：source|entity|concept|overview|query")] string type,
        [Description("Markdown 正文")] string body,
        [Description("来源路径数组（相对工作区根），可空")] string[]? sources = null,
        [Description("标签数组，可空")] string[]? tags = null,
        [Description("供 index.md 的摘要，可空")] string? summary = null,
        CancellationToken cancellationToken = default)
    {
        var args = new Dictionary<string, object?> { ["relativePath"] = relativePath };
        var outcome = await _confirmation.EvaluateAsync("wiki.savePage", relativePath, args);
        if (outcome == EnumConfirmationOutcome.Planned) return ToolResult.Plan<string>();
        if (outcome == EnumConfirmationOutcome.Denied) return ToolResult.Denied<string>();

        try
        {
            await _wiki.SavePageAsync(new WikiPage
            {
                RelativePath = relativePath,
                Title = title,
                Type = type,
                Body = body,
                Sources = sources?.ToList() ?? new List<string>(),
                Tags = tags?.ToList() ?? new List<string>(),
                IndexSummary = summary
            }, cancellationToken);
            return ToolResult.Ok($"已保存 {relativePath}");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail<string>(ex.Message);
        }
    }

    /// <summary>删除页面。</summary>
    [Description("删除一个 wiki 页面，并同步移除 index 条目与向量索引。")]
    public async Task<ToolResult<string>> DeletePageAsync(
        [Description("相对 wiki 根的页面路径")] string relativePath,
        CancellationToken cancellationToken = default)
    {
        var args = new Dictionary<string, object?> { ["relativePath"] = relativePath };
        var outcome = await _confirmation.EvaluateAsync("wiki.deletePage", relativePath, args);
        if (outcome == EnumConfirmationOutcome.Planned) return ToolResult.Plan<string>();
        if (outcome == EnumConfirmationOutcome.Denied) return ToolResult.Denied<string>();

        try
        {
            await _wiki.DeletePageAsync(relativePath, cancellationToken);
            return ToolResult.Ok($"已删除 {relativePath}");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail<string>(ex.Message);
        }
    }

    /// <summary>搜索。</summary>
    [Description("在 wiki 中做向量搜索；includeRaw=true 时同时回落到 raw 工作区文件。")]
    public async Task<ToolResult<string>> SearchAsync(
        [Description("查询内容")] string query,
        [Description("返回条数，省略用默认值")] int? topK = null,
        [Description("是否同时检索 raw 工作区文件")] bool? includeRaw = null,
        CancellationToken cancellationToken = default)
    {
        var results = await _wiki.SearchAsync(query, topK ?? _options.TopK, includeRaw ?? _options.IncludeRawDefault, cancellationToken);
        if (results.Count == 0) return ToolResult.Fail<string>("未找到相关内容。");

        var sb = new System.Text.StringBuilder();
        foreach (var r in results)
            sb.Append("### ").Append(r.FilePath).Append(" (L").Append(r.StartLine).Append('-').Append(r.EndLine).Append(")\n")
              .Append(r.Content).Append("\n\n");

        var text = sb.ToString();
        if (text.Length > _options.MaxResultChars) text = text[.._options.MaxResultChars];
        return ToolResult.Ok(text);
    }

    /// <summary>lint。</summary>
    [Description("检查 wiki 的孤儿页、死链、未收录、来源缺失、来源过期。")]
    public async Task<ToolResult<string>> LintAsync(CancellationToken cancellationToken = default)
    {
        var report = await _wiki.LintAsync(cancellationToken);
        if (report.Findings.Count == 0) return ToolResult.Ok($"共 {report.PageCount} 页，未发现问题。");

        var sb = new System.Text.StringBuilder();
        sb.Append("共 ").Append(report.PageCount).Append(" 页，发现 ").Append(report.Findings.Count).Append(" 项：\n");
        foreach (var f in report.Findings)
            sb.Append("- [").Append(f.Kind).Append("] ").Append(f.Path).Append("：").Append(f.Detail).Append('\n');
        return ToolResult.Ok(sb.ToString());
    }
}