/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Common.DI
*文件名： ServiceProvider
*版本号： V1.0.0.0
*唯一标识：f1e96015-a10d-48d3-a33e-7938c16b8e2a
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 18:45:47
*描述：DI工具类
*
*=================================================
*修改标记
*修改时间：2023/12/4 18:45:47
*修改人： yswenli
*版本号： V1.0.0.0
*描述：DI工具类
*
*****************************************************************************/


namespace LuBan.DI;

/// <summary>
/// DI工具类
/// </summary>
public static class ServiceProviderUtil
{
    static readonly object _lock = new();
    static IServiceCollection _services = new ServiceCollection();
    static IServiceProvider? _serviceProvider;

    /// <summary>
    /// 服务提供器
    /// </summary>
    public static IServiceCollection Services
    {
        set
        {
            lock (_lock)
            {
                _services = value;
            }
        }
        get
        {
            return _services;
        }
    }

    /// <summary>
    /// 设置服务提供容器
    /// </summary>
    /// <param name="services"></param>
    public static void InitServiceProvider(this IServiceCollection services)
    {
        Services = services;
    }

    /// <summary>
    /// 注入瞬态周期的服务
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    public static void AddTransient<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>() where TService : class
    {
        _services.AddTransient<TService>();
    }

    /// <summary>
    /// 注入请求生存周期的服务
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    public static void AddScoped<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>() where TService : class
    {
        _services.AddScoped<TService>();
    }

    /// <summary>
    /// 注入单例周期的服务
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    public static void AddSingleton<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>() where TService : class
    {
        _services.AddSingleton<TService>();
    }

    /// <summary>
    /// 完成DI注入容器
    /// </summary>
    /// <param name="services"></param>
    public static void BuildProvider(this IServiceCollection services)
    {
        ConsoleUtil.WriteLineWithCount("完成DI注入容器", color: ConsoleColor.Green);
        lock (_lock)
        {
            (_serviceProvider as IDisposable)?.Dispose();
            _serviceProvider = _services.BuildServiceProvider();
        }
    }

    /// <summary>
    /// 完成DI注入容器，一般放在最后一步
    /// </summary>
    public static void BuildProvider()
    {
        ConsoleUtil.WriteLineWithCount("创建DI容器", color: ConsoleColor.Green);
        lock (_lock)
        {
            (_serviceProvider as IDisposable)?.Dispose();
            _serviceProvider = _services.BuildServiceProvider();
        }
    }

    /// <summary>
    /// 查找已注入的类型
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object? GetService(this Type type)
    {
        return _serviceProvider?.GetService(type);
    }

    /// <summary>
    /// 查找已注入的类型
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    public static TService? GetService<TService>()
    {
        return _serviceProvider != null ? _serviceProvider.GetService<TService>() : default;
    }

    /// <summary>
    /// 获取请求生存周期的服务
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    public static TService GetRequiredService<TService>()
        where TService : notnull
    {
        return (_serviceProvider ?? throw new InvalidOperationException("ServiceProvider has not been built. Call BuildProvider first.")).GetRequiredService<TService>();
    }

    /// <summary>
    /// 获取请求生存周期的服务
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object GetRequiredService(this Type type)
    {
        return (_serviceProvider ?? throw new InvalidOperationException("ServiceProvider has not been built. Call BuildProvider first.")).GetRequiredService(type);
    }

    /// <summary>
    /// 获取作用域共享的异常信息
    /// </summary>
    /// <returns></returns>
    public static ExceptionScope GetExceptionScope()
    {
        return GetRequiredService<ExceptionScope>();
    }

    /// <summary>
    /// 获取请求生存周期的服务，适用于一次性获取某个服务的场景，不可以重复调用，例如测试
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="addServiceProvider">添加自定义ServiceProvider</param>
    /// <returns></returns>
    public static T GetRequiredServiceForOnce<T>(Action<IServiceCollection> addServiceProvider) where T : notnull
    {
        lock (_lock)
        {
            addServiceProvider?.Invoke(_services);
            (_serviceProvider as IDisposable)?.Dispose();
            _serviceProvider = _services.BuildServiceProvider();
            return GetRequiredService<T>();
        }
    }

    /// <summary>
    /// 全局自动注入di，带InjectionAttribute的IScoped，ISingleton，ITransient
    /// </summary>
    /// <param name="services"></param>
    public static IServiceCollection AutoInjectAllCustomerServices(this IServiceCollection services)
    {
        ConsoleUtil.WriteLineWithCount("正在自动注入全局中符合框架的InjectionAttribute的IScoped，ISingleton，ITransient的对象到DI容器", color: ConsoleColor.Green);
        return ServiceDescriptorUtil.AutoInjectAllCustomerServices(services);
    }
}
