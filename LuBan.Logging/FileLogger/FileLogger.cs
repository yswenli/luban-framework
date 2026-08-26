namespace LuBan.Logging.FileLogger;

/// <summary>
/// 文件日志 ILogger 实现，按 category name 路由到不同的文件。
/// </summary>
internal sealed class FileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly FileLoggerProvider _provider;

    /// <summary>
    /// 初始化文件日志记录器。
    /// </summary>
    /// <param name="categoryName">类别名称。</param>
    /// <param name="provider">文件日志 Provider。</param>
    public FileLogger(string categoryName, FileLoggerProvider provider)
    {
        _categoryName = categoryName;
        _provider = provider;
    }

    /// <inheritdoc/>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel)
    {
        return _provider.IsEnabledForCategory(_categoryName);
    }

    /// <inheritdoc/>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (formatter == null) return;

        var message = formatter(state, exception);
        if (string.IsNullOrEmpty(message)) return;

        _provider.Write(_categoryName, message);
    }
}
