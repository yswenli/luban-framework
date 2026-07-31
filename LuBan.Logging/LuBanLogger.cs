namespace LuBan.Logging;

/// <summary>
/// LuBan 日志服务，通过 DI 单例方式提供 ILogger 创建能力。
/// </summary>
public interface ILuBanLogger
{
    /// <summary>
    /// 按类别名称创建日志记录器。
    /// </summary>
    ILogger CreateLogger(string categoryName);

    /// <summary>
    /// 按类型创建日志记录器。
    /// </summary>
    ILogger<T> CreateLogger<T>();
}

/// <summary>
/// LuBan 日志服务实现，封装 ILoggerFactory。
/// </summary>
public sealed class LuBanLogger : ILuBanLogger, ISingleton
{
    private readonly ILoggerFactory _factory;

    /// <summary>
    /// 初始化 LuBan 日志服务。
    /// </summary>
    /// <param name="factory">日志工厂。</param>
    public LuBanLogger(ILoggerFactory factory)
    {
        _factory = factory;
    }

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName)
    {
        return _factory.CreateLogger(categoryName);
    }

    /// <inheritdoc/>
    public ILogger<T> CreateLogger<T>()
    {
        return _factory.CreateLogger<T>();
    }
}
