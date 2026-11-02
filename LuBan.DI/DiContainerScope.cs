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
*文件名： DiContainerScope
*版本号： V1.0.0.0
*唯一标识：b5f4eaf8-126a-44a6-be06-e94615a3db65
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/30 14:39:30
*描述：IServiceScopeFactory.CreateScope() 的适配器，把作用域容器包装成标准 IServiceScope，仅供第三方按接口消费。
*
*=================================================
*修改标记
*修改时间：2026/10/30 14:39:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：IServiceScopeFactory.CreateScope() 的适配器，把作用域容器包装成标准 IServiceScope，仅供第三方按接口消费。
*
*****************************************************************************/
namespace LuBan.DI;

/// <summary>
/// <see cref="IServiceScopeFactory.CreateScope()"/> 的适配器，
/// 把 <see cref="DiContainer"/> 作用域实例包装成标准的 <see cref="IServiceScope"/>。
/// </summary>
internal sealed class DiContainerScope : IServiceScope
{
    private readonly DiContainer _container;

    /// <summary>
    /// 初始化适配器。
    /// </summary>
    /// <param name="container">被包装的作用域容器。</param>
    public DiContainerScope(DiContainer container)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
    }

    /// <summary>
    /// 作用域内的服务提供者。
    /// </summary>
    public IServiceProvider ServiceProvider => _container;

    /// <summary>
    /// 释放被包装的作用域容器。
    /// </summary>
    public void Dispose() => _container.Dispose();
}