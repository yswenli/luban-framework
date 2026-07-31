using LuBan.Logging.Configuration;
using LuBan.Logging.FileLogger;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace LuBan.Logging;

/// <summary>
/// ILoggingBuilder 扩展方法。
/// </summary>
public static class LuBanLoggingExtensions
{
    /// <summary>
    /// 添加 LuBan 文件日志 Provider。
    /// 若已注册 IConfiguration，则自动从 "LuBanLoggingOptions" 节读取配置；未配置或未注册时使用默认值。
    /// </summary>
    /// <param name="builder">日志构建器。</param>
    /// <param name="configure">配置回调（可选，覆盖 IConfiguration 中的值）。</param>
    /// <returns>日志构建器。</returns>
    public static ILoggingBuilder AddLuBanFileLogger(
        this ILoggingBuilder builder,
        Action<LuBanLoggingOptions>? configure = null)
    {
        // 注册 IOptions<LuBanLoggingOptions>，支持从 IConfiguration 读取
        builder.Services.AddOptions<LuBanLoggingOptions>()
            .Configure<IConfiguration>((options, configuration) =>
            {
                configuration.GetSection("LuBanLoggingOptions").Bind(options);
            });

        if (configure != null)
        {
            builder.Services.Configure(configure);
        }

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, FileLoggerProvider>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<LuBanLoggingOptions>>().Value;
            var fileOptions = FileLoggerOptions.FromLuBanOptions(options);
            return new FileLoggerProvider(fileOptions);
        }));

        return builder;
    }

    /// <summary>
    /// 添加 LuBan 文件日志 Provider，并显式指定 IConfiguration。
    /// </summary>
    /// <param name="builder">日志构建器。</param>
    /// <param name="configuration">配置根节点，从 "LuBanLoggingOptions" 节读取。</param>
    /// <param name="configure">配置回调（可选，覆盖 IConfiguration 中的值）。</param>
    /// <returns>日志构建器。</returns>
    public static ILoggingBuilder AddLuBanFileLogger(
        this ILoggingBuilder builder,
        IConfiguration configuration,
        Action<LuBanLoggingOptions>? configure = null)
    {
        builder.Services.Configure<LuBanLoggingOptions>(configuration.GetSection("LuBanLoggingOptions"));

        if (configure != null)
        {
            builder.Services.Configure(configure);
        }

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, FileLoggerProvider>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<LuBanLoggingOptions>>().Value;
            var fileOptions = FileLoggerOptions.FromLuBanOptions(options);
            return new FileLoggerProvider(fileOptions);
        }));

        return builder;
    }
}
