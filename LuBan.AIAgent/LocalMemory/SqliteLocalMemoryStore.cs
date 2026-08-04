/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.LocalMemory
*文件名： SqliteLocalMemoryStore
*版本号： V1.0.0.0
*唯一标识：a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/4
*描述：基于 SQLite 的本地记忆存储实现
*
*=================================================
*修改标记
*修改时间：2026/8/4
*修改人： yswenli
*版本号： V1.0.0.0
*描述：基于 SQLite 的本地记忆存储实现
*
*****************************************************************************/
using System.Data;
using System.Data.SQLite;

namespace LuBan.AIAgent.LocalMemory;

/// <summary>
/// 基于 SQLite 的本地记忆存储实现
/// </summary>
public class SqliteLocalMemoryStore : ILocalMemoryStore, IDisposable
{
    private readonly string _connectionString;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _disposed;

    /// <summary>
    /// 创建 SQLite 本地记忆存储
    /// </summary>
    /// <param name="dbPath">SQLite 数据库文件路径，若不存在则自动创建目录与文件</param>
    public SqliteLocalMemoryStore(string dbPath)
    {
        var dir = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        _connectionString = $"Data Source={dbPath};Pooling=false;Foreign Keys=false;";
        EnsureTableAsync().GetAwaiter().GetResult();
    }

    private async Task EnsureTableAsync()
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS LocalMemory (
                Id TEXT PRIMARY KEY,
                Content TEXT NOT NULL,
                Category TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                VectorDimension INTEGER NOT NULL,
                Vector BLOB NOT NULL
            );
            CREATE INDEX IF NOT EXISTS idx_localmemory_category ON LocalMemory(Category);
            CREATE INDEX IF NOT EXISTS idx_localmemory_updated ON LocalMemory(UpdatedAt DESC);
            """;
        await cmd.ExecuteNonQueryAsync();
    }

    private SQLiteConnection CreateConnection() => new(_connectionString);

    /// <inheritdoc />
    public async Task SaveAsync(MemoryEntry entry, byte[] vectorBytes, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(vectorBytes);

        await _gate.WaitAsync(cancellationToken);
        try
        {
            using var conn = CreateConnection();
            await conn.OpenAsync(cancellationToken);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                INSERT INTO LocalMemory (Id, Content, Category, CreatedAt, UpdatedAt, VectorDimension, Vector)
                VALUES (@id, @content, @category, @createdAt, @updatedAt, @dimension, @vector)
                ON CONFLICT(Id) DO UPDATE SET
                    Content = excluded.Content,
                    Category = excluded.Category,
                    UpdatedAt = excluded.UpdatedAt,
                    VectorDimension = excluded.VectorDimension,
                    Vector = excluded.Vector;
                """;
            cmd.Parameters.AddWithValue("@id", entry.Id);
            cmd.Parameters.AddWithValue("@content", entry.Content);
            cmd.Parameters.AddWithValue("@category", entry.Category);
            cmd.Parameters.AddWithValue("@createdAt", entry.CreatedAt.ToString("O"));
            cmd.Parameters.AddWithValue("@updatedAt", entry.UpdatedAt.ToString("O"));
            cmd.Parameters.AddWithValue("@dimension", entry.VectorDimension);
            cmd.Parameters.AddWithValue("@vector", vectorBytes);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        await _gate.WaitAsync(cancellationToken);
        try
        {
            using var conn = CreateConnection();
            await conn.OpenAsync(cancellationToken);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM LocalMemory WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
            return rows > 0;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MemoryEntry>> ListAsync(string? category = null, int limit = 100, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var results = new List<MemoryEntry>();
        using var conn = CreateConnection();
        await conn.OpenAsync(cancellationToken);
        using var cmd = conn.CreateCommand();
        cmd.CommandText = category == null
            ? "SELECT Id, Content, Category, CreatedAt, UpdatedAt, VectorDimension FROM LocalMemory ORDER BY UpdatedAt DESC LIMIT @limit"
            : "SELECT Id, Content, Category, CreatedAt, UpdatedAt, VectorDimension FROM LocalMemory WHERE Category = @category ORDER BY UpdatedAt DESC LIMIT @limit";
        if (category != null) cmd.Parameters.AddWithValue("@category", category);
        cmd.Parameters.AddWithValue("@limit", limit);

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(ReadEntry(reader));
        }
        return results;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<(MemoryEntry Entry, byte[] VectorBytes)>> LoadAllAsync(string? category = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var results = new List<(MemoryEntry, byte[])>();
        using var conn = CreateConnection();
        await conn.OpenAsync(cancellationToken);
        using var cmd = conn.CreateCommand();
        cmd.CommandText = category == null
            ? "SELECT Id, Content, Category, CreatedAt, UpdatedAt, VectorDimension, Vector FROM LocalMemory ORDER BY UpdatedAt DESC"
            : "SELECT Id, Content, Category, CreatedAt, UpdatedAt, VectorDimension, Vector FROM LocalMemory WHERE Category = @category ORDER BY UpdatedAt DESC";
        if (category != null) cmd.Parameters.AddWithValue("@category", category);

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var entry = ReadEntry(reader);
            var vector = (byte[])reader["Vector"];
            results.Add((entry, vector));
        }
        return results;
    }

    private static MemoryEntry ReadEntry(IDataRecord reader)
    {
        return new MemoryEntry
        {
            Id = reader.GetString(0),
            Content = reader.GetString(1),
            Category = reader.GetString(2),
            CreatedAt = DateTime.ParseExact(reader.GetString(3), "O", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind),
            UpdatedAt = DateTime.ParseExact(reader.GetString(4), "O", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind),
            VectorDimension = reader.GetInt32(5)
        };
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _gate.Dispose();
        SQLiteConnection.ClearAllPools();
    }
}
