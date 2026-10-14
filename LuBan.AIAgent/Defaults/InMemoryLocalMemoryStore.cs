/***************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Defaults
*文件名： InMemoryLocalMemoryStore
*创建时间：2026/10/14
*描述：本地长期记忆的内存实现（进程内，不持久化），供最小宿主开箱使用
*
*****************************************************************************/
namespace LuBan.AIAgent.Defaults;

/// <summary>
/// 本地长期记忆的内存实现（进程内，不持久化）。
/// <para>
/// 语义对齐 SQLite 实现：按 <c>(UserId, WorkspaceId, ContentHash)</c> 去重 upsert，
/// 命中时更新并保留原 <see cref="MemoryEntry.Id"/>；查询按用户隔离，
/// <c>category == "global"</c> 时仅返回工作区为空的行，否则返回
/// 「当前工作区 + 全局」行；过期条目不返回。
/// </para>
/// <para>
/// 仅用于开发/单机场景。<b>进程退出即丢失</b>，需要持久化请注册自定义
/// <see cref="ILocalMemoryStore"/>（<c>TryAdd</c> 语义允许覆盖默认注册）。
/// </para>
/// </summary>
public sealed class InMemoryLocalMemoryStore : ILocalMemoryStore
{
    private readonly Lock _gate = new();
    private readonly Dictionary<string, (MemoryEntry Entry, byte[] Vector)> _entries = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public Task<MemoryEntry> UpsertAsync(string userId, MemoryEntry entry, byte[] vectorBytes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(vectorBytes);

        lock (_gate)
        {
            var existing = _entries.Values.FirstOrDefault(v =>
                string.Equals(v.Entry.UserId, userId, StringComparison.Ordinal)
                && string.Equals(v.Entry.WorkspaceId, entry.WorkspaceId, StringComparison.Ordinal)
                && string.Equals(v.Entry.ContentHash, entry.ContentHash, StringComparison.Ordinal));

            if (existing.Entry is not null)
            {
                existing.Entry.Content = entry.Content;
                existing.Entry.Category = entry.Category;
                existing.Entry.UpdatedAt = entry.UpdatedAt;
                existing.Entry.ExpiresAt = entry.ExpiresAt;
                existing.Entry.VectorDimension = entry.VectorDimension;
                _entries[existing.Entry.Id] = (existing.Entry, vectorBytes);
                return Task.FromResult(Clone(existing.Entry));
            }

            entry.UserId = userId;
            _entries[entry.Id] = (entry, vectorBytes);
            return Task.FromResult(Clone(entry));
        }
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(string userId, string id, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            if (!_entries.TryGetValue(id, out var hit)) return Task.FromResult(false);
            if (!string.Equals(hit.Entry.UserId, userId, StringComparison.Ordinal)) return Task.FromResult(false);
            _entries.Remove(id);
            return Task.FromResult(true);
        }
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<MemoryEntry>> ListAsync(string userId, string? category = null, string? workspaceId = null, int limit = 100, CancellationToken cancellationToken = default)
    {
        var results = Filter(userId, category, workspaceId, includeAllWorkspaces: false)
            .OrderByDescending(v => v.Entry.UpdatedAt)
            .Take(limit)
            .Select(v => Clone(v.Entry))
            .ToList();
        return Task.FromResult<IReadOnlyList<MemoryEntry>>(results);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<(MemoryEntry Entry, byte[] VectorBytes)>> LoadAllAsync(string userId, string? category = null, string? workspaceId = null, bool includeAllWorkspaces = false, CancellationToken cancellationToken = default)
    {
        var results = Filter(userId, category, workspaceId, includeAllWorkspaces)
            .Select(v => (Clone(v.Entry), v.Vector))
            .ToList();
        return Task.FromResult<IReadOnlyList<(MemoryEntry Entry, byte[] VectorBytes)>>(results);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<(MemoryEntry Entry, byte[] VectorBytes)>> LoadByIdsAsync(string userId, IEnumerable<string> ids, string? workspaceId, CancellationToken cancellationToken = default)
    {
        var idSet = ids.Distinct(StringComparer.Ordinal).ToHashSet(StringComparer.Ordinal);
        if (idSet.Count == 0)
            return Task.FromResult<IReadOnlyList<(MemoryEntry Entry, byte[] VectorBytes)>>(Array.Empty<(MemoryEntry, byte[])>());

        var results = Filter(userId, category: null, workspaceId, includeAllWorkspaces: false)
            .Where(v => idSet.Contains(v.Entry.Id))
            .Select(v => (Clone(v.Entry), v.Vector))
            .ToList();
        return Task.FromResult<IReadOnlyList<(MemoryEntry Entry, byte[] VectorBytes)>>(results);
    }

    /// <inheritdoc />
    public Task<int> DeleteExpiredAsync(string userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        lock (_gate)
        {
            var expired = _entries.Values
                .Where(v => string.Equals(v.Entry.UserId, userId, StringComparison.Ordinal)
                            && v.Entry.ExpiresAt is { } exp && exp < now)
                .Select(v => v.Entry.Id)
                .ToList();
            foreach (var id in expired)
                _entries.Remove(id);
            return Task.FromResult(expired.Count);
        }
    }

    private List<(MemoryEntry Entry, byte[] Vector)> Filter(string userId, string? category, string? workspaceId, bool includeAllWorkspaces)
    {
        var now = DateTime.UtcNow;
        lock (_gate)
        {
            return _entries.Values
                .Where(v => string.Equals(v.Entry.UserId, userId, StringComparison.Ordinal))
                .Where(v => category is null || string.Equals(v.Entry.Category, category, StringComparison.Ordinal))
                .Where(v => v.Entry.ExpiresAt is not { } exp || exp >= now)
                .Where(v => includeAllWorkspaces || IsVisibleInWorkspace(v.Entry, category, workspaceId))
                .ToList();
        }
    }

    private static bool IsVisibleInWorkspace(MemoryEntry entry, string? category, string? workspaceId)
    {
        if (string.Equals(category, MemoryCategories.Global, StringComparison.Ordinal))
            return entry.WorkspaceId is null;

        return entry.WorkspaceId is null
               || string.Equals(entry.WorkspaceId, workspaceId, StringComparison.Ordinal);
    }

    private static MemoryEntry Clone(MemoryEntry entry) => new()
    {
        Id = entry.Id,
        Content = entry.Content,
        Category = entry.Category,
        CreatedAt = entry.CreatedAt,
        UpdatedAt = entry.UpdatedAt,
        VectorDimension = entry.VectorDimension,
        WorkspaceId = entry.WorkspaceId,
        UserId = entry.UserId,
        ContentHash = entry.ContentHash,
        ExpiresAt = entry.ExpiresAt
    };
}