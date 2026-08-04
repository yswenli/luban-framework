/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.LocalMemory
*文件名： LocalMemoryService
*版本号： V1.0.0.0
*唯一标识：a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/4
*描述：本地长期记忆服务实现，基于本地 Embedding + SQLite 向量检索
*
*=================================================
*修改标记
*修改时间：2026/8/4
*修改人： yswenli
*版本号： V1.0.0.0
*描述：本地长期记忆服务实现
*
*****************************************************************************/
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace LuBan.AIAgent.LocalMemory;

/// <summary>
/// 本地长期记忆服务实现，基于本地 Embedding + SQLite 向量检索
/// </summary>
public class LocalMemoryService : ILocalMemoryService
{
    private readonly ILocalMemoryStore _store;
    private readonly IEmbeddingGenerator<string, Embedding<float>>? _embedder;
    private readonly LocalMemoryOptions _options;

    /// <summary>
    /// 创建本地记忆服务
    /// </summary>
    public LocalMemoryService(
        ILocalMemoryStore store,
        IOptions<LocalMemoryOptions> options,
        IEmbeddingGenerator<string, Embedding<float>>? embedder = null)
    {
        _store = store;
        _options = options.Value;
        _embedder = embedder;
    }

    /// <inheritdoc />
    public async Task<MemoryEntry> SaveAsync(string content, string category = "general", CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("记忆内容不能为空", nameof(content));

        category = string.IsNullOrWhiteSpace(category) ? "general" : category;

        var entry = new MemoryEntry
        {
            Id = Guid.NewGuid().ToString("N"),
            Content = content.Trim(),
            Category = category.Trim().ToLowerInvariant(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            VectorDimension = 0
        };

        var vector = await GenerateEmbeddingAsync(entry.Content, cancellationToken);
        entry.VectorDimension = vector.Length;
        await _store.SaveAsync(entry, ToBytes(vector), cancellationToken);
        return entry;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MemorySearchResult>> SearchAsync(string query, string? category = null, int topK = 5, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query)) return Array.Empty<MemorySearchResult>();
        topK = Math.Max(1, topK);

        var queryVector = await GenerateEmbeddingAsync(query, cancellationToken);
        var all = await _store.LoadAllAsync(category, cancellationToken);

        if (all.Count == 0) return Array.Empty<MemorySearchResult>();

        var scored = new List<MemorySearchResult>(all.Count);
        foreach (var (entry, bytes) in all)
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
                Score = score
            });
        }

        return scored
            .OrderByDescending(r => r.Score)
            .Take(topK)
            .ToList();
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<MemoryEntry>> ListAsync(string? category = null, int limit = 100, CancellationToken cancellationToken = default)
        => _store.ListAsync(category, limit, cancellationToken);

    /// <inheritdoc />
    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        => _store.DeleteAsync(id, cancellationToken);

    private async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken)
    {
        if (_embedder == null)
        {
            // 退化方案：未提供本地 embedder 时，返回基于词袋哈希的固定长度向量
            // 这样可以保证在没有 ONNX 模型时 localmemory 仍可工作（语义能力较弱）
            return FallbackEmbedding(text, _options.FallbackDimension);
        }

        var embedding = await _embedder.GenerateAsync(text, cancellationToken: cancellationToken);
        return embedding.Vector.ToArray();
    }

    private static float[] FallbackEmbedding(string text, int dimension)
    {
        var vector = new float[dimension];
        var words = text.Split(new[] { ' ', '\t', '\n', '\r', '，', '。', '；', '！', '？', ',', '.', ';', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var word in words)
        {
            var hash = word.GetHashCode();
            var idx = Math.Abs(hash % dimension);
            vector[idx] += 1.0f;
        }
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
