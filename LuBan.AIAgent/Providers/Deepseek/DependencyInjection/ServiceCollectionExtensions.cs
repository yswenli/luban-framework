using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Providers.DeepSeek;

namespace LuBan.AIAgent.Providers.DeepSeek.DependencyInjection;

/// <summary>
/// 服务集合扩展，用于向服务集合中添加 DeepSeek 提供者
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 向服务集合中添加 DeepSeek 提供者
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configureOptions">配置选项的委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddDeepSeekProvider(this IServiceCollection services, Action<DeepSeekOptions> configureOptions)
    {
        services.Configure(configureOptions);
        
        services.AddHttpClient<DeepSeekChatModelProvider>()
            .AddHttpMessageHandler(sp =>
            {
                var options = sp.GetRequiredService<IOptions<DeepSeekOptions>>().Value;
                var logger = sp.GetService<Microsoft.Extensions.Logging.ILogger<DeepSeekRetryHttpMessageHandler>>();
                return new DeepSeekRetryHttpMessageHandler(options, logger);
            });
        
        services.AddSingleton<IChatModelProvider>(sp => sp.GetRequiredService<DeepSeekChatModelProvider>());
        
        return services;
    }
}