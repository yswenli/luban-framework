/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Logging.FileLogger
*文件名： FileLogger.cs
*版本号： V1.0.0.0
*唯一标识：eb69703b-57f1-46e9-8428-6d87cd42a5ed
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31 14:36:04
*描述：FileLogger 类
*
*=================================================
*修改标记
*修改时间：2026/7/31 14:36:04
*修改人： yswenli
*版本号： V1.0.0.0
*描述：FileLogger 类
*
*****************************************************************************/

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
