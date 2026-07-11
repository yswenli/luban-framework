using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Providers.MiniMax;

namespace LuBan.AIAgent.Providers.MiniMax.DependencyInjection;

/// <summary>
/// 服务集合扩展，用于向服务集合中添加 MiniMax 提供者
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 向服务集合中添加 MiniMax 提供者
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configureOptions">配置选项的委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddMiniMaxProvider(this IServiceCollection services, Action<MiniMaxOptions> configureOptions)
    {
        services.Configure(configureOptions);

        services.AddHttpClient<MiniMaxChatModelProvider>()
            .AddHttpMessageHandler(sp =>
            {
                var options = sp.GetRequiredService<IOptions<MiniMaxOptions>>().Value;
                var logger = sp.GetService<Microsoft.Extensions.Logging.ILogger<MiniMaxRetryHttpMessageHandler>>();
                return new MiniMaxRetryHttpMessageHandler(options, logger);
            });

        services.AddSingleton<IChatModelProvider>(sp => sp.GetRequiredService<MiniMaxChatModelProvider>());

        return services;
    }
}
