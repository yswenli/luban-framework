using LuBan.AIAgent.Retrieval;
using System.Security.Cryptography;
using System.Text;

namespace LuBan.AIAgent.Tests.Retrieval.Fakes;

public class FakeVectorStore : IVectorStore
{
    private readonly Dictionary<long, IndexedFile> _files = new();
    private readonly Dictionary<long, (long fileId, CodeChunk chunk, string hash, float[] vector, string modelId)> _chunks = new();
    private long _nextFileId = 1, _nextChunkId = 1;

    public int ReplaceCallCount { get; private set; }

    public Task<IReadOnlyList<IndexedFile>> GetFilesAsync(string? pathPrefix = null)
    {
        var q = _files.Values.AsEnumerable();
        if (!string.IsNullOrEmpty(pathPrefix)) q = q.Where(f => f.FilePath.StartsWith(pathPrefix, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult<IReadOnlyList<IndexedFile>>(q.ToList());
    }

    public Task<long> UpsertFileAsync(string filePath, string fileHash, string language, int chunkCount)
    {
        var existing = _files.Values.FirstOrDefault(f => f.FilePath == filePath);
        if (existing != null)
        {
            existing.FileHash = fileHash;
            existing.Language = language;
            return Task.FromResult(existing.Id);
        }
        var id = _nextFileId++;
        _files[id] = new IndexedFile { Id = id, FilePath = filePath, FileHash = fileHash, Language = language };
        return Task.FromResult(id);
    }

    public Task SoftDeleteFileAsync(long fileId)
    {
        _files.Remove(fileId);
        foreach (var kv in _chunks.Where(c => c.Value.fileId == fileId).ToList()) _chunks.Remove(kv.Key);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<StoredChunk>> GetFileChunksAsync(long fileId)
    {
        var list = _chunks.Values.Where(c => c.fileId == fileId).Select(c => new StoredChunk { Id = c.chunk.ChunkIndex, ChunkIndex = c.chunk.ChunkIndex, ContentHash = c.hash, Vector = c.vector }).ToList();
        return Task.FromResult<IReadOnlyList<StoredChunk>>(list);
    }

    public Task ReplaceFileChunksAsync(long fileId, string modelId, IReadOnlyList<ChunkVectorPair> chunks)
    {
        ReplaceCallCount++;
        foreach (var kv in _chunks.Where(c => c.Value.fileId == fileId).ToList()) _chunks.Remove(kv.Key);
        foreach (var p in chunks)
        {
            var id = _nextChunkId++;
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(p.Chunk.Content)));
            _chunks[id] = (fileId, p.Chunk, hash, p.Vector, modelId);
        }
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<VectorEntry>> LoadVectorsAsync(string? pathPrefix = null, string? language = null, int maxResults = int.MaxValue)
    {
        var q = _chunks.Values.AsEnumerable();
        if (!string.IsNullOrEmpty(pathPrefix)) q = q.Where(c => _files.TryGetValue(c.fileId, out var f) && f.FilePath.StartsWith(pathPrefix, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(language)) q = q.Where(c => _files.TryGetValue(c.fileId, out var f) && f.Language == language);
        if (maxResults < int.MaxValue) q = q.Take(maxResults);
        var list = q.Select(c => new VectorEntry { ChunkId = _chunks.First(kv => kv.Value.chunk == c.chunk).Key, Vector = c.vector }).ToList();
        return Task.FromResult<IReadOnlyList<VectorEntry>>(list);
    }

    public Task<Dictionary<long, CodeChunk>> GetChunksAsync(IReadOnlyList<long> chunkIds)
    {
        var dict = new Dictionary<long, CodeChunk>();
        foreach (var id in chunkIds)
        {
            if (_chunks.TryGetValue(id, out var c))
            {
                var file = _files[c.fileId];
                dict[id] = new CodeChunk { FilePath = file.FilePath, Language = file.Language, ChunkIndex = c.chunk.ChunkIndex, StartLine = c.chunk.StartLine, EndLine = c.chunk.EndLine, ChunkType = c.chunk.ChunkType, SymbolName = c.chunk.SymbolName, Content = c.chunk.Content };
            }
        }
        return Task.FromResult(dict);
    }

    public Task<StoreStats> GetStatsAsync()
    {
        var first = _chunks.Values.FirstOrDefault();
        return Task.FromResult(new StoreStats { FileCount = _files.Count, ChunkCount = _chunks.Count, ModelId = first.modelId, Dimension = first.vector.Length });
    }
}
