using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Providers.Qwen;

namespace LuBan.AIAgent.Providers.Qwen.DependencyInjection;

/// <summary>
/// 服务集合扩展，用于向服务集合中添加 Qwen 提供者
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 向服务集合中添加 Qwen 提供者
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configureOptions">配置选项的委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddQwenProvider(this IServiceCollection services, Action<QwenOptions> configureOptions)
    {
        services.Configure(configureOptions);
        
        services.AddHttpClient<QwenChatModelProvider>()
            .AddHttpMessageHandler(sp =>
            {
                var options = sp.GetRequiredService<IOptions<QwenOptions>>().Value;
                var logger = sp.GetService<Microsoft.Extensions.Logging.ILogger<QwenRetryHttpMessageHandler>>();
                return new QwenRetryHttpMessageHandler(options, logger);
            });
        
        services.AddSingleton<IChatModelProvider>(sp => sp.GetRequiredService<QwenChatModelProvider>());
        
        return services;
    }
}