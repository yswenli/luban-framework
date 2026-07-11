using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Core;
using LuBan.AIAgent.Providers.Claude;

namespace LuBan.AIAgent.Providers.Claude.DependencyInjection;

/// <summary>
/// 服务集合扩展，用于向服务集合中添加 Claude 提供者
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 向服务集合中添加 Claude 提供者
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configureOptions">配置选项的委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddClaudeProvider(this IServiceCollection services, Action<ClaudeOptions> configureOptions)
    {
        services.Configure(configureOptions);
        
        services.AddHttpClient<ClaudeChatModelProvider>()
            .AddHttpMessageHandler(sp =>
            {
                var options = sp.GetRequiredService<IOptions<ClaudeOptions>>().Value;
                var logger = sp.GetService<Microsoft.Extensions.Logging.ILogger<ClaudeRetryHttpMessageHandler>>();
                return new ClaudeRetryHttpMessageHandler(options, logger);
            });
        
        services.AddSingleton<IChatModelProvider>(sp => sp.GetRequiredService<ClaudeChatModelProvider>());
        
        return services;
    }
}
