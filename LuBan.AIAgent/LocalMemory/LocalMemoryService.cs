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
namespace LuBan.AIAgent.LocalMemory;

/// <summary>
/// 本地长期记忆服务实现，基于本地 Embedding + SQLite 向量检索。
/// 特性：工作区隔离、内容去重、可选 TTL、fallback 字符 n-gram 向量 + 倒排索引预筛。
/// </summary>
public class LocalMemoryService : ILocalMemoryService
{
    private readonly ILocalMemoryStore _store;
    private readonly IEmbeddingGenerator<string, Embedding<float>>? _embedder;
    private readonly LocalMemoryOptions _options;
    private readonly IWorkspaceContextProvider? _workspaceContext;

    private readonly object _indexLock = new();
    private Dictionary<uint, HashSet<string>>? _inverted;
    private bool _indexDirty = true;

    /// <summary>
    /// 创建本地记忆服务
    /// </summary>
    /// <param name="store">本地记忆持久化存储</param>
    /// <param name="options">本地记忆配置选项</param>
    /// <param name="embedder">Embedding 生成器；为 null 时使用 fallback n-gram 向量</param>
    /// <param name="workspaceContext">当前工作区上下文提供者；为 null 时不按工作区隔离</param>
    public LocalMemoryService(
        ILocalMemoryStore store,
        IOptions<LocalMemoryOptions> options,
        IEmbeddingGenerator<string, Embedding<float>>? embedder = null,
        IWorkspaceContextProvider? workspaceContext = null)
    {
        _store = store;
        _options = options.Value;
        _embedder = embedder;
        _workspaceContext = workspaceContext;
    }

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
            ContentHash = ComputeContentHash(content),
            ExpiresAt = _options.TtlDays.HasValue ? now.AddDays(_options.TtlDays.Value) : null
        };

        var vector = await GenerateEmbeddingAsync(entry.Content, cancellationToken);
        entry.VectorDimension = vector.Length;
        var saved = await _store.UpsertAsync(entry, ToBytes(vector), cancellationToken);

        lock (_indexLock)
            _indexDirty = true;

        return saved;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MemorySearchResult>> SearchAsync(string query, string? category = null, int topK = 5, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query)) return Array.Empty<MemorySearchResult>();
        topK = Math.Max(1, topK);

        _ = Task.Run(async () =>
        {
            try
            {
                await _store.DeleteExpiredAsync(default);
                lock (_indexLock)
                    _indexDirty = true;
            }
            catch (Exception ex)
            {
                Logger.Error("Expired cleanup failed", ex);
            }
        });

        var workspaceId = category == MemoryCategories.Global ? null : _workspaceContext?.CurrentWorkspaceId;
        var queryVector = await GenerateEmbeddingAsync(query, cancellationToken);

        // fallback 模式：倒排索引预筛；ONNX 模式：全量扫描
        if (_embedder == null)
        {
            var candidates = await GetCandidates(query, cancellationToken);
            if (candidates.Count > 0)
            {
                var rows = await _store.LoadByIdsAsync(candidates, workspaceId, cancellationToken);
                if (category != null)
                    rows = rows.Where(r => r.Entry.Category == category).ToList();
                return ScoreRows(queryVector, rows, topK);
            }
        }

        var all = await _store.LoadAllAsync(category, workspaceId, cancellationToken: cancellationToken);
        return ScoreRows(queryVector, all, topK);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<MemoryEntry>> ListAsync(string? category = null, int limit = 100, CancellationToken cancellationToken = default)
        => _store.ListAsync(category, _workspaceContext?.CurrentWorkspaceId, limit, cancellationToken);

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var ok = await _store.DeleteAsync(id, cancellationToken);
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

        var all = await _store.LoadAllAsync(category: null, workspaceId: null, includeAllWorkspaces: true, cancellationToken: cancellationToken);
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
        var scored = new List<MemorySearchResult>(rows.Count);
        foreach (var (entry, bytes) in rows)
        {
            var storedVector = ToFloats(bytes);
            if (storedVector.Length == 0 || storedVector.Length != queryVector.Length) continue;
            var score = CosineSimilarity(queryVector, storedVector);
            scored.Add(new MemorySearchResult
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
            });
        }
        return scored
            .OrderByDescending(r => r.Score)
            .Take(topK)
            .ToList();
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

    private static double CosineSimilarity(float[] a, float[] b)
    {
        double dot = 0, normA = 0, normB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }
        if (normA == 0 || normB == 0) return 0;
        return dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
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

    private static float[] ToFloats(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0) return Array.Empty<float>();
        var floats = new float[bytes.Length / sizeof(float)];
        Buffer.BlockCopy(bytes, 0, floats, 0, bytes.Length);
        return floats;
    }
}
