using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LuBan.AIAgent.Providers.AzureOpenAI.DependencyInjection;

/// <summary>
/// 服务集合扩展，用于向服务集合中添加 Azure OpenAI 提供者
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 向服务集合中添加 Azure OpenAI 提供者
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configureOptions">配置选项的委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddAzureOpenAIProvider(this IServiceCollection services, Action<AzureOpenAIOptions> configureOptions)
    {
        services.Configure(configureOptions);

        services.AddHttpClient<AzureOpenAIChatModelProvider>()
            .AddHttpMessageHandler(sp =>
            {
                var options = sp.GetRequiredService<IOptions<AzureOpenAIOptions>>().Value;
                var logger = sp.GetService<Microsoft.Extensions.Logging.ILogger<AzureOpenAIRetryHttpMessageHandler>>();
                return new AzureOpenAIRetryHttpMessageHandler(options, logger);
            });

        services.AddSingleton<IChatModelProvider>(sp => sp.GetRequiredService<AzureOpenAIChatModelProvider>());

        return services;
    }
}
