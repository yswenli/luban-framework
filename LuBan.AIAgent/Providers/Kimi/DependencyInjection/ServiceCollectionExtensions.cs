using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Providers.Kimi;

namespace LuBan.AIAgent.Providers.Kimi.DependencyInjection;

/// <summary>
/// 服务集合扩展，用于向服务集合中添加 Kimi 提供者
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 向服务集合中添加 Kimi 提供者
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configureOptions">配置选项的委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddKimiProvider(this IServiceCollection services, Action<KimiOptions> configureOptions)
    {
        services.Configure(configureOptions);
        
        services.AddHttpClient<KimiChatModelProvider>()
            .ConfigureHttpClient((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<KimiOptions>>().Value;
                client.Timeout = options.RequestTimeout;
            })
            .AddHttpMessageHandler(sp =>
            {
                var options = sp.GetRequiredService<IOptions<KimiOptions>>().Value;
                var logger = sp.GetService<Microsoft.Extensions.Logging.ILogger<KimiRetryHttpMessageHandler>>();
                return new KimiRetryHttpMessageHandler(options, logger);
            });
        
        services.AddSingleton<IChatModelProvider>(sp => sp.GetRequiredService<KimiChatModelProvider>());
        
        return services;
    }
}