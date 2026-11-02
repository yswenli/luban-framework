/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.DI
*文件名： DiContainerExtensions
*版本号： V1.0.0.0
*唯一标识：d1b2f774-9358-405a-a882-b8eeb120f6bd
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/30 14:39:30
*描述：容器适配扩展，把已有的容器、服务集合或服务提供者包装成 DiContainer。
*
*=================================================
*修改标记
*修改时间：2026/10/30 14:39:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：容器适配扩展，把已有的容器、服务集合或服务提供者包装成 DiContainer。
*
*****************************************************************************/
namespace LuBan.DI;

/// <summary>
/// 容器适配扩展。
/// </summary>
public static class DiContainerExtensions
{
    /// <summary>
    /// 已是容器，直接返回自身（必需：否则同时满足集合/provider 重载会产生二义性）。
    /// </summary>
    /// <param name="container">已有容器。</param>
    /// <returns>同一个容器实例。</returns>
    public static DiContainer AsDiContainer(this DiContainer container)
        => container ?? throw new ArgumentNullException(nameof(container));

    /// <summary>
    /// 把外部 provider 包成已构建视图（只解析；不持有、不释放被包装对象）。
    /// </summary>
    /// <param name="provider">被包装的服务提供者。</param>
    /// <returns>只解析的容器视图。</returns>
    public static DiContainer AsDiContainer(this IServiceProvider provider)
        => new(provider ?? throw new ArgumentNullException(nameof(provider)));

    /// <summary>
    /// 把外部集合包成注册视图（只注册，不可 Build）。
    /// </summary>
    /// <param name="services">被包装的服务集合。</param>
    /// <returns>只注册的容器视图。</returns>
    public static DiContainer AsDiContainer(this IServiceCollection services)
        => new(services ?? throw new ArgumentNullException(nameof(services)), DiContainerMode.CollectionView);
}