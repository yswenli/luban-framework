using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Core;
using LuBan.AIAgent.Providers.Gemini;

namespace LuBan.AIAgent.Providers.Gemini.DependencyInjection;

/// <summary>
/// 服务集合扩展，用于向服务集合中添加 Gemini 提供者
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 向服务集合中添加 Gemini 提供者
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configureOptions">配置选项的委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddGeminiProvider(this IServiceCollection services, Action<GeminiOptions> configureOptions)
    {
        services.Configure(configureOptions);
        
        services.AddHttpClient<GeminiChatModelProvider>()
            .AddHttpMessageHandler(sp =>
            {
                var options = sp.GetRequiredService<IOptions<GeminiOptions>>().Value;
                var logger = sp.GetService<Microsoft.Extensions.Logging.ILogger<GeminiRetryHttpMessageHandler>>();
                return new GeminiRetryHttpMessageHandler(options, logger);
            })
            .ConfigureHttpClient((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<GeminiOptions>>().Value;
                client.DefaultRequestHeaders.Add("x-goog-api-key", options.ApiKey);
            });
        
        services.AddSingleton<IChatModelProvider>(sp => sp.GetRequiredService<GeminiChatModelProvider>());
        
        return services;
    }
}
