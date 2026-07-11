using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Core;
using LuBan.AIAgent.Providers.OpenAI;

namespace LuBan.AIAgent.Providers.OpenAI.DependencyInjection;

/// <summary>
/// 服务集合扩展，用于向服务集合中添加 OpenAI 提供者
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 向服务集合中添加 OpenAI 提供者
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configureOptions">配置选项的委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddOpenAIProvider(this IServiceCollection services, Action<OpenAIOptions> configureOptions)
    {
        services.Configure(configureOptions);
        
        services.AddHttpClient<OpenAIChatModelProvider>()
            .AddHttpMessageHandler(sp =>
            {
                var options = sp.GetRequiredService<IOptions<OpenAIOptions>>().Value;
                var logger = sp.GetService<Microsoft.Extensions.Logging.ILogger<RetryHttpMessageHandler>>();
                return new RetryHttpMessageHandler(options, logger);
            });
        
        services.AddSingleton<IChatModelProvider>(sp => sp.GetRequiredService<OpenAIChatModelProvider>());
        
        return services;
    }
}
