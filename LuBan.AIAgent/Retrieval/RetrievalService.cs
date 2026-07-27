using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Retrieval.Chunkers;

namespace LuBan.AIAgent.Retrieval;

/// <summary>
/// 语义检索服务实现
/// </summary>
public class RetrievalService : IRetrievalService
{
    private const int EmbedBatchSize = 32;
    private readonly IVectorStore _store;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embedder;
    private readonly ChunkerFactory _chunkers;
    private readonly RetrievalToolOptions _options;
    private readonly SemaphoreSlim _writeGate = new(1, 1);
    private readonly SemaphoreSlim _readGate = new(1, 1);
    private int _readCount;

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
        await _writeGate.WaitAsync(cancellationToken);
        try
        {
            var report = new IndexReport();
            var root = Path.GetFullPath(path);
            var patterns = string.IsNullOrWhiteSpace(glob) ? new[] { "*" } : glob.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var files = patterns.SelectMany(p => Directory.EnumerateFiles(root, p, SearchOption.AllDirectories))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(f => _chunkers.ShouldIndex(f, root, _options.MaxFileSizeKB * 1024L))
                .ToList();
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
        finally { _writeGate.Release(); }
    }

    /// <inheritdoc />
    public async Task<IndexReport> IndexContentAsync(string content, string language, string sourceName, CancellationToken cancellationToken = default)
    {
        await _writeGate.WaitAsync(cancellationToken);
        try
        {
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)));
            var existing = await _store.GetFilesAsync(null);
            var old = existing.FirstOrDefault(f => f.FilePath == sourceName);
            if (old != null && old.FileHash == hash)
                return new IndexReport { ScannedFiles = 1, SkippedFiles = 1 };
            return await IndexSingleContentAsync(content, language, sourceName, hash, cancellationToken);
        }
        finally { _writeGate.Release(); }
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
            pairs.Add(new ChunkVectorPair { Chunk = chunks[i], Vector = reusedVectors[i] });

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
        await _readGate.WaitAsync(cancellationToken);
        try { _readCount++; }
        finally { _readGate.Release(); }
        try
        {
            var embeddings = await _embedder.GenerateAsync(new[] { query }, cancellationToken: cancellationToken);
            var queryVector = embeddings[0].Vector.ToArray();
            var entries = await _store.LoadVectorsAsync(pathPrefix, language, topK * 20);
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
        finally
        {
            await _readGate.WaitAsync(cancellationToken);
            try { _readCount--; }
            finally { _readGate.Release(); }
        }
    }

    /// <inheritdoc />
    public async Task<IndexStats> GetStatsAsync()
    {
        var s = await _store.GetStatsAsync();
        return new IndexStats { TotalFiles = s.FileCount, TotalChunks = s.ChunkCount, ModelId = s.ModelId, VectorDimension = s.Dimension };
    }
}
