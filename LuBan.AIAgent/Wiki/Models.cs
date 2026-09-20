/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Wiki
*文件名： Models
*版本号： V1.0.0.0
*唯一标识：6f3a2c10-9d4e-4b8a-8f21-7c5e0d3b1a92
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/20
*描述：wiki 领域模型
*
*=================================================
*修改标记
*修改时间：2026/9/20
*修改人： yswenli
*版本号： V1.0.0.0
*描述：wiki 领域模型
*
*****************************************************************************/
using System.Globalization;
using System.Text.RegularExpressions;

namespace LuBan.AIAgent.Wiki;

/// <summary>wiki 页面（已解析 frontmatter）。</summary>
public class WikiPage
{
    /// <summary>相对 wiki 根的文件路径，统一用 '/' 分隔（如 sources/xxx.md）。</summary>
    public string RelativePath { get; set; } = "";
    /// <summary>标题。</summary>
    public string Title { get; set; } = "";
    /// <summary>页面类型：source|entity|concept|overview|query。</summary>
    public string Type { get; set; } = "source";
    /// <summary>标签。</summary>
    public List<string> Tags { get; set; } = new();
    /// <summary>来源路径（相对工作区根，或 http 链接）。</summary>
    public List<string> Sources { get; set; } = new();
    /// <summary>正文（不含 frontmatter）。</summary>
    public string Body { get; set; } = "";
    /// <summary>供 index.md 使用的摘要（可空）。</summary>
    public string? IndexSummary { get; set; }
    /// <summary>创建日期。</summary>
    public DateTime? Created { get; set; }
    /// <summary>更新日期。</summary>
    public DateTime? Updated { get; set; }
}

/// <summary>index.md 的一个条目。</summary>
public class WikiIndexEntry
{
    /// <summary>分类（目录名或 "pages"）。</summary>
    public string Category { get; set; } = "pages";
    /// <summary>相对路径。</summary>
    public string RelativePath { get; set; } = "";
    /// <summary>标题。</summary>
    public string Title { get; set; } = "";
    /// <summary>摘要。</summary>
    public string? Summary { get; set; }
    /// <summary>更新时间。</summary>
    public DateTime? Updated { get; set; }
}

/// <summary>index.md 的内存模型。</summary>
public class WikiIndex
{
    /// <summary>条目集合。</summary>
    public List<WikiIndexEntry> Entries { get; } = new();

    /// <summary>index.md 中摘要与更新时间的日期格式（Render/Parse 共用）。</summary>
    private const string DateFormat = "yyyy-MM-dd";

    /// <summary>渲染 index.md 中摘要与更新时间之间的分隔标记。</summary>
    private const string UpdatedMarker = " ｜ updated:";

    /// <summary>按相对路径 upsert 条目；页面未提供摘要时保留原条目摘要，避免被静默清空。</summary>
    public void Upsert(string relativePath, WikiPage page)
    {
        var existing = Entries.FirstOrDefault(e => string.Equals(e.RelativePath, relativePath, StringComparison.OrdinalIgnoreCase));
        Entries.RemoveAll(e => string.Equals(e.RelativePath, relativePath, StringComparison.OrdinalIgnoreCase));
        Entries.Add(new WikiIndexEntry
        {
            Category = CategoryOf(relativePath),
            RelativePath = relativePath,
            Title = page.Title,
            Summary = page.IndexSummary ?? existing?.Summary,
            Updated = page.Updated
        });
    }

    /// <summary>移除条目。</summary>
    public void Remove(string relativePath)
        => Entries.RemoveAll(e => string.Equals(e.RelativePath, relativePath, StringComparison.OrdinalIgnoreCase));

    /// <summary>取分类：一级目录名，根目录页面归 "pages"。</summary>
    public static string CategoryOf(string relativePath)
    {
        var idx = relativePath.IndexOf('/');
        return idx > 0 ? relativePath[..idx] : "pages";
    }

    /// <summary>解析 index.md。</summary>
    public static WikiIndex Parse(string markdown)
    {
        var index = new WikiIndex();
        var category = "pages";
        var regex = new Regex(@"^\s*-\s*\[(?<title>[^\]]+)\]\((?<path>[^)]+)\)");
        foreach (var raw in markdown.Replace("\r\n", "\n").Split('\n'))
        {
            var line = raw.Trim();
            if (line.StartsWith("## ")) { category = line[3..].Trim(); continue; }
            var m = regex.Match(line);
            if (!m.Success) continue;

            // 解析 [Title](path) 之后的可选后缀： — <summary> ｜ updated:<yyyy-MM-dd>
            var summary = default(string);
            DateTime? updated = null;
            var rest = line[m.Length..];
            if (rest.Length > 0)
            {
                var idx = rest.IndexOf(UpdatedMarker, StringComparison.Ordinal);
                var summaryPart = idx >= 0 ? rest[..idx] : rest;
                if (idx >= 0)
                {
                    var dateText = rest[(idx + UpdatedMarker.Length)..].Trim();
                    if (DateTime.TryParseExact(dateText, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                        updated = parsedDate.Date;
                }

                summaryPart = summaryPart.Trim();
                if (summaryPart.StartsWith('—')) summaryPart = summaryPart[1..].Trim();
                if (summaryPart.Length > 0) summary = summaryPart;
            }

            index.Entries.Add(new WikiIndexEntry
            {
                Category = category,
                RelativePath = m.Groups["path"].Value,
                Title = m.Groups["title"].Value,
                Summary = summary,
                Updated = updated
            });
        }
        return index;
    }

    /// <summary>渲染 index.md。</summary>
    public string Render()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("# Wiki Index\n\n");
        foreach (var group in Entries.GroupBy(e => e.Category, StringComparer.OrdinalIgnoreCase)
                     .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase))
        {
            sb.Append("## ").Append(group.Key).Append('\n');
            foreach (var e in group.OrderBy(e => e.Title, StringComparer.OrdinalIgnoreCase))
            {
                sb.Append("- [").Append(e.Title).Append("](").Append(e.RelativePath).Append(')');
                if (!string.IsNullOrWhiteSpace(e.Summary)) sb.Append(" — ").Append(e.Summary);
                if (e.Updated.HasValue) sb.Append(UpdatedMarker).Append(e.Updated.Value.ToString(DateFormat));
                sb.Append('\n');
            }
            sb.Append('\n');
        }
        return sb.ToString();
    }
}

/// <summary>lint 发现类型。</summary>
public enum LintFindingKind
{
    /// <summary>孤儿页：无任何入链。</summary>
    Orphan,
    /// <summary>死链：指向不存在的页面。</summary>
    DeadLink,
    /// <summary>未收录于 index.md。</summary>
    Unindexed,
    /// <summary>声明的来源文件不存在。</summary>
    MissingSource,
    /// <summary>来源文件比页面更新，建议重读。</summary>
    StaleCandidate
}

/// <summary>一条 lint 发现。</summary>
public class LintFinding
{
    /// <summary>类型。</summary>
    public LintFindingKind Kind { get; set; }
    /// <summary>相关页面相对路径。</summary>
    public string Path { get; set; } = "";
    /// <summary>说明。</summary>
    public string Detail { get; set; } = "";

    /// <summary>便于构造的便捷构造。</summary>
    public LintFinding() { }

    /// <summary>便于构造的便捷构造。</summary>
    public LintFinding(LintFindingKind kind, string path, string detail)
        => (Kind, Path, Detail) = (kind, path, detail);
}

/// <summary>lint 报告。</summary>
public class WikiLintReport
{
    /// <summary>发现集合。</summary>
    public List<LintFinding> Findings { get; set; } = new();
    /// <summary>扫描页面数（不含 log.md）。</summary>
    public int PageCount { get; set; }
}

/// <summary>wiki 统计。</summary>
public class WikiStats
{
    /// <summary>页面数（不含 log.md）。</summary>
    public int PageCount { get; set; }
    /// <summary>最近一次重建摘要。</summary>
    public string? LastRebuildSummary { get; set; }
}