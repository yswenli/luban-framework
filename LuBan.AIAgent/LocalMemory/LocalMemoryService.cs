/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.LocalMemory
*文件名： LocalMemoryService
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：本地长期记忆服务实现
*
*****************************************************************************/
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Retrieval;

namespace LuBan.AIAgent.LocalMemory;

/// <summary>
/// 本地长期记忆服务实现，基于本地 Embedding + SQLite 向量检索。
/// 特性：工作区隔离、内容去重、可选 TTL、fallback 字符 n-gram 向量 + 倒排索引预筛。
/// </summary>
public class LocalMemoryService : ILocalMemoryService, IDisposable
{
    private readonly ILocalMemoryStore _store;
    private readonly IEmbeddingGenerator<string, Embedding<float>>? _embedder;
    private readonly LocalMemoryOptions _options;
    private readonly IWorkspaceContextProvider? _workspaceContext;
    private readonly IUserContext? _userContext;

    /// <summary>过期清理的节流间隔：清理不再挂在检索热路径上，由后台循环按此间隔执行。</summary>
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(5);

    private readonly CancellationTokenSource _cleanupCts = new();
    private int _cleanupState; // 0=未启动 1=已启动 2=已释放

    private readonly object _indexLock = new();
    private Dictionary<uint, HashSet<string>>? _inverted;
    private bool _indexDirty = true;

    /// <summary>
    /// 创建本地记忆服务
    /// </summary>
    /// <param name="store">本地记忆持久化存储</param>
    /// <param name="options">本地记忆配置选项</param>
    /// <param name="userContext">当前用户上下文（可选；为 null 时使用空 userId，等价未归属）</param>
    /// <param name="embedder">Embedding 生成器；为 null 时使用 fallback n-gram 向量</param>
    /// <param name="workspaceContext">当前工作区上下文提供者；为 null 时不按工作区隔离</param>
    public LocalMemoryService(
        ILocalMemoryStore store,
        IOptions<LocalMemoryOptions> options,
        IUserContext? userContext = null,
        IEmbeddingGenerator<string, Embedding<float>>? embedder = null,
        IWorkspaceContextProvider? workspaceContext = null)
    {
        _store = store;
        _options = options.Value;
        _userContext = userContext;
        _embedder = embedder;
        _workspaceContext = workspaceContext;
    }

    private string CurrentUserId => _userContext?.UserId ?? string.Empty;

    /// <inheritdoc />
    public async Task<MemoryEntry> SaveAsync(string content, string category = "general", CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("记忆内容不能为空", nameof(content));

        category = string.IsNullOrWhiteSpace(category) ? "general" : category.Trim().ToLowerInvariant();

        var workspaceId = category == MemoryCategories.Global ? null : _workspaceContext?.CurrentWorkspaceId;

        var now = DateTime.UtcNow;
        var entry = new MemoryEntry
        {
            Id = Guid.NewGuid().ToString("N"),
            Content = content.Trim(),
            Category = category,
            CreatedAt = now,
            UpdatedAt = now,
            VectorDimension = 0,
            WorkspaceId = workspaceId,
            UserId = CurrentUserId,
            ContentHash = ComputeContentHash(content),
            ExpiresAt = _options.TtlDays.HasValue ? now.AddDays(_options.TtlDays.Value) : null
        };

        var vector = await GenerateEmbeddingAsync(entry.Content, cancellationToken);
        entry.VectorDimension = vector.Length;
        var saved = await _store.UpsertAsync(CurrentUserId, entry, ToBytes(vector), cancellationToken);

        lock (_indexLock)
            _indexDirty = true;

        return saved;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MemorySearchResult>> SearchAsync(string query, string? category = null, int topK = 5, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query)) return Array.Empty<MemorySearchResult>();
        topK = Math.Max(1, topK);

        // 过期清理移出检索热路径：只在首次检索时启动一个按间隔执行的后台循环，
        // 避免「每次召回都 fire-and-forget 一个 DB 删除」与 SQLite 写锁竞争。
        EnsureCleanupLoop();

        var workspaceId = category == MemoryCategories.Global ? null : _workspaceContext?.CurrentWorkspaceId;

        // fallback 模式：倒排索引预筛；ONNX 模式：全量扫描。
        // 两条路径都先取候选、判定为空，再计算查询向量：空库/空候选时不做嵌入，
        // 否则一次空检索就会把嵌入模型（数十 MB 原生内存）拉起来。
        if (_embedder == null)
        {
            var candidates = await GetCandidates(query, cancellationToken);
            if (candidates.Count == 0) return Array.Empty<MemorySearchResult>();

            var rows = await _store.LoadByIdsAsync(CurrentUserId, candidates, workspaceId, cancellationToken);
            if (category != null)
                rows = rows.Where(r => r.Entry.Category == category).ToList();
            if (rows.Count == 0) return Array.Empty<MemorySearchResult>();

            var fallbackVector = await GenerateEmbeddingAsync(query, cancellationToken);
            return ScoreRows(fallbackVector, rows, topK);
        }

        var all = await _store.LoadAllAsync(CurrentUserId, category, workspaceId, cancellationToken: cancellationToken);
        if (all.Count == 0) return Array.Empty<MemorySearchResult>();

        var queryVector = await GenerateEmbeddingAsync(query, cancellationToken);
        return ScoreRows(queryVector, all, topK);
    }

    /// <summary>
    /// 首次检索时启动过期清理循环（幂等）：立即清理一次，之后按 <see cref="CleanupInterval"/> 周期执行。
    /// </summary>
    private void EnsureCleanupLoop()
    {
        if (Interlocked.CompareExchange(ref _cleanupState, 1, 0) != 0) return;
        _ = CleanupLoopAsync();
    }

    /// <summary>
    /// 后台过期清理循环；随服务 scope 释放而取消。
    /// </summary>
    private async Task CleanupLoopAsync()
    {
        try
        {
            while (!_cleanupCts.IsCancellationRequested)
            {
                await _store.DeleteExpiredAsync(CurrentUserId);
                lock (_indexLock)
                    _indexDirty = true;
                await Task.Delay(CleanupInterval, _cleanupCts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // 服务释放：正常退出
        }
        catch (Exception ex)
        {
            Logger.Error("Expired cleanup failed", ex);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _cleanupState, 2) == 2) return;
        // 只取消、不 Dispose 令牌源：循环可能正持有该令牌等待，Dispose 会造成竞争异常。
        try { _cleanupCts.Cancel(); }
        catch (ObjectDisposedException) { }
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<MemoryEntry>> ListAsync(string? category = null, int limit = 100, CancellationToken cancellationToken = default)
        => _store.ListAsync(CurrentUserId, category, _workspaceContext?.CurrentWorkspaceId, limit, cancellationToken);

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var ok = await _store.DeleteAsync(CurrentUserId, id, cancellationToken);
        if (ok)
            lock (_indexLock)
                _indexDirty = true;
        return ok;
    }

    private async Task<List<string>> GetCandidates(string query, CancellationToken cancellationToken)
    {
        var grams = NGramExtractor.ExtractHashes(query).ToHashSet();
        if (grams.Count == 0) return new List<string>();

        lock (_indexLock)
        {
            if (_inverted != null && !_indexDirty)
                return CollectCandidates(grams);
        }

        var all = await _store.LoadAllAsync(CurrentUserId, category: null, workspaceId: null, includeAllWorkspaces: true, cancellationToken: cancellationToken);
        var rebuilt = new Dictionary<uint, HashSet<string>>();
        foreach (var (entry, _) in all)
        {
            foreach (var gram in NGramExtractor.ExtractHashes(entry.Content))
            {
                if (!rebuilt.TryGetValue(gram, out var set))
                    rebuilt[gram] = set = new HashSet<string>();
                set.Add(entry.Id);
            }
        }
        lock (_indexLock)
        {
            _inverted = rebuilt;
            _indexDirty = false;
        }
        return CollectCandidates(grams);
    }

    private List<string> CollectCandidates(HashSet<uint> grams)
    {
        var candidates = new HashSet<string>();
        if (_inverted == null) return new List<string>();
        foreach (var gram in grams)
        {
            if (_inverted.TryGetValue(gram, out var ids))
                candidates.UnionWith(ids);
        }
        return candidates.ToList();
    }

    private static IReadOnlyList<MemorySearchResult> ScoreRows(
        float[] queryVector,
        IReadOnlyList<(MemoryEntry Entry, byte[] VectorBytes)> rows,
        int topK)
    {
        if (topK <= 0 || rows.Count == 0) return Array.Empty<MemorySearchResult>();

        // 有界 top-K：只维护 K 条结果，避免为整表每行都建 MemorySearchResult 再全量排序。
        var top = new List<(MemorySearchResult Result, double Score)>(Math.Min(topK, rows.Count));
        foreach (var (entry, bytes) in rows)
        {
            // 维度不符/空向量直接跳过（与原实现一致）
            if (bytes.Length == 0 || bytes.Length / sizeof(float) != queryVector.Length) continue;

            // 零拷贝打分：直接在同一块 byte 缓冲上按 float 读取，不再「每行 new float[]」
            var score = VectorMath.Cosine(queryVector, bytes);
            if (top.Count >= topK && score <= top[^1].Score) continue;

            var result = new MemorySearchResult
            {
                Id = entry.Id,
                Content = entry.Content,
                Category = entry.Category,
                CreatedAt = entry.CreatedAt,
                UpdatedAt = entry.UpdatedAt,
                VectorDimension = entry.VectorDimension,
                WorkspaceId = entry.WorkspaceId,
                ContentHash = entry.ContentHash,
                ExpiresAt = entry.ExpiresAt,
                Score = score
            };

            // 同分按原始行序（稳定排序），插入到第一条分数更低的位置之前
            var index = top.Count;
            for (var i = 0; i < top.Count; i++)
            {
                if (score > top[i].Score) { index = i; break; }
            }
            top.Insert(index, (result, score));
            if (top.Count > topK) top.RemoveAt(top.Count - 1);
        }

        return top.Select(t => t.Result).ToList();
    }

    private static string ComputeContentHash(string content)
        => TextUtils.ComputeContentHash(content);

    private async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken)
    {
        if (_embedder == null)
            return FallbackEmbedding(text, _options.FallbackDimension);

        var embedding = await _embedder.GenerateAsync(text, cancellationToken: cancellationToken);
        return embedding.Vector.ToArray();
    }

    private static float[] FallbackEmbedding(string text, int dimension)
    {
        dimension = Math.Max(1, dimension);
        var vector = new float[dimension];
        foreach (var gram in NGramExtractor.ExtractHashes(text))
            vector[gram % dimension] += 1.0f;
        Normalize(vector);
        return vector;
    }

    private static void Normalize(float[] vector)
    {
        double sum = 0;
        foreach (var v in vector) sum += v * v;
        if (sum == 0) return;
        var norm = (float)Math.Sqrt(sum);
        for (int i = 0; i < vector.Length; i++) vector[i] /= norm;
    }

    private static byte[] ToBytes(float[] vector)
    {
        var bytes = new byte[vector.Length * sizeof(float)];
        Buffer.BlockCopy(vector, 0, bytes, 0, bytes.Length);
        return bytes;
    }
}
