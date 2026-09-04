/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common.Http
*文件名： HttpClientServiceCollectionExtensions.cs
*版本号： V1.0.0.0
*唯一标识：bdf3132a-be37-4fda-815a-0488c2317822
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：HttpClientServiceCollectionExtensions 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：HttpClientServiceCollectionExtensions 类
*
*****************************************************************************/

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
