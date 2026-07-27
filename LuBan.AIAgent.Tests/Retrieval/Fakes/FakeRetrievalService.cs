using LuBan.AIAgent.Retrieval;

namespace LuBan.AIAgent.Tests.Retrieval.Fakes;

public class FakeRetrievalService : IRetrievalService
{
    public Task<IndexReport> IndexDirectoryAsync(string path, string? glob = null, bool force = false, CancellationToken cancellationToken = default)
        => Task.FromResult(new IndexReport { ScannedFiles = 1, NewFiles = 1, TotalChunks = 1, EmbeddedChunks = 1 });

    public Task<IndexReport> IndexContentAsync(string content, string language, string sourceName, CancellationToken cancellationToken = default)
        => Task.FromResult(new IndexReport { ScannedFiles = 1, NewFiles = 1, TotalChunks = 1, EmbeddedChunks = 1 });

    public Task<IReadOnlyList<RetrievalResult>> SearchAsync(string query, int topK = 5, string? pathPrefix = null, string? language = null, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<RetrievalResult>>(new List<RetrievalResult>
        {
            new() { ChunkId = 1, FilePath = "test.cs", StartLine = 1, EndLine = 10, ChunkType = "Method", SymbolName = "Test", Content = "Test content", Score = 0.95 }
        });

    public Task<IndexStats> GetStatsAsync()
        => Task.FromResult(new IndexStats { TotalFiles = 10, TotalChunks = 100, ModelId = "test", VectorDimension = 384 });
}
