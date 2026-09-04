/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common.Errors
*文件名： ErrorCodeServiceCollectionExtensions.cs
*版本号： V1.0.0.0
*唯一标识：f94dcf3f-dc5e-420b-bf3c-3811a6ecb4a9
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/17 13:20:44
*描述：ErrorCodeServiceCollectionExtensions 类
*
*=================================================
*修改标记
*修改时间：2026/8/17 13:20:44
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ErrorCodeServiceCollectionExtensions 类
*
*****************************************************************************/

namespace LuBan.Common.Errors;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// IServiceCollection 扩展方法，用于注册错误码到依赖注入容器
/// </summary>
public static class ErrorCodeServiceCollectionExtensions
{
    /// <summary>
    /// 注册业务项目自定义错误码。框架内置错误码（FrameworkErrors.All）自动加载。
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="descriptors">业务错误描述符集合</param>
    /// <returns>服务集合（支持链式调用）</returns>
    /// <example>
    /// <code>
    /// services.AddErrorCodes(AppErrors.All);
    /// </code>
    /// </example>
    public static IServiceCollection AddErrorCodes(this IServiceCollection services, IEnumerable<ErrorDescriptor> descriptors)
    {
        services.AddSingleton<ErrorCodeRegistry>(sp =>
        {
            var registry = new ErrorCodeRegistry();
            registry.Register(descriptors);
            return registry;
        });
        return services;
    }

    /// <summary>
    /// 注册 ErrorCodeRegistry（仅包含框架内置错误码）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合（支持链式调用）</returns>
    public static IServiceCollection AddErrorCodes(this IServiceCollection services)
    {
        services.AddSingleton<ErrorCodeRegistry>();
        return services;
    }
}
