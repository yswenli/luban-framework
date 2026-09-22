/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Wiki
*文件名： WikiService
*版本号： V1.0.0.0
*唯一标识：d7c4a2e9-1f63-4b28-9a05-8e3c6f1b7d94
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/20
*描述：wiki 知识库服务默认实现
*
*****************************************************************************/
using System.Text.RegularExpressions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Retrieval;
using LuBan.AIAgent.Retrieval.Chunkers;

namespace LuBan.AIAgent.Wiki;

/// <summary>wiki 服务默认实现。</summary>
public class WikiService : IWikiService
{
    private const string WikiFolder = "wiki";
    private readonly IRetrievalService _retrieval;
    private readonly IWikiContext _context;
    private readonly WikiToolOptions _options;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private IndexReport? _lastRebuild;

    /// <summary>构造。</summary>
    public WikiService(IRetrievalService retrieval, IWikiContext context, WikiToolOptions options)
    {
        _retrieval = retrieval;
        _context = context;
        _options = options;
    }

    private string RequireRoot(string? workspaceId = null)
    {
        var root = _context.WorkspaceRootFor(workspaceId);
        if (string.IsNullOrWhiteSpace(root))
            throw new InvalidOperationException("当前没有工作区上下文，无法访问 wiki。");
        return Path.Combine(root, WikiFolder);
    }

    private static string NormalizeRel(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("路径不能为空。", nameof(path));
        return path.Replace('\\', '/').Trim('/');
    }

    private static string ToFull(string wikiRoot, string relative)
    {
        var normalized = NormalizeRel(relative).Replace('/', Path.DirectorySeparatorChar);
        var full = Path.GetFullPath(Path.Combine(wikiRoot, normalized));
        var rootFull = Path.GetFullPath(wikiRoot);
        if (!full.Equals(rootFull, StringComparison.OrdinalIgnoreCase)
            && !full.StartsWith(rootFull + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"wiki 页路径越界: {relative}", nameof(relative));
        return full;
    }

    private static List<string> EnumeratePages(string wikiRoot)
    {
        if (!Directory.Exists(wikiRoot)) return new List<string>();
        // 与工作区索引同一套枚举口径：跳过排除目录与重解析点，单层不可访问不中断整体。
        return ChunkerFactory.EnumerateFiles(wikiRoot, "*.md")
            .Where(f => !string.Equals(Path.GetFileName(f), "log.md", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private static async Task AppendLogAsync(string wikiRoot, string verb, string detail, CancellationToken ct)
    {
        var path = Path.Combine(wikiRoot, "log.md");
        var line = $"## [{DateTime.Today:yyyy-MM-dd}] {verb} | {detail}{Environment.NewLine}";
        await File.AppendAllTextAsync(path, line, ct);
    }

    /// <inheritdoc />
    public async Task<WikiIndex> ReadIndexAsync(CancellationToken cancellationToken = default, string? workspaceId = null)
    {
        var path = Path.Combine(RequireRoot(workspaceId), "index.md");
        if (!File.Exists(path)) return new WikiIndex();
        return WikiIndex.Parse(await File.ReadAllTextAsync(path, cancellationToken));
    }

    /// <inheritdoc />
    public async Task<WikiPage> ReadPageAsync(string relativePath, CancellationToken cancellationToken = default, string? workspaceId = null)
    {
        var full = ToFull(RequireRoot(workspaceId), relativePath);
        if (!File.Exists(full)) throw new FileNotFoundException($"wiki 页不存在: {relativePath}", full);
        return WikiPageSerializer.Parse(NormalizeRel(relativePath), await File.ReadAllTextAsync(full, cancellationToken));
    }

    /// <inheritdoc />
    public async Task SavePageAsync(WikiPage page, CancellationToken cancellationToken = default, string? workspaceId = null)
    {
        await _writeLock.WaitAsync(cancellationToken);
        try
        {
            var root = RequireRoot(workspaceId);
            var rel = NormalizeRel(page.RelativePath);
            if (string.IsNullOrWhiteSpace(rel) || rel.Equals("index.md", StringComparison.OrdinalIgnoreCase) || rel.Equals("log.md", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("页面路径非法（不得为空或为 index.md/log.md）。", nameof(page));

            var full = ToFull(root, rel);
            Directory.CreateDirectory(Path.GetDirectoryName(full)!);
            if (File.Exists(full) && page.Created == null)
                page.Created = WikiPageSerializer.Parse(rel, await File.ReadAllTextAsync(full, cancellationToken)).Created;
            page.Updated = DateTime.Today;
            await File.WriteAllTextAsync(full, WikiPageSerializer.Render(page), cancellationToken);

            var index = await ReadIndexAsync(cancellationToken, workspaceId);
            index.Upsert(rel, page);
            await File.WriteAllTextAsync(Path.Combine(root, "index.md"), index.Render(), cancellationToken);

            await AppendLogAsync(root, "save", $"{rel}（{page.Title}）", cancellationToken);

            try { await _retrieval.IndexFileAsync(full, force: true, cancellationToken: cancellationToken, workspaceId: workspaceId); }
            catch (Exception ex) { Logger.Warn($"wiki 页重索引失败: {rel} - {ex.Message}", ex); }
        }
        finally { _writeLock.Release(); }
    }

    /// <inheritdoc />
    public async Task DeletePageAsync(string relativePath, CancellationToken cancellationToken = default, string? workspaceId = null)
    {
        await _writeLock.WaitAsync(cancellationToken);
        try
        {
            var root = RequireRoot(workspaceId);
            var rel = NormalizeRel(relativePath);
            if (rel.Equals("index.md", StringComparison.OrdinalIgnoreCase) || rel.Equals("log.md", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("不得删除 index.md/log.md。", nameof(relativePath));

            var full = ToFull(root, rel);
            if (File.Exists(full)) File.Delete(full);

            Directory.CreateDirectory(root);
            var index = await ReadIndexAsync(cancellationToken, workspaceId);
            index.Remove(rel);
            await File.WriteAllTextAsync(Path.Combine(root, "index.md"), index.Render(), cancellationToken);
            await AppendLogAsync(root, "delete", rel, cancellationToken);

            try { await _retrieval.RemoveAsync(full, cancellationToken, workspaceId); }
            catch (Exception ex) { Logger.Warn($"wiki 页向量删除失败: {rel} - {ex.Message}", ex); }
        }
        finally { _writeLock.Release(); }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RetrievalResult>> SearchAsync(string query, int? topK = null, bool? includeRaw = null,
        CancellationToken cancellationToken = default, string? workspaceId = null)
    {
        var root = RequireRoot(workspaceId);
        var effectiveTopK = topK ?? _options.TopK;
        var effectiveIncludeRaw = includeRaw ?? _options.IncludeRawDefault;
        var results = (await _retrieval.SearchAsync(query, effectiveTopK, root, null, cancellationToken, workspaceId)).ToList();
        if (effectiveIncludeRaw)
        {
            var seen = new HashSet<string>(results.Select(r => r.FilePath), StringComparer.OrdinalIgnoreCase);
            foreach (var r in await _retrieval.SearchAsync(query, effectiveTopK, null, null, cancellationToken, workspaceId))
                if (seen.Add(r.FilePath)) results.Add(r);
        }
        return results;
    }

    /// <inheritdoc />
    public async Task<IndexReport> RebuildIndexAsync(CancellationToken cancellationToken = default, string? workspaceId = null)
    {
        // 与 SavePage/DeletePage 共用写锁，避免重建与单页写在 index.md/向量库上交错
        await _writeLock.WaitAsync(cancellationToken);
        try
        {
            var root = RequireRoot(workspaceId);
            Directory.CreateDirectory(root);
            var report = new IndexReport();
            var pages = EnumeratePages(root);

            foreach (var page in pages)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var r = await _retrieval.IndexFileAsync(page, force: true, cancellationToken: cancellationToken, workspaceId: workspaceId);
                    report.ScannedFiles++;
                    report.TotalChunks += r.TotalChunks;
                }
                catch (Exception ex) { report.Errors.Add($"{page}: {ex.Message}"); }
            }

            var index = await ReadIndexAsync(cancellationToken, workspaceId);
            var known = new HashSet<string>(pages.Select(p => NormalizeRel(Path.GetRelativePath(root, p))), StringComparer.OrdinalIgnoreCase);
            foreach (var entry in index.Entries.ToList())
            {
                if (known.Contains(entry.RelativePath)) continue;
                try
                {
                    await _retrieval.RemoveAsync(ToFull(root, entry.RelativePath), cancellationToken, workspaceId);
                    report.DeletedFiles++;
                }
                catch (Exception ex) { report.Errors.Add($"{entry.RelativePath}: {ex.Message}"); }
            }

            _lastRebuild = report;
            return report;
        }
        finally { _writeLock.Release(); }
    }

    /// <inheritdoc />
    public async Task<WikiLintReport> LintAsync(CancellationToken cancellationToken = default, string? workspaceId = null)
    {
        // 与 SavePage/DeletePage 共用写锁，避免 lint 读到写入中的中间态
        await _writeLock.WaitAsync(cancellationToken);
        try
        {
            var root = RequireRoot(workspaceId);
            var workspaceRoot = _context.WorkspaceRootFor(workspaceId);
            var pages = EnumeratePages(root);
            var rels = new HashSet<string>(pages.Select(p => NormalizeRel(Path.GetRelativePath(root, p))), StringComparer.OrdinalIgnoreCase);
            var index = await ReadIndexAsync(cancellationToken, workspaceId);
            var indexed = new HashSet<string>(index.Entries.Select(e => e.RelativePath), StringComparer.OrdinalIgnoreCase);
            var findings = new List<LintFinding>();
            var inbound = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var linkRegex = new Regex(@"\]\(([^)]+\.md)\)", RegexOptions.Compiled);

            foreach (var page in pages)
            {
                var rel = NormalizeRel(Path.GetRelativePath(root, page));
                var parsed = WikiPageSerializer.Parse(rel, await File.ReadAllTextAsync(page, cancellationToken));

                // index.md/overview.md 为保留页，天然不进入 index.Entries，不算"未收录"（与下方孤儿页豁免一致）
                if (!rel.Equals("index.md", StringComparison.OrdinalIgnoreCase)
                    && !rel.Equals("overview.md", StringComparison.OrdinalIgnoreCase)
                    && !indexed.Contains(rel))
                    findings.Add(new LintFinding(LintFindingKind.Unindexed, rel, "页面未收录于 index.md"));

                foreach (var src in parsed.Sources.Where(s => s.Contains('/') && !s.StartsWith("http", StringComparison.OrdinalIgnoreCase)))
                    if (!File.Exists(ToFull(workspaceRoot!, src)))
                        findings.Add(new LintFinding(LintFindingKind.MissingSource, rel, $"来源缺失: {src}"));

                var dir = rel.Contains('/') ? rel[..rel.LastIndexOf('/')] : "";
                foreach (Match m in linkRegex.Matches(parsed.Body))
                {
                    var target = m.Groups[1].Value;
                    if (target.StartsWith("http", StringComparison.OrdinalIgnoreCase)) continue;
                    var targetRel = NormalizeRel(string.IsNullOrEmpty(dir) ? target : $"{dir}/{target}");
                    if (!rels.Contains(targetRel))
                        findings.Add(new LintFinding(LintFindingKind.DeadLink, rel, $"死链: {target}"));
                    else
                        inbound.Add(targetRel);
                }
            }

            foreach (var rel in rels)
                if (!rel.Equals("index.md", StringComparison.OrdinalIgnoreCase)
                    && !rel.Equals("overview.md", StringComparison.OrdinalIgnoreCase)
                    && !inbound.Contains(rel)
                    && !index.Entries.Any(e => string.Equals(e.RelativePath, rel, StringComparison.OrdinalIgnoreCase) && e.Category == "sources"))
                    findings.Add(new LintFinding(LintFindingKind.Orphan, rel, "无入链"));

            foreach (var page in pages)
            {
                var rel = NormalizeRel(Path.GetRelativePath(root, page));
                var parsed = WikiPageSerializer.Parse(rel, await File.ReadAllTextAsync(page, cancellationToken));
                if (parsed.Updated == null) continue;
                foreach (var src in parsed.Sources.Where(s => s.Contains('/') && !s.StartsWith("http", StringComparison.OrdinalIgnoreCase)))
                {
                    var full = ToFull(workspaceRoot!, src);
                    if (File.Exists(full) && File.GetLastWriteTime(full).Date > parsed.Updated.Value.Date)
                        findings.Add(new LintFinding(LintFindingKind.StaleCandidate, rel, $"来源较新: {src}"));
                }
            }

            return new WikiLintReport { Findings = findings, PageCount = rels.Count };
        }
        finally { _writeLock.Release(); }
    }

    /// <inheritdoc />
    public Task<WikiStats> GetStatsAsync(CancellationToken cancellationToken = default, string? workspaceId = null)
    {
        var root = RequireRoot(workspaceId);
        var last = _lastRebuild == null
            ? null
            : $"扫描 {_lastRebuild.ScannedFiles}，删除 {_lastRebuild.DeletedFiles}，切块 {_lastRebuild.TotalChunks}";
        return Task.FromResult(new WikiStats { PageCount = EnumeratePages(root).Count, LastRebuildSummary = last });
    }
}