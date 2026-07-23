/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.DI
*文件名： ServiceDescriptorUtil
*版本号： V1.0.0.0
*唯一标识：ae9657ec-cf08-44ec-af75-3fff43568cc5
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/9/9 17:26:35
*描述：服务描述符工具类
*
*=================================================
*修改标记
*修改时间：2025/9/9 17:26:35
*修改人： yswenli
*版本号： V1.0.0.0
*描述：服务描述符工具类
*
*****************************************************************************/
namespace LuBan.DI;

/// <summary>
/// 服务描述符工具类，
/// 用于描述一个服务（Service）的元数据集合
/// </summary>
public static class ServiceDescriptorUtil
{
    /// <summary>
    /// 根据依赖接口类型解析 ServiceLifetime 对象
    /// </summary>
    /// <param name="dependencyType"></param>
    /// <returns></returns>
    public static ServiceLifetime TryGetServiceLifetime(Type dependencyType)
    {
        if (dependencyType == typeof(ITransient))
        {
            return ServiceLifetime.Transient;
        }
        else if (dependencyType == typeof(IScoped))
        {
            return ServiceLifetime.Scoped;
        }
        else if (dependencyType == typeof(ISingleton))
        {
            return ServiceLifetime.Singleton;
        }
        else
        {
            throw new InvalidCastException("Invalid service registration lifetime.");
        }
    }


    /// <summary>
    /// 创建调度代理
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="dependencyType"></param>
    /// <param name="type">拦截的类型</param>
    /// <param name="proxyType">代理类型</param>
    /// <param name="inter">代理接口</param>
    /// <param name="hasTarget">是否有实现类</param>
    public static void AddDispatchProxy(IServiceCollection services, Type dependencyType, Type type, Type proxyType, Type inter, bool hasTarget = true)
    {
        if (proxyType == null || type == null || type.IsDefined(typeof(SuppressProxyAttribute), true)) return;

        if (!typeof(AspectDispatchProxy).IsAssignableFrom(proxyType))
            throw new ArgumentException($"Proxy type '{proxyType.FullName}' must inherit from AspectDispatchProxy.", nameof(proxyType));

        var lifetime = TryGetServiceLifetime(dependencyType);

        // 注册代理类型
        services.Add(ServiceDescriptor.Describe(typeof(AspectDispatchProxy), proxyType, lifetime));

        // 注册服务
        services.Add(ServiceDescriptor.Describe(inter, provider =>
        {
            var createMethod = typeof(AspectDispatchProxy).GetMethod(nameof(AspectDispatchProxy.Create))
                ?? throw new InvalidOperationException("Create method not found.");
            var proxy = createMethod.MakeGenericMethod(inter, proxyType).Invoke(null, null)
                ?? throw new InvalidOperationException("Create proxy returned null.");
            if (proxy is IDispatchProxy dispatchProxy)
            {
                dispatchProxy.Services = provider;
                if (hasTarget)
                {
                    dispatchProxy.Target = provider.GetService(type)!;
                }
            }
            return proxy;
        }, lifetime));
    }

    /// <summary>
    /// 注册类型
    /// </summary>
    /// <param name="services">服务</param>
    /// <param name="dependencyType"></param>
    /// <param name="type">类型</param>
    /// <param name="injectionAttribute">注入特性</param>
    /// <param name="inter">接口</param>
    public static void Register(IServiceCollection services, Type dependencyType, Type type, InjectionAttribute injectionAttribute, Type? inter = null)
    {
        // 修复泛型注册类型
        var fixedType = ReflectionUtil.FixedGenericType(type);
        var fixedInter = inter == null ? null : ReflectionUtil.FixedGenericType(inter);
        var lifetime = TryGetServiceLifetime(dependencyType);
        if (services.Any(q => q.ServiceType == fixedType || (fixedInter != null && q.ServiceType == fixedInter))) return;
        switch (injectionAttribute.Action)
        {
            case EnumInjectionActions.Add:
                if (fixedInter == null) services.Add(ServiceDescriptor.Describe(fixedType, fixedType, lifetime));
                else
                {
                    services.Add(ServiceDescriptor.Describe(fixedInter, fixedType, lifetime));
                    AddDispatchProxy(services, dependencyType, fixedType, injectionAttribute.Proxy, fixedInter, true);
                }
                break;

            case EnumInjectionActions.TryAdd:
                if (fixedInter == null) services.TryAdd(ServiceDescriptor.Describe(fixedType, fixedType, lifetime));
                else
                {
                    services.TryAdd(ServiceDescriptor.Describe(fixedInter, fixedType, lifetime));
                    AddDispatchProxy(services, dependencyType, fixedType, injectionAttribute.Proxy, fixedInter, true);
                }
                break;

            default: break;
        }
    }



    /// <summary>
    /// 注册服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="dependencyType"></param>
    /// <param name="type">类型</param>
    /// <param name="injectionAttribute">注入特性</param>
    public static void RegisterService(IServiceCollection services, Type dependencyType, Type type, InjectionAttribute injectionAttribute)
    {
        // 注册自己
        if (injectionAttribute.Pattern is EnumInjectionPatterns.Self or EnumInjectionPatterns.All or EnumInjectionPatterns.SelfWithFirstInterface)
        {
            Register(services, dependencyType, type, injectionAttribute);
        }
    }

    /// <summary>
    /// 获取 注册 排序
    /// </summary>
    /// <param name="type">排序类型</param>
    /// <returns>int</returns>
    private static int GetOrder(Type type)
    {
        return !type.IsDefined(typeof(InjectionAttribute), true) ? 0 : type.GetCustomAttribute<InjectionAttribute>(true)?.Order ?? 0;
    }

    /// <summary>
    /// 全局自动注入di，带InjectionAttribute的IScoped，ISingleton，ITransient
    /// </summary>
    /// <param name="services"></param>
    public static IServiceCollection AutoInjectAllCustomerServices(IServiceCollection services)
    {
        // 查找所有需要依赖注入的类型
        var injectTypes = RuntimeUtil.GetTypes(fromCache: false)?
            .Where(u => typeof(IDependency).IsAssignableFrom(u) && u.IsClass && !u.IsInterface && !u.IsAbstract)
            .OrderBy(u => GetOrder(u));

        var lifetimeInterfaces = new[] { typeof(ITransient), typeof(IScoped), typeof(ISingleton) };

        if (injectTypes == null) return services;

        // 执行依赖注入
        foreach (var type in injectTypes)
        {
            // 获取注册方式
            var injectionAttribute = !type.IsDefined(typeof(InjectionAttribute)) ? new InjectionAttribute() : type.GetCustomAttribute<InjectionAttribute>() ?? new InjectionAttribute();

            var interfaces = type.GetInterfaces();

            var dependencyType = interfaces.LastOrDefault(u => lifetimeInterfaces.Contains(u));
            if (dependencyType == null) continue;

            RegisterService(services, dependencyType, type, injectionAttribute);
        }

        return services;
    }
}
