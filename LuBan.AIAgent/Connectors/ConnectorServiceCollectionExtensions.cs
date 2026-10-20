/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Connectors
*文件名： ConnectorServiceCollectionExtensions
*版本号： V1.0.0.0
*唯一标识：新建
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/19
*描述：连接器 DI 注册扩展 —— 基础设施 / AddConnector
*
*****************************************************************************/
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LuBan.AIAgent.Connectors;

/// <summary>
/// 连接器 DI 注册扩展。
/// </summary>
/// <remarks>
/// <para>
/// 注册<b>不</b>等于启用：<see cref="AddConnector{TConnector}"/> 只把「提供方键 → 工厂」登记进
/// 单例工厂表，实际是否实例化由 <c>config.json:connectors</c>（可用性）与
/// <c>.luban-agent/connectors/*.json</c>（启用）决定。
/// </para>
/// <para>
/// 不使用特性扫描 / 反射发现连接器（Native AOT 不友好），由宿主显式注册
/// （<see cref="AddBuiltinConnectors"/> 只登记基础设施，厂商连接器由宿主 / 产品 / 插件各自登记）。
/// </para>
/// </remarks>
public static class ConnectorServiceCollectionExtensions
{
    /// <summary>
    /// 注册自定义连接器工厂。
    /// </summary>
    /// <typeparam name="TConnector">连接器类型。</typeparam>
    /// <param name="services">服务集合。</param>
    /// <param name="providerKey">提供方键（如 <c>myConnector</c>）。</param>
    /// <param name="factory">连接器工厂；实例名 / 设置 / 容器由 <see cref="ConnectorBuildContext"/> 传入。</param>
    /// <returns>服务集合（链式）。</returns>
    public static IServiceCollection AddConnector<TConnector>(
        this IServiceCollection services,
        string providerKey,
        Func<ConnectorBuildContext, TConnector> factory)
        where TConnector : IConnector
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(factory);

        if (string.IsNullOrWhiteSpace(providerKey))
            throw new ArgumentException("提供方键不能为空。", nameof(providerKey));

        services.AddSingleton(new ConnectorFactoryRegistration(providerKey.Trim(), context => factory(context)));

        return services;
    }

    /// <summary>
    /// 注册连接器基础设施（工厂表、实例宿主、会话级注册表）。
    /// </summary>
    /// <param name="services">服务集合。</param>
    /// <param name="configuration">宿主配置（保留参数；当前基础设施不依赖配置，厂商连接器由宿主另行注册）。</param>
    /// <returns>服务集合（链式）。</returns>
    /// <remarks>
    /// 框架<b>不</b>再登记任何厂商连接器：连接器定义属产品或插件，调用方在注册基础设施后
    /// 自行用 <see cref="AddConnector{TConnector}"/> 登记。
    /// </remarks>
    public static IServiceCollection AddBuiltinConnectors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        _ = configuration;

        // 基础设施：工厂表与实例宿主为单例；注册表为会话级
        services.AddSingleton<ConnectorFactoryRegistry>();
        services.AddSingleton<ConnectorInstanceHost>();
        services.TryAddScoped<ConnectorRegistry>();

        // 连接器配置写入器：默认写工作区 .luban-agent/connectors/<name>.json；
        // 产品可注册自定义实现覆盖（TryAdd 保证不覆盖已注册实现）。
        services.TryAddScoped<IConnectorConfigWriter, WorkspaceConnectorConfigWriter>();

        // 说明：内置连接器（厂商实现）不在此登记——
        // 框架只保留「抽象基本连接器 + 连接器管理」，厂商连接器由宿主 / 产品 / 插件
        // 经 AddConnector 显式注册（硬编码或配置驱动）。
        return services;
    }
}