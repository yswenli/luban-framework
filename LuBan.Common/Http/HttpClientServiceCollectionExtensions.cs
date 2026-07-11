using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LuBan.Common.Http;

/// <summary>
/// HttpClient 服务集合扩展方法
/// </summary>
public static class HttpClientServiceCollectionExtensions
{
    /// <summary>
    /// 注册 HttpClientProvider 服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddHttpClientProvider(this IServiceCollection services)
    {
        services.TryAddSingleton<IHttpClientProvider, HttpClientProviderAdapter>();
        return services;
    }
}
