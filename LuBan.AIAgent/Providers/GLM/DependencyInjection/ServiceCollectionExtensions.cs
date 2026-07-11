using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Providers.GLM;

namespace LuBan.AIAgent.Providers.GLM.DependencyInjection;

/// <summary>
/// 服务集合扩展，用于向服务集合中添加 GLM 提供者
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 向服务集合中添加 GLM 提供者
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configureOptions">配置选项的委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddGLMProvider(this IServiceCollection services, Action<GLMOptions> configureOptions)
    {
        services.Configure(configureOptions);
        
        services.AddHttpClient<GLMChatModelProvider>()
            .AddHttpMessageHandler(sp =>
            {
                var options = sp.GetRequiredService<IOptions<GLMOptions>>().Value;
                var logger = sp.GetService<Microsoft.Extensions.Logging.ILogger<GLMRetryHttpMessageHandler>>();
                return new GLMRetryHttpMessageHandler(options, logger);
            });
        
        services.AddSingleton<IChatModelProvider>(sp => sp.GetRequiredService<GLMChatModelProvider>());
        
        return services;
    }
}
