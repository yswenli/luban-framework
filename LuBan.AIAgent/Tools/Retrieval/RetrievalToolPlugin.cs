using System.ComponentModel;
using System.Reflection;
using System.Text;
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Retrieval;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LuBan.AIAgent.Tools.Retrieval;

/// <summary>
/// 语义检索工具插件
/// </summary>
public class RetrievalToolPlugin : ILuBanToolPlugin
{
    private readonly IOptions<LuBanAgentOptions> _options;

    /// <summary>
    /// 创建检索工具插件
    /// </summary>
    public RetrievalToolPlugin(IOptions<LuBanAgentOptions> options) => _options = options;

    /// <inheritdoc />
    public string GroupName => "retrieval";

    /// <inheritdoc />
    public string? Description => "语义检索工具：索引本地代码/文档/网页内容并按语义搜索";

    /// <inheritdoc />
    public IReadOnlyList<AIFunction> GetTools(IServiceProvider sp)
    {
        var svc = sp.GetService<IRetrievalService>();
        if (svc == null) return Array.Empty<AIFunction>();
        var group = new RetrievalToolGroup(svc, _options.Value.Tools.Retrieval);
        return typeof(RetrievalToolGroup)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Select(m => AIFunctionFactory.Create(m, group))
            .ToList();
    }

    /// <inheritdoc />
    public bool IsEnabled(LuBanAgentOptions options) => options.Tools.Retrieval.Enabled;
}

/// <summary>
/// 语义检索工具组
/// </summary>
public class RetrievalToolGroup
{
    private readonly IRetrievalService _service;
    private readonly RetrievalToolOptions _options;

    /// <summary>
    /// 创建工具组
    /// </summary>
    public RetrievalToolGroup(IRetrievalService service, RetrievalToolOptions options)
    {
        _service = service;
        _options = options;
    }

    /// <summary>
    /// 索引指定目录中的文本文件
    /// </summary>
    [Description("索引指定目录中的文本文件（代码/文档），用于后续语义检索。重复调用只做增量更新。")]
    public async Task<string> IndexDirectoryAsync(
        [Description("目录绝对路径")] string path,
        [Description("文件匹配模式，如 *.cs 或 *.cs;*.md，留空匹配全部")] string? glob = null,
        [Description("强制全部重建索引")] bool force = false)
    {
        if (!Directory.Exists(path)) return $"错误：目录不存在 {path}";
        var r = await _service.IndexDirectoryAsync(path, glob, force);
        var sb = new StringBuilder();
        sb.AppendLine($"索引完成：扫描 {r.ScannedFiles}，新增 {r.NewFiles}，更新 {r.UpdatedFiles}，跳过 {r.SkippedFiles}，删除 {r.DeletedFiles}");
        sb.AppendLine($"切块 {r.TotalChunks}（新嵌入 {r.EmbeddedChunks}，复用 {r.ReusedChunks}）");
        if (r.Errors.Count > 0) sb.AppendLine($"错误 {r.Errors.Count}：{string.Join("；", r.Errors.Take(3))}");
        return sb.ToString();
    }

    /// <summary>
    /// 索引一段文本内容
    /// </summary>
    [Description("索引一段文本内容（如网页抓取结果），用于后续语义检索")]
    public async Task<string> IndexContentAsync(
        [Description("文本内容")] string content,
        [Description("语言/格式，如 html、markdown、csharp")] string language,
        [Description("来源标识，如 web://example.com/page")] string sourceName)
    {
        if (string.IsNullOrWhiteSpace(content)) return "错误：内容为空";
        if (content.Length > 2_000_000) return "错误：内容过大（>2MB），请分段索引";
        var r = await _service.IndexContentAsync(content, language, sourceName);
        return $"索引完成：{sourceName}，切块 {r.TotalChunks}（新嵌入 {r.EmbeddedChunks}）";
    }

    /// <summary>
    /// 语义搜索
    /// </summary>
    [Description("在已索引内容中做语义搜索，返回最相关的代码/文档片段")]
    public async Task<string> SearchCodeAsync(
        [Description("搜索内容（自然语言或关键字）")] string query,
        [Description("返回数量，默认5，最大20")] int topK = 5,
        [Description("限定路径前缀（可选）")] string? pathPrefix = null,
        [Description("限定语言（可选），如 csharp、html")] string? language = null)
    {
        var results = await _service.SearchAsync(query, topK, pathPrefix, language);
        if (results.Count == 0) return "未找到相关内容。请先用 IndexDirectoryAsync 或 IndexContentAsync 建立索引。";
        var sb = new StringBuilder();
        int budget = _options.MaxResultChars;
        foreach (var r in results)
        {
            var symbol = r.SymbolName != null ? $" {r.SymbolName}" : "";
            sb.AppendLine($"--- {r.FilePath}:{r.StartLine}-{r.EndLine} [{r.ChunkType}]{symbol} (相关度 {r.Score:F2}) ---");
            var content = r.Content.Length > 1500 ? r.Content[..1500] + "\n…(截断)" : r.Content;
            if (sb.Length + content.Length > budget) { sb.AppendLine("…(结果超出预算，已截断，可缩小 topK 或追加 pathPrefix 重试)"); break; }
            sb.AppendLine(content);
        }
        return sb.ToString();
    }

    /// <summary>
    /// 获取索引统计
    /// </summary>
    [Description("获取语义检索索引的统计信息")]
    public async Task<string> GetIndexStatsAsync()
    {
        var s = await _service.GetStatsAsync();
        return $"已索引文件 {s.TotalFiles} 个，切块 {s.TotalChunks} 个，模型 {s.ModelId ?? "未知"}，向量维度 {s.VectorDimension}";
    }
}
