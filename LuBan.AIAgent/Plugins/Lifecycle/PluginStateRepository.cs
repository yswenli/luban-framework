/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Plugins.Lifecycle
*文件名： PluginStateRepository
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/15
*描述：插件状态仓储 - JSON 文件式默认实现（框架无 SQLite 依赖）
*
*****************************************************************************/
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LuBan.AIAgent.Plugins.Lifecycle;

/// <summary>
/// <see cref="IPluginStateRepository"/> 的 JSON 文件式默认实现。
/// </summary>
/// <remarks>
/// PRD §3.5 原拟在宿主 <c>LubanAgentCore</c> 复用 SQLite 仓储；但框架 <c>LuBan.AIAgent</c>
/// 不依赖 SQLite / Orm，故此处提供无外部依赖的默认实现，宿主可在 DI 中替换为 SQLite 版本。
/// 所有读写经 <see cref="_gate"/> 串行化，避免并发写坏文件。
/// </remarks>
public sealed class PluginStateRepository : IPluginStateRepository
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1, 1);

    /// <summary>
    /// 创建插件状态仓储。
    /// </summary>
    /// <param name="filePath">
    /// 状态文件路径；为空时使用
    /// <c>%LocalAppData%/LuBan/AIAgent/plugins/plugin-states.json</c>（对齐 PRD §3.3 用户级目录约定）。
    /// </param>
    public PluginStateRepository(string? filePath = null)
    {
        _filePath = string.IsNullOrWhiteSpace(filePath)
            ? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "LuBan",
                "AIAgent",
                "plugins",
                "plugin-states.json")
            : filePath!;
    }

    /// <summary>
    /// 状态文件路径。
    /// </summary>
    public string FilePath => _filePath;

    /// <inheritdoc />
    public async Task<IReadOnlyList<PluginStateRecord>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return (await ReadAllCoreAsync(cancellationToken).ConfigureAwait(false)).Values.ToList();
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task<PluginStateRecord?> GetAsync(string pluginId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
            return null;

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var all = await ReadAllCoreAsync(cancellationToken).ConfigureAwait(false);
            return all.TryGetValue(pluginId, out var record) ? record : null;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task SaveAsync(PluginStateRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentException.ThrowIfNullOrWhiteSpace(record.Id);

        record.UpdatedAt = DateTimeOffset.Now;

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var all = await ReadAllCoreAsync(cancellationToken).ConfigureAwait(false);
            all[record.Id] = record;
            await WriteAllCoreAsync(all, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string pluginId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
            return;

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var all = await ReadAllCoreAsync(cancellationToken).ConfigureAwait(false);
            if (!all.Remove(pluginId))
                return;

            await WriteAllCoreAsync(all, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// 读取状态文件（不存在或损坏时返回空字典）。
    /// </summary>
    private async Task<Dictionary<string, PluginStateRecord>> ReadAllCoreAsync(CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, PluginStateRecord>(StringComparer.OrdinalIgnoreCase);

        if (!File.Exists(_filePath))
            return result;

        try
        {
            await using var stream = File.OpenRead(_filePath);
            var all = await JsonSerializer
                .DeserializeAsync<List<PluginStateRecord>>(stream, JsonOpts, cancellationToken)
                .ConfigureAwait(false);

            if (all is null)
                return result;

            foreach (var record in all)
            {
                if (!string.IsNullOrWhiteSpace(record.Id))
                    result[record.Id] = record;
            }
        }
        catch (Exception ex)
        {
            // 状态文件损坏不应阻断插件系统启动，仅记录并重建。
            Logger.Warn($"插件状态文件读取失败，将重建：{_filePath}", ex);
        }

        return result;
    }

    /// <summary>
    /// 原子写入状态文件（先写临时文件再替换）。
    /// </summary>
    private async Task WriteAllCoreAsync(
        Dictionary<string, PluginStateRecord> all,
        CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var tempPath = _filePath + ".tmp";
        await using (var stream = File.Create(tempPath))
        {
            await JsonSerializer
                .SerializeAsync(stream, all.Values.ToList(), JsonOpts, cancellationToken)
                .ConfigureAwait(false);
        }

        File.Move(tempPath, _filePath, overwrite: true);
    }
}