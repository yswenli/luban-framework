using LuBan.AIAgent.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LuBan.AIAgent.Providers.ERNIE.DependencyInjection;

/// <summary>
/// 服务集合扩展，用于向服务集合中添加 ERNIE 提供者
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 向服务集合中添加 ERNIE 提供者
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configureOptions">配置选项的委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddERNIEProvider(this IServiceCollection services, Action<ERNIEOptions> configureOptions)
    {
        services.Configure(configureOptions);

        services.AddHttpClient<ERNIEChatModelProvider>()
            .AddHttpMessageHandler(sp =>
            {
                var options = sp.GetRequiredService<IOptions<ERNIEOptions>>().Value;
                var logger = sp.GetService<Microsoft.Extensions.Logging.ILogger<ERNIERetryHttpMessageHandler>>();
                return new ERNIERetryHttpMessageHandler(options, logger);
            });

        services.AddSingleton<IChatModelProvider>(sp => sp.GetRequiredService<ERNIEChatModelProvider>());

        return services;
    }
}