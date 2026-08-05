using LuBan.Logging.Configuration;
using LuBan.Logging.Serialization;

using Microsoft.Extensions.DependencyInjection;

namespace LuBan.Logging;

/// <summary>
/// IServiceCollection 扩展方法，注册 LuBan 文件日志服务。
/// </summary>
public static class LuBanLoggingServiceExtensions
{
    /// <summary>
    /// 注册 LuBan 文件日志 Provider 并注入 static Logger。
    /// 自动从 IConfiguration 读取 LuBanLoggingOptions 配置节。
    /// </summary>
    /// <param name="services">服务集合。</param>
    /// <param name="configure">配置回调（可选，覆盖 IConfiguration 中的值）。</param>
    /// <returns>服务集合。</returns>
    public static IServiceCollection AddLuBanFileLogger(
        this IServiceCollection services,
        Action<LuBanLoggingOptions>? configure = null)
    {
        services.AddLogging(builder => builder.AddLuBanFileLogger(configure));

        return services;
    }

    /// <summary>
    /// 注入 ILoggerFactory 和 STJ 序列化器给 static Logger。
    /// 必须在 BuildProvider 之后调用，以确保 ServiceProviderUtil 能解析 ILoggerFactory。
    /// </summary>
    /// <param name="services">服务集合（未使用，仅作扩展方法锚点）。</param>
    public static void InitLuBanLogger(this IServiceCollection services)
    {
        var factory = ServiceProviderUtil.GetRequiredService<ILoggerFactory>();
        Logger.SetLogger(factory);
        Logger.SetSerializer(CreateLuBanSerializer());
    }

    /// <summary>
    /// 创建 LuBan STJ 序列化器委托，用于注入到 static Logger。
    /// </summary>
    /// <returns>序列化委托。</returns>
    public static Func<object, string> CreateLuBanSerializer()
    {
        return obj => LuBanJsonSerializer.Serialize(obj);
    }
}
