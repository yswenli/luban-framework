using System.Collections.Concurrent;

namespace LuBan.Logging.FileLogger;

/// <summary>
/// 文件日志 Provider，按 category name 路由到 5 个文件。
/// </summary>
internal sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly FileLoggerOptions _options;
    private readonly ConcurrentDictionary<string, RollingFileWriter> _writers = new();
    private readonly Dictionary<string, string> _categoryToFile = new()
    {
        { "loginfo", "info.txt" },
        { "logdebug", "debug.txt" },
        { "logwarn", "warn.txt" },
        { "logerror", "error.txt" },
        { "logcall", "calllog.txt" }
    };

    /// <summary>
    /// 初始化文件日志 Provider。
    /// </summary>
    /// <param name="options">文件日志配置。</param>
    public FileLoggerProvider(FileLoggerOptions options)
    {
        _options = options;
    }

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(categoryName, this);
    }

    /// <summary>
    /// 按 category name 写入对应的文件。
    /// </summary>
    /// <param name="categoryName">类别名称。</param>
    /// <param name="message">日志消息。</param>
    public void Write(string categoryName, string message)
    {
        if (!_categoryToFile.TryGetValue(categoryName, out var fileName))
        {
            return;
        }

        var writer = _writers.GetOrAdd(fileName, fn => new RollingFileWriter(fn, _options));
        writer.WriteLine(message);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var writer in _writers.Values)
        {
            writer.Dispose();
        }
        _writers.Clear();
    }
}
