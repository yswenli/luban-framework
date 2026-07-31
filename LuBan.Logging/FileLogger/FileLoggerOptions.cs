using LuBan.Logging.Configuration;

namespace LuBan.Logging.FileLogger;

/// <summary>
/// 文件日志 Provider 专用配置。
/// </summary>
internal sealed class FileLoggerOptions
{
    /// <summary>
    /// 从 LuBanLoggingOptions 创建 FileLoggerOptions。
    /// </summary>
    public static FileLoggerOptions FromLuBanOptions(LuBanLoggingOptions options)
    {
        return new FileLoggerOptions
        {
            Directory = options.Directory,
            MaxFileSizeBytes = options.MaxFileSizeMB * 1024 * 1024,
            MaxRollBackups = options.MaxRollBackups,
            IncludeScopes = options.IncludeScopes
        };
    }

    /// <summary>
    /// 日志目录。
    /// </summary>
    public string Directory { get; set; } = "logs";

    /// <summary>
    /// 单个日志文件最大大小（字节）。
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 100 * 1024 * 1024;

    /// <summary>
    /// 最大备份数。
    /// </summary>
    public int MaxRollBackups { get; set; } = 5;

    /// <summary>
    /// 是否包含作用域。
    /// </summary>
    public bool IncludeScopes { get; set; } = false;
}
