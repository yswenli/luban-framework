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

using LuBan.AIAgent.Wiki.Extractors;

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
    private readonly AsyncReaderWriterLock _rwLock = new();
    // 嵌入串行门：避免后台索引与前台查询并发使用同一 embedder（本地 ONNX）
    private readonly SemaphoreSlim _embedGate = new(1, 1);

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

    /// <summary>
    /// 带串行门的嵌入调用，防止并发使用同一 embedder
    /// </summary>
    private async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(IEnumerable<string> inputs, CancellationToken cancellationToken)
    {
        await _embedGate.WaitAsync(cancellationToken);
        try
        {
            return await _embedder.GenerateAsync(inputs, cancellationToken: cancellationToken);
        }
        finally
        {
            _embedGate.Release();
        }
    }

    /// <inheritdoc />
    public async Task<IndexReport> IndexDirectoryAsync(string path, string? glob = null, bool force = false,
        IProgress<IndexProgress>? progress = null, CancellationToken cancellationToken = default, string? workspaceId = null)
    {
        var report = new IndexReport();
        var root = Path.GetFullPath(path);
        var patterns = string.IsNullOrWhiteSpace(glob) ? new[] { "*" } : glob.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        progress?.Report(new IndexProgress(IndexStage.Scanning, 0, 0, null));
        var files = new List<string>();
        try
        {
            // 枚举期间按“进入新目录”上报扫描进度，调用方可显示“正在扫描：<目录>（已发现 N 个）”，
            // 且无需为获知文件数再预扫一遍。
            var seenFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var lastDir = string.Empty;
            foreach (var p in patterns)
                foreach (var f in ChunkerFactory.EnumerateFiles(root, p))
                {
                    if (!_chunkers.ShouldIndex(f, root, _options.MaxFileSizeKB * 1024L)) continue;
                    if (!seenFiles.Add(f)) continue;
                    files.Add(f);
                    var dir = Path.GetDirectoryName(f) ?? root;
                    if (files.Count % 256 != 0 && string.Equals(dir, lastDir, StringComparison.OrdinalIgnoreCase)) continue;
                    lastDir = dir;
                    progress?.Report(new IndexProgress(IndexStage.Scanning, 0, files.Count, dir));
                }
        }
        catch (Exception ex)
        {
            Logger.Error("枚举索引目录文件失败", ex, path, glob ?? "");
            report.Errors.Add($"枚举文件失败: {ex.Message}");
            return report;
        }
        report.ScannedFiles = files.Count;
        progress?.Report(new IndexProgress(IndexStage.Scanning, 0, files.Count, null));

        var existing = (await _store.GetFilesAsync(root, workspaceId)).ToDictionary(f => f.FilePath, StringComparer.OrdinalIgnoreCase);
        var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < files.Count; i++)
        {
            var file = files[i];
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var fullPath = Path.GetFullPath(file);
                var (content, truncated) = await ReadIndexableContentAsync(fullPath, cancellationToken);
                if (content is null)
                {
                    // 不进 seenPaths：内容已变为二进制，旧分块按“不再可索引”在下方清理
                    report.SkippedFiles++;
                    progress?.Report(new IndexProgress(IndexStage.Embedding, i + 1, files.Count, fullPath));
                    continue;
                }
                if (truncated)
                {
                    Logger.Warn($"{fullPath}: 表格内容超出单文件上限（{ExcelExtractor.IndexingMaxChars} 字符），已截断");
                    report.Warnings.Add($"{fullPath}: 内容超出 {ExcelExtractor.IndexingMaxChars} 字符已截断");
                }
                seenPaths.Add(fullPath);
                var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)));
                existing.TryGetValue(fullPath, out var existingFile);
                if (!force && existingFile != null && existingFile.FileHash == hash)
                {
                    report.SkippedFiles++;
                    progress?.Report(new IndexProgress(IndexStage.Embedding, i + 1, files.Count, fullPath));
                    continue;
                }
                bool isNew = existingFile == null;
                var r = await IndexSingleContentAsync(content, _chunkers.GetLanguage(fullPath), fullPath, hash, cancellationToken, workspaceId);
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
            progress?.Report(new IndexProgress(IndexStage.Embedding, i + 1, files.Count, file));
        }

        var toDelete = existing
            .Where(kv => !seenPaths.Contains(kv.Key) && kv.Key.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            .ToList();
        for (int i = 0; i < toDelete.Count; i++)
        {
            await _store.SoftDeleteFileAsync(toDelete[i].Value.Id, workspaceId);
            report.DeletedFiles++;
            progress?.Report(new IndexProgress(IndexStage.Deleting, i + 1, toDelete.Count, toDelete[i].Key));
        }
        progress?.Report(new IndexProgress(IndexStage.Done, files.Count, files.Count, null));
        return report;
    }

    /// <summary>
    /// 读取待索引文件内容：`.xlsx` 走 Excel 提取（MiniExcel，与 <c>LuBan.Common.ExcelUtil</c> 同库），
    /// 其余白名单文本按 UTF-8 直读。内容含 NUL 字节或全空白视为无可索引内容，返回 <c>null</c>，
    /// 由调用方计入跳过（二进制判断从扫描阶段移到读取阶段，避免预读全部文件）。
    /// 返回的 <c>Truncated</c> 表示表格提取超出 <see cref="ExcelExtractor.IndexingMaxChars"/> 被截断。
    /// </summary>
    private static async Task<(string? Content, bool Truncated)> ReadIndexableContentAsync(string fullPath, CancellationToken cancellationToken)
    {
        if (string.Equals(Path.GetExtension(fullPath), ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            // MiniExcel 为同步 API，放线程池执行并允许在 await 处响应取消（大表提取期间可中断）。
            var source = await Task.Run(
                    () => new ExcelExtractor().ExtractAsync(fullPath, ExcelExtractor.IndexingMaxChars, cancellationToken),
                    CancellationToken.None)
                .WaitAsync(cancellationToken);
            var text = string.IsNullOrWhiteSpace(source.Markdown) ? source.Text : source.Markdown;
            return string.IsNullOrWhiteSpace(text) ? (null, false) : (text, source.Truncated);
        }

        var content = await File.ReadAllTextAsync(fullPath, cancellationToken);
        if (string.IsNullOrWhiteSpace(content)) return (null, false);
        return content.AsSpan().IndexOf('\0') >= 0 ? (null, false) : (content, false);
    }

    /// <inheritdoc />
    public async Task<IndexReport> IndexContentAsync(string content, string language, string sourceName, CancellationToken cancellationToken = default, string? workspaceId = null)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)));
        var existing = await _store.GetFilesAsync(null, workspaceId);
        var old = existing.FirstOrDefault(f => f.FilePath == sourceName);
        if (old != null && old.FileHash == hash)
            return new IndexReport { ScannedFiles = 1, SkippedFiles = 1 };
        return await IndexSingleContentAsync(content, language, sourceName, hash, cancellationToken, workspaceId);
    }

    /// <inheritdoc />
    public async Task<IndexReport> IndexFileAsync(string path, bool force = false, CancellationToken cancellationToken = default, string? workspaceId = null)
    {
        var fullPath = Path.GetFullPath(path);
        // 单文件索引同样受白名单与大小上限约束：.docx/.pdf 等未提取格式不得按 UTF-8 直读出乱码入库。
        var fileRoot = Path.GetDirectoryName(fullPath) ?? fullPath;
        if (!_chunkers.ShouldIndex(fullPath, fileRoot, _options.MaxFileSizeKB * 1024L))
            return new IndexReport { ScannedFiles = 1, SkippedFiles = 1 };
        var (content, truncated) = await ReadIndexableContentAsync(fullPath, cancellationToken);
        if (content is null)
            return new IndexReport { ScannedFiles = 1, SkippedFiles = 1 };
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)));

        if (!force)
        {
            // 前缀查询缩小范围后按路径精确比较（GetFilesAsync 为 StartsWith 语义，需再过滤到相等）。
            var matched = await _store.GetFilesAsync(fullPath, workspaceId);
            var old = matched.FirstOrDefault(f => string.Equals(f.FilePath, fullPath, StringComparison.OrdinalIgnoreCase));
            if (old != null && string.Equals(old.FileHash, hash, StringComparison.OrdinalIgnoreCase))
                return new IndexReport { ScannedFiles = 1, SkippedFiles = 1 };
        }

        var report = await IndexSingleContentAsync(content, _chunkers.GetLanguage(fullPath), fullPath, hash, cancellationToken, workspaceId);
        if (truncated)
        {
            Logger.Warn($"{fullPath}: 表格内容超出单文件上限（{ExcelExtractor.IndexingMaxChars} 字符），已截断");
            report.Warnings.Add($"{fullPath}: 内容超出 {ExcelExtractor.IndexingMaxChars} 字符已截断");
        }
        return report;
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string sourceName, CancellationToken cancellationToken = default, string? workspaceId = null)
    {
        using var _ = await _rwLock.WriteLockAsync(cancellationToken);
        // 前缀查询缩小范围后按来源名精确比较。
        var matched = await _store.GetFilesAsync(sourceName, workspaceId);
        foreach (var file in matched)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.Equals(file.FilePath, sourceName, StringComparison.OrdinalIgnoreCase))
                await _store.SoftDeleteFileAsync(file.Id, workspaceId);
        }
    }

    private async Task<IndexReport> IndexSingleContentAsync(string content, string language, string filePath, string hash, CancellationToken ct, string? workspaceId = null)
    {
        var chunker = _chunkers.GetChunker(filePath);
        var chunks = chunker.Chunk(filePath, content);

        long fileId;
        IReadOnlyList<StoredChunk> stored;
        // 写锁只覆盖落库阶段；提取与嵌入在锁外执行，避免长时间阻塞并发检索
        using (await _rwLock.WriteLockAsync(ct))
        {
            fileId = await _store.UpsertFileAsync(filePath, hash, language, chunks.Count, workspaceId);
            stored = await _store.GetFileChunksAsync(fileId, workspaceId);
        }

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
            var embeddings = await GenerateAsync(batch.Select(b => b.chunk.Content), ct);
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

        using (await _rwLock.WriteLockAsync(ct))
        {
            await _store.ReplaceFileChunksAsync(fileId, _options.ModelId, pairs, workspaceId);
        }

        return new IndexReport
        {
            ScannedFiles = 1, NewFiles = 1, TotalChunks = chunks.Count,
            EmbeddedChunks = embedded, ReusedChunks = chunks.Count - embedded
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RetrievalResult>> SearchAsync(string query, int topK = 5, string? pathPrefix = null, string? language = null,
        CancellationToken cancellationToken = default, string? workspaceId = null)
    {
        topK = Math.Clamp(topK, 1, 20);
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<RetrievalResult>();
        using var _ = await _rwLock.ReadLockAsync(cancellationToken);
        var embeddings = await GenerateAsync(new[] { query }, cancellationToken);
        var queryVector = embeddings[0].Vector.ToArray();
        // 全量候选：maxResults 截断会漏检相关内容，且实现方需按 Id 稳定排序保证可复现
        var entries = await _store.LoadVectorsAsync(pathPrefix, language, int.MaxValue, workspaceId);
        var scored = entries
            .Select(e => (e.ChunkId, Score: VectorMath.Cosine(queryVector, e.Vector)))
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.ChunkId)
            .Take(topK)
            .ToList();
        var map = await _store.GetChunksAsync(scored.Select(s => s.ChunkId).ToList(), workspaceId);
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
    public async Task<IndexStats> GetStatsAsync(string? workspaceId = null)
    {
        var s = await _store.GetStatsAsync(workspaceId);
        return new IndexStats { TotalFiles = s.FileCount, TotalChunks = s.ChunkCount, ModelId = s.ModelId, VectorDimension = s.Dimension };
    }
}
