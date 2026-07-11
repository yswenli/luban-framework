/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.EventBus.Extensions
*文件名： EventBusServiceExtensions
*版本号： V2.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/6/2
*描述：EventBus DI 扩展方法
*
*=================================================
*修改标记
*修改时间：2026/6/2
*修改人： yswenli
*版本号： V2.0.0.0
*描述：EventBus DI 扩展方法
*
*****************************************************************************/
using Microsoft.AspNetCore.Components;

using System.Reflection;

namespace LuBan.EventBus.Extensions;

/// <summary>
/// EventBus DI 扩展方法
/// </summary>
public static class EventBusServiceExtensions
{
    /// <summary>
    /// 注册 EventBus 核心服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configure">配置委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddEventBus(
        this IServiceCollection services,
        Action<EventBusOptions>? configure = null)
    {
        ConsoleUtil.WriteLineWithCount("正在注册事件总线的配置和核心服务", color: ConsoleColor.Green);

        // 注册配置
        if (configure != null)
            services.Configure(configure);
        else
            services.Configure<EventBusOptions>(_ => { });

        // 注册核心服务
        services.AddSingleton<EventPersistence>();
        services.AddSingleton<IEventBus, Core.EventBus>();

        return services;
    }

    /// <summary>
    /// 自动注册的继承IEventHandler事件处理器
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="assemblies">程序集列表</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddEventHandlers(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        ConsoleUtil.WriteLineWithCount("正在注入事件总线的事件处理器", color: ConsoleColor.Green);

        if (assemblies == null || assemblies.Length == 0)
            assemblies = RuntimeUtil.GetAssemblies()?.ToArray()
                ?? Array.Empty<Assembly>();

        foreach (var assembly in assemblies)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract &&
                    t.GetInterfaces().Any(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IEventHandler<>)));

            foreach (var handlerType in handlerTypes)
            {
                var interfaceType = handlerType.GetInterfaces()
                    .First(i => i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IEventHandler<>));

                services.AddTransient(interfaceType, handlerType);
            }
        }

        return services;
    }
}
