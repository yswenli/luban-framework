using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Providers.MiMo;

namespace LuBan.AIAgent.Providers.MiMo.DependencyInjection;

/// <summary>
/// 服务集合扩展，用于向服务集合中添加 MiMo 提供者
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 向服务集合中添加 MiMo 提供者
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configureOptions">配置选项的委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddMiMoProvider(this IServiceCollection services, Action<MiMoOptions> configureOptions)
    {
        services.Configure(configureOptions);

        services.AddHttpClient<MiMoChatModelProvider>()
            .AddHttpMessageHandler(sp =>
            {
                var options = sp.GetRequiredService<IOptions<MiMoOptions>>().Value;
                var logger = sp.GetService<Microsoft.Extensions.Logging.ILogger<MiMoRetryHttpMessageHandler>>();
                return new MiMoRetryHttpMessageHandler(options, logger);
            });

        services.AddSingleton<IChatModelProvider>(sp => sp.GetRequiredService<MiMoChatModelProvider>());

        return services;
    }
}