/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Retrieval
*文件名： RetrievalService
*版本号： V1.0.0.0
*唯一标识：1ee50bee-84cf-4cd6-ba4a-8347d6408a8f
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：检索服务实现
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：检索服务实现
*
*****************************************************************************/

namespace LuBan.AIAgent.Retrieval;

/// <summary>
/// 语义检索服务实现
/// </summary>
public class RetrievalService : IRetrievalService
{
    private const int EmbedBatchSize = 32;
    private const int SearchOversampleFactor = 20;
    private readonly IVectorStore _store;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embedder;
    private readonly ChunkerFactory _chunkers;
    private readonly RetrievalToolOptions _options;
    private readonly AsyncReaderWriterLock _rwLock = new();

    /// <summary>
    /// 创建检索服务
    /// </summary>
    public RetrievalService(
        IVectorStore store,
        IEmbeddingGenerator<string, Embedding<float>> embedder,
        IOptions<LuBanAgentOptions> options,
        ChunkerFactory? chunkerFactory = null)
    {
        _store = store;
        _embedder = embedder;
        _options = options.Value.Tools.Retrieval;
        _chunkers = chunkerFactory ?? new ChunkerFactory();
    }

    /// <inheritdoc />
    public async Task<IndexReport> IndexDirectoryAsync(string path, string? glob = null, bool force = false, CancellationToken cancellationToken = default)
    {
        using var _ = await _rwLock.WriteLockAsync(cancellationToken);
        var report = new IndexReport();
        var root = Path.GetFullPath(path);
        var patterns = string.IsNullOrWhiteSpace(glob) ? new[] { "*" } : glob.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        List<string> files;
        try
        {
            files = patterns.SelectMany(p => SafeEnumerateFiles(root, p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(f => _chunkers.ShouldIndex(f, root, _options.MaxFileSizeKB * 1024L))
                .ToList();
        }
        catch (Exception ex)
        {
            Logger.Error("枚举索引目录文件失败", ex, path, glob ?? "");
            report.Errors.Add($"枚举文件失败: {ex.Message}");
            return report;
        }
        report.ScannedFiles = files.Count;

        var existing = (await _store.GetFilesAsync(root)).ToDictionary(f => f.FilePath, StringComparer.OrdinalIgnoreCase);
        var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var fullPath = Path.GetFullPath(file);
                seenPaths.Add(fullPath);
                var content = await File.ReadAllTextAsync(fullPath, cancellationToken);
                var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)));
                existing.TryGetValue(fullPath, out var existingFile);
                if (!force && existingFile != null && existingFile.FileHash == hash)
                {
                    report.SkippedFiles++;
                    continue;
                }
                bool isNew = existingFile == null;
                var r = await IndexSingleContentAsync(content, _chunkers.GetLanguage(fullPath), fullPath, hash, cancellationToken);
                report.TotalChunks += r.TotalChunks;
                report.EmbeddedChunks += r.EmbeddedChunks;
                report.ReusedChunks += r.ReusedChunks;
                if (isNew) report.NewFiles++; else report.UpdatedFiles++;
            }
            catch (Exception ex)
            {
                Logger.Error("索引单个文件失败", ex, file);
                report.Errors.Add($"{file}: {ex.Message}");
            }
        }

        foreach (var kv in existing)
        {
            if (!seenPaths.Contains(kv.Key) && kv.Key.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                await _store.SoftDeleteFileAsync(kv.Value.Id);
                report.DeletedFiles++;
            }
        }
        return report;
    }

    private static IEnumerable<string> SafeEnumerateFiles(string root, string pattern)
    {
        Queue<string> dirs = new();
        dirs.Enqueue(root);
        while (dirs.Count > 0)
        {
            var dir = dirs.Dequeue();
            string[] files;
            try { files = Directory.GetFiles(dir, pattern); }
            catch { continue; }
            foreach (var f in files) yield return f;
            string[] subDirs;
            try { subDirs = Directory.GetDirectories(dir); }
            catch { continue; }
            foreach (var d in subDirs) dirs.Enqueue(d);
        }
    }

    /// <inheritdoc />
    public async Task<IndexReport> IndexContentAsync(string content, string language, string sourceName, CancellationToken cancellationToken = default)
    {
        using var _ = await _rwLock.WriteLockAsync(cancellationToken);
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)));
        var existing = await _store.GetFilesAsync(null);
        var old = existing.FirstOrDefault(f => f.FilePath == sourceName);
        if (old != null && old.FileHash == hash)
            return new IndexReport { ScannedFiles = 1, SkippedFiles = 1 };
        return await IndexSingleContentAsync(content, language, sourceName, hash, cancellationToken);
    }

    private async Task<IndexReport> IndexSingleContentAsync(string content, string language, string filePath, string hash, CancellationToken ct)
    {
        var chunker = _chunkers.GetChunker(filePath);
        var chunks = chunker.Chunk(filePath, content);
        long fileId = await _store.UpsertFileAsync(filePath, hash, language, chunks.Count);

        var stored = await _store.GetFileChunksAsync(fileId);
        var storedByHash = new Dictionary<string, StoredChunk>();
        foreach (var s in stored) storedByHash.TryAdd(s.ContentHash, s);

        var reusedVectors = new float[chunks.Count][];
        var toEmbed = new List<(int index, CodeChunk chunk, string hash)>();
        for (int i = 0; i < chunks.Count; i++)
        {
            var h = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(chunks[i].Content)));
            if (storedByHash.TryGetValue(h, out var s)) reusedVectors[i] = s.Vector;
            else toEmbed.Add((i, chunks[i], h));
        }

        int embedded = 0;
        foreach (var batch in toEmbed.Chunk(EmbedBatchSize))
        {
            var embeddings = await _embedder.GenerateAsync(batch.Select(b => b.chunk.Content), cancellationToken: ct);
            for (int k = 0; k < batch.Length; k++) reusedVectors[batch[k].index] = embeddings[k].Vector.ToArray();
            embedded += batch.Length;
        }

        var pairs = new List<ChunkVectorPair>(chunks.Count);
        for (int i = 0; i < chunks.Count; i++)
        {
            if (reusedVectors[i] == null)
                throw new InvalidOperationException($"Chunk {i} has no vector (neither reused nor embedded)");
            pairs.Add(new ChunkVectorPair { Chunk = chunks[i], Vector = reusedVectors[i] });
        }

        await _store.ReplaceFileChunksAsync(fileId, _options.ModelId, pairs);

        return new IndexReport
        {
            ScannedFiles = 1, NewFiles = 1, TotalChunks = chunks.Count,
            EmbeddedChunks = embedded, ReusedChunks = chunks.Count - embedded
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RetrievalResult>> SearchAsync(string query, int topK = 5, string? pathPrefix = null, string? language = null, CancellationToken cancellationToken = default)
    {
        topK = Math.Clamp(topK, 1, 20);
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<RetrievalResult>();
        using var _ = await _rwLock.ReadLockAsync(cancellationToken);
        var embeddings = await _embedder.GenerateAsync(new[] { query }, cancellationToken: cancellationToken);
        var queryVector = embeddings[0].Vector.ToArray();
        var entries = await _store.LoadVectorsAsync(pathPrefix, language, topK * SearchOversampleFactor);
        var scored = entries
            .Select(e => (e.ChunkId, Score: VectorMath.Cosine(queryVector, e.Vector)))
            .OrderByDescending(x => x.Score)
            .Take(topK)
            .ToList();
        var map = await _store.GetChunksAsync(scored.Select(s => s.ChunkId).ToList());
        var results = new List<RetrievalResult>();
        foreach (var (chunkId, score) in scored)
        {
            if (!map.TryGetValue(chunkId, out var c)) continue;
            results.Add(new RetrievalResult
            {
                ChunkId = chunkId, FilePath = c.FilePath, StartLine = c.StartLine, EndLine = c.EndLine,
                ChunkType = c.ChunkType, SymbolName = c.SymbolName, Content = c.Content, Score = score
            });
        }
        return results;
    }

    /// <inheritdoc />
    public async Task<IndexStats> GetStatsAsync()
    {
        var s = await _store.GetStatsAsync();
        return new IndexStats { TotalFiles = s.FileCount, TotalChunks = s.ChunkCount, ModelId = s.ModelId, VectorDimension = s.Dimension };
    }
}
