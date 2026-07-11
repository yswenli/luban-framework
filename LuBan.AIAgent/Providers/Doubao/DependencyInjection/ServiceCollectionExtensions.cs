using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Providers.Doubao;

namespace LuBan.AIAgent.Providers.Doubao.DependencyInjection;

/// <summary>
/// 服务集合扩展，用于向服务集合中添加 Doubao 提供者
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 向服务集合中添加 Doubao 提供者
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configureOptions">配置选项的委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddDoubaoProvider(this IServiceCollection services, Action<DoubaoOptions> configureOptions)
    {
        services.Configure(configureOptions);
        
        services.AddHttpClient<DoubaoChatModelProvider>()
            .AddHttpMessageHandler(sp =>
            {
                var options = sp.GetRequiredService<IOptions<DoubaoOptions>>().Value;
                var logger = sp.GetService<Microsoft.Extensions.Logging.ILogger<DoubaoRetryHttpMessageHandler>>();
                return new DoubaoRetryHttpMessageHandler(options, logger);
            });
        
        services.AddSingleton<IChatModelProvider>(sp => sp.GetRequiredService<DoubaoChatModelProvider>());
        
        return services;
    }
}