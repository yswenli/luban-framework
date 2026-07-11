using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LuBan.AIAgent.Abstractions;
using Microsoft.Extensions.Logging;

namespace LuBan.AIAgent.Core;

/// <summary>
/// 文件会话存储，将会话状态保存到文件系统
/// </summary>
public class FileSessionStore : ISessionStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _rootDirectory;
    private readonly SemaphoreSlim _mutex = new(1, 1);
    private readonly ILogger<FileSessionStore>? _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options">文件会话存储选项</param>
    /// <param name="logger">日志记录器</param>
    public FileSessionStore(FileSessionStoreOptions options, ILogger<FileSessionStore>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.RootDirectory))
        {
            throw new ArgumentException("RootDirectory is required", nameof(options));
        }

        _rootDirectory = options.RootDirectory;
        _logger = logger;
        Directory.CreateDirectory(_rootDirectory);
    }

    /// <summary>
    /// 异步获取会话状态
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>会话状态，若不存在则返回null</returns>
    public async Task<ConversationState?> GetAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        ValidateSessionId(sessionId);

        var filePath = GetFilePath(sessionId);
        if (!File.Exists(filePath))
        {
            _logger?.LogDebug("FileSessionStore GetAsync. SessionId={SessionId}, Found=false", sessionId);
            return null;
        }

        await _mutex.WaitAsync(cancellationToken);
        try
        {
            await using var stream = File.OpenRead(filePath);
            var state = await JsonSerializer.DeserializeAsync<ConversationState>(stream, JsonOptions, cancellationToken);
            _logger?.LogDebug(
                "FileSessionStore GetAsync. SessionId={SessionId}, Found={Found}, HistoryCount={HistoryCount}, ActiveSkill={ActiveSkill}",
                sessionId,
                state != null,
                state?.History?.Count ?? 0,
                state?.ActiveSkill);
            return state;
        }
        finally
        {
            _mutex.Release();
        }
    }

    /// <summary>
    /// 异步保存会话状态
    /// </summary>
    /// <param name="state">会话状态</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task SaveAsync(ConversationState state, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);
        ValidateSessionId(state.SessionId);

        Directory.CreateDirectory(_rootDirectory);

        var filePath = GetFilePath(state.SessionId);
        var tempFilePath = filePath + ".tmp";

        await _mutex.WaitAsync(cancellationToken);
        try
        {
            await using (var stream = File.Create(tempFilePath))
            {
                await JsonSerializer.SerializeAsync(stream, state, JsonOptions, cancellationToken);
            }

            File.Move(tempFilePath, filePath, overwrite: true);
            _logger?.LogDebug(
                "FileSessionStore SaveAsync. SessionId={SessionId}, HistoryCount={HistoryCount}, ActiveSkill={ActiveSkill}",
                state.SessionId,
                state.History?.Count ?? 0,
                state.ActiveSkill);
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }

            _mutex.Release();
        }
    }

    /// <summary>
    /// 异步删除会话状态
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task DeleteAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        ValidateSessionId(sessionId);

        var filePath = GetFilePath(sessionId);

        await _mutex.WaitAsync(cancellationToken);
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            _logger?.LogDebug("FileSessionStore DeleteAsync. SessionId={SessionId}", sessionId);
        }
        finally
        {
            _mutex.Release();
        }
    }

    /// <summary>
    /// 获取文件路径
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <returns>文件路径</returns>
    private string GetFilePath(string sessionId)
    {
        var fileName = ToSafeFileName(sessionId) + ".json";
        return Path.Combine(_rootDirectory, fileName);
    }

    /// <summary>
    /// 验证会话ID
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    private static void ValidateSessionId(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ArgumentException("SessionId is required", nameof(sessionId));
        }
    }

    /// <summary>
    /// 将会话ID转换为安全的文件名
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <returns>安全的文件名</returns>
    private static string ToSafeFileName(string sessionId)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(sessionId));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
