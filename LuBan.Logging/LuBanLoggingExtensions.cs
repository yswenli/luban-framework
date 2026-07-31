using LuBan.Logging.Configuration;
using LuBan.Logging.FileLogger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LuBan.Logging;

/// <summary>
/// ILoggingBuilder 扩展方法。
/// </summary>
public static class LuBanLoggingExtensions
{
    /// <summary>
    /// 添加 LuBan 文件日志 Provider。
    /// </summary>
    /// <param name="builder">日志构建器。</param>
    /// <param name="configure">配置回调（可选，不传则使用默认值）。</param>
    /// <returns>日志构建器。</returns>
    public static ILoggingBuilder AddLuBanFileLogger(
        this ILoggingBuilder builder,
        Action<LuBanLoggingOptions>? configure = null)
    {
        var options = new LuBanLoggingOptions();
        configure?.Invoke(options);

        var fileOptions = FileLoggerOptions.FromLuBanOptions(options);
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, FileLoggerProvider>(_ => new FileLoggerProvider(fileOptions)));

        return builder;
    }
}
