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
