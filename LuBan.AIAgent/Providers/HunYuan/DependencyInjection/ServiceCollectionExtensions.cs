using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Providers.HunYuan;

namespace LuBan.AIAgent.Providers.HunYuan.DependencyInjection;

/// <summary>
/// 服务集合扩展，用于向服务集合中添加 HunYuan 提供者
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 向服务集合中添加 HunYuan 提供者
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configureOptions">配置选项的委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddHunYuanProvider(this IServiceCollection services, Action<HunYuanOptions> configureOptions)
    {
        services.Configure(configureOptions);
        
        services.AddHttpClient<HunYuanChatModelProvider>()
            .AddHttpMessageHandler(sp =>
            {
                var options = sp.GetRequiredService<IOptions<HunYuanOptions>>().Value;
                var logger = sp.GetService<Microsoft.Extensions.Logging.ILogger<HunYuanRetryHttpMessageHandler>>();
                return new HunYuanRetryHttpMessageHandler(options, logger);
            });
        
        services.AddSingleton<IChatModelProvider>(sp => sp.GetRequiredService<HunYuanChatModelProvider>());
        
        return services;
    }
}
