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
*文件名： DiContainer
*版本号： V1.0.0.0
*唯一标识：51e1ffba-04c1-4999-96d7-e41b5b5e2ba4
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/30 14:39:30
*描述：LuBan 统一 DI 容器，同时承担注册期（IServiceCollection）、解析期（IServiceProvider）、作用域工厂与释放职责。
*
*=================================================
*修改标记
*修改时间：2026/10/30 14:39:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：LuBan 统一 DI 容器，同时承担注册期（IServiceCollection）、解析期（IServiceProvider）、作用域工厂与释放职责。
*
*****************************************************************************/
namespace LuBan.DI;

/// <summary>
/// LuBan 统一 DI 容器。同时实现 <see cref="IServiceCollection"/>（注册期）、
/// <see cref="IServiceProvider"/>（解析期）、<see cref="IServiceScopeFactory"/>（互操作兜底）、
/// <see cref="IDisposable"/> 与 <see cref="IAsyncDisposable"/>（释放宿主与子作用域）。
/// </summary>
public class DiContainer : IServiceCollection, IServiceProvider, IServiceScopeFactory, IDisposable, IAsyncDisposable
{
    private static DiContainer _current = new();

    private readonly object _sync = new();
    private readonly DiContainerMode _mode;
    private readonly bool _canBuild;
    private readonly List<DiContainer> _children = new();

    private IServiceCollection _services;
    private IServiceProvider? _provider;
    private IServiceScope? _scope;
    private DiContainer? _root;
    private bool _ownsProvider;
    private int _disposed;

    // ---------------- 静态入口 ----------------

    /// <summary>
    /// 全局容器入口。全进程唯一的静态解析入口，也是宿主容器对外暴露的句柄。
    /// 默认为空容器（<see cref="GetService(Type)"/> 返回 null、<see cref="GetRequiredService(Type)"/> 抛未构建异常）；
    /// 调用 <see cref="UseAsGlobal"/> 后指向宿主容器。
    /// </summary>
    public static DiContainer Instance => Volatile.Read(ref _current);

    // ---------------- 构造 ----------------

    /// <summary>
    /// 创建空容器（宿主根）。
    /// </summary>
    public DiContainer() : this(new ServiceCollection(), DiContainerMode.Root) { }

    /// <summary>
    /// 包装已有集合（宿主根：接管该集合，可注册、可构建）。
    /// </summary>
    /// <param name="services">被接管的服务集合，注册直接写入该集合。</param>
    public DiContainer(IServiceCollection services)
        : this(services ?? throw new ArgumentNullException(nameof(services)), DiContainerMode.Root) { }

    /// <summary>
    /// 包装已有 provider（已构建视图：只解析，不持有、不释放被包装对象）。
    /// </summary>
    /// <param name="provider">被包装的服务提供者，如 HttpContext.RequestServices。</param>
    public DiContainer(IServiceProvider provider)
        : this(new ServiceCollection(), DiContainerMode.ProviderView)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _provider = provider;
        _ownsProvider = false;
    }

    /// <summary>
    /// 按指定模式初始化容器。
    /// </summary>
    /// <param name="services">服务集合，宿主根与集合视图为调用方集合，其余模式为内部空集合。</param>
    /// <param name="mode">容器模式。</param>
    internal DiContainer(IServiceCollection services, DiContainerMode mode)
    {
        _services = services;
        _mode = mode;
        _canBuild = mode == DiContainerMode.Root;
    }

    /// <summary>
    /// 作用域容器（由 <see cref="CreateScope"/> 创建）。
    /// </summary>
    /// <param name="scope">底层作用域。</param>
    /// <param name="root">派生该作用域的根容器。</param>
    private DiContainer(IServiceScope scope, DiContainer root) : this(new ServiceCollection(), DiContainerMode.Scope)
    {
        _scope = scope;
        _provider = scope.ServiceProvider;
        _ownsProvider = true;
        _root = root;
    }

    // ---------------- 状态 ----------------

    /// <summary>
    /// 当前实例是否即全局容器。
    /// </summary>
    public bool IsGlobal => ReferenceEquals(Instance, this);

    /// <summary>
    /// 是否已有可解析的 provider（已构建根、provider 视图、作用域为 true；未构建根与集合视图为 false）。
    /// </summary>
    public bool IsBuilt => _provider != null;

    /// <summary>
    /// 是否为作用域容器。
    /// </summary>
    public bool IsScope => _mode == DiContainerMode.Scope;

    /// <summary>
    /// 作用域容器的根容器；非作用域返回 null。
    /// </summary>
    public DiContainer? Parent => _root;

    /// <summary>
    /// 内部 provider（极少数第三方互操作使用；产品代码禁止直接使用）。
    /// </summary>
    public IServiceProvider Provider => RequireProvider();

    /// <summary>
    /// 把当前实例设为全局容器。通常在本实例<b>全部注册完成之后</b>调用：
    /// 切换后，原先注册到旧全局实例上的服务不再可见。
    /// </summary>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer UseAsGlobal()
    {
        Interlocked.Exchange(ref _current, this);
        return this;
    }

    // ---------------- 解析 ----------------

    /// <summary>
    /// 解析服务；未注册或未构建返回 null。
    /// </summary>
    /// <param name="serviceType">服务类型。</param>
    /// <returns>服务实例，未解析到返回 null。</returns>
    public virtual object? GetService(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        ThrowIfDisposed();
        return _provider?.GetService(serviceType);   // 未构建：返回 null（GetService 契约允许）
    }

    /// <summary>
    /// 解析服务；未注册或未构建返回 null。
    /// </summary>
    /// <typeparam name="T">服务类型。</typeparam>
    /// <returns>服务实例，未解析到返回 null。</returns>
    public T? GetService<T>() => (T?)GetService(typeof(T));

    /// <summary>
    /// 解析服务；未注册或未构建抛异常。
    /// </summary>
    /// <param name="serviceType">服务类型。</param>
    /// <returns>服务实例。</returns>
    public virtual object GetRequiredService(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        return RequireProvider().GetRequiredService(serviceType);
    }

    /// <summary>
    /// 解析服务；未注册或未构建抛异常。
    /// </summary>
    /// <typeparam name="T">服务类型。</typeparam>
    /// <returns>服务实例。</returns>
    public T GetRequiredService<T>() where T : notnull => (T)GetRequiredService(typeof(T));

    /// <summary>
    /// 解析全部注册。
    /// </summary>
    /// <typeparam name="T">服务类型。</typeparam>
    /// <returns>该类型的全部注册实例。</returns>
    public IEnumerable<T> GetServices<T>() => RequireProvider().GetServices<T>();

    /// <summary>
    /// 未构建 / 已释放的失败语义集中在这一点上。
    /// </summary>
    /// <returns>当前可用的 provider。</returns>
    private IServiceProvider RequireProvider()
    {
        ThrowIfDisposed();
        return _provider ?? throw new InvalidOperationException("容器尚未构建，请先调用 Build()。");
    }

    /// <summary>
    /// 已释放则抛异常。
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (Volatile.Read(ref _disposed) != 0) throw new ObjectDisposedException(nameof(DiContainer));
    }

    // ---------------- 注册（泛型） ----------------

    /// <summary>
    /// 单例。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer AddSingleton<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>() where TService : class
        => Add(ServiceDescriptor.Singleton<TService, TService>());

    /// <summary>
    /// 单例（服务 + 实现）。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <typeparam name="TImpl">实现类型。</typeparam>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer AddSingleton<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImpl>() where TService : class where TImpl : class, TService
        => Add(ServiceDescriptor.Singleton<TService, TImpl>());

    /// <summary>
    /// 单例（实例）。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <param name="instance">单例实例。</param>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer AddSingleton<TService>(TService instance) where TService : class
        => Add(ServiceDescriptor.Singleton(instance));

    /// <summary>
    /// 单例（工厂，工厂参数是 DiContainer）。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <param name="factory">工厂方法，参数为当前解析上下文的容器。</param>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer AddSingleton<TService>(Func<DiContainer, TService> factory) where TService : class
        => Add(ServiceDescriptor.Singleton<TService>(sp => factory(sp.AsDiContainer())));

    /// <summary>
    /// 单例（服务 + 实现工厂）。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <typeparam name="TImpl">实现类型。</typeparam>
    /// <param name="factory">工厂方法，参数为当前解析上下文的容器。</param>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer AddSingleton<TService, TImpl>(Func<DiContainer, TImpl> factory)
        where TService : class where TImpl : class, TService
        => Add(ServiceDescriptor.Singleton<TService>(sp => factory(sp.AsDiContainer())));

    /// <summary>
    /// 作用域。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer AddScoped<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>() where TService : class
        => Add(ServiceDescriptor.Scoped<TService, TService>());

    /// <summary>
    /// 作用域（服务 + 实现）。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <typeparam name="TImpl">实现类型。</typeparam>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer AddScoped<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImpl>() where TService : class where TImpl : class, TService
        => Add(ServiceDescriptor.Scoped<TService, TImpl>());

    /// <summary>
    /// 作用域（实例，等价于单例实例）。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <param name="instance">服务实例。</param>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer AddScoped<TService>(TService instance) where TService : class
        => Add(ServiceDescriptor.Scoped(_ => instance));

    /// <summary>
    /// 作用域（工厂）。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <param name="factory">工厂方法，参数为当前解析上下文的容器。</param>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer AddScoped<TService>(Func<DiContainer, TService> factory) where TService : class
        => Add(ServiceDescriptor.Scoped<TService>(sp => factory(sp.AsDiContainer())));

    /// <summary>
    /// 作用域（服务 + 实现工厂）。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <typeparam name="TImpl">实现类型。</typeparam>
    /// <param name="factory">工厂方法，参数为当前解析上下文的容器。</param>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer AddScoped<TService, TImpl>(Func<DiContainer, TImpl> factory)
        where TService : class where TImpl : class, TService
        => Add(ServiceDescriptor.Scoped<TService>(sp => factory(sp.AsDiContainer())));

    /// <summary>
    /// 瞬态。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer AddTransient<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>() where TService : class
        => Add(ServiceDescriptor.Transient<TService, TService>());

    /// <summary>
    /// 瞬态（服务 + 实现）。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <typeparam name="TImpl">实现类型。</typeparam>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer AddTransient<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImpl>() where TService : class where TImpl : class, TService
        => Add(ServiceDescriptor.Transient<TService, TImpl>());

    /// <summary>
    /// 瞬态（实例，等价于单例实例）。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <param name="instance">服务实例。</param>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer AddTransient<TService>(TService instance) where TService : class
        => Add(ServiceDescriptor.Transient(_ => instance));

    /// <summary>
    /// 瞬态（工厂）。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <param name="factory">工厂方法，参数为当前解析上下文的容器。</param>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer AddTransient<TService>(Func<DiContainer, TService> factory) where TService : class
        => Add(ServiceDescriptor.Transient<TService>(sp => factory(sp.AsDiContainer())));

    /// <summary>
    /// 瞬态（服务 + 实现工厂）。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <typeparam name="TImpl">实现类型。</typeparam>
    /// <param name="factory">工厂方法，参数为当前解析上下文的容器。</param>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer AddTransient<TService, TImpl>(Func<DiContainer, TImpl> factory)
        where TService : class where TImpl : class, TService
        => Add(ServiceDescriptor.Transient<TService>(sp => factory(sp.AsDiContainer())));

    // ---------------- 注册（非泛型，运行时类型 / 外部插件必需） ----------------

    /// <summary>
    /// 单例（服务自身）。
    /// </summary>
    /// <param name="serviceType">服务类型，同时作为实现类型。</param>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer AddSingleton([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type serviceType)
        => Add(ServiceDescriptor.Singleton(serviceType, serviceType));

    /// <summary>
    /// 单例（服务 + 实现）。
    /// </summary>
    /// <param name="serviceType">服务类型。</param>
    /// <param name="implementationType">实现类型。</param>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer AddSingleton(Type serviceType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
        => Add(ServiceDescriptor.Singleton(serviceType, implementationType));

    /// <summary>
    /// 单例（实例）。
    /// </summary>
    /// <param name="serviceType">服务类型。</param>
    /// <param name="instance">单例实例。</param>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer AddSingleton(Type serviceType, object instance)
        => Add(ServiceDescriptor.Singleton(serviceType, instance));

    /// <summary>
    /// 单例（工厂）。
    /// </summary>
    /// <param name="serviceType">服务类型。</param>
    /// <param name="factory">工厂方法，参数为当前解析上下文的容器。</param>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer AddSingleton(Type serviceType, Func<DiContainer, object> factory)
        => Add(ServiceDescriptor.Singleton(serviceType, sp => factory(sp.AsDiContainer())));

    /// <summary>
    /// 作用域（服务自身）。
    /// </summary>
    /// <param name="serviceType">服务类型，同时作为实现类型。</param>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer AddScoped([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type serviceType)
        => Add(ServiceDescriptor.Scoped(serviceType, serviceType));

    /// <summary>
    /// 作用域（服务 + 实现）。
    /// </summary>
    /// <param name="serviceType">服务类型。</param>
    /// <param name="implementationType">实现类型。</param>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer AddScoped(Type serviceType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
        => Add(ServiceDescriptor.Scoped(serviceType, implementationType));

    /// <summary>
    /// 作用域（工厂）。
    /// </summary>
    /// <param name="serviceType">服务类型。</param>
    /// <param name="factory">工厂方法，参数为当前解析上下文的容器。</param>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer AddScoped(Type serviceType, Func<DiContainer, object> factory)
        => Add(ServiceDescriptor.Scoped(serviceType, sp => factory(sp.AsDiContainer())));

    /// <summary>
    /// 瞬态（服务自身）。
    /// </summary>
    /// <param name="serviceType">服务类型，同时作为实现类型。</param>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer AddTransient([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type serviceType)
        => Add(ServiceDescriptor.Transient(serviceType, serviceType));

    /// <summary>
    /// 瞬态（服务 + 实现）。
    /// </summary>
    /// <param name="serviceType">服务类型。</param>
    /// <param name="implementationType">实现类型。</param>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer AddTransient(Type serviceType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
        => Add(ServiceDescriptor.Transient(serviceType, implementationType));

    /// <summary>
    /// 瞬态（工厂）。
    /// </summary>
    /// <param name="serviceType">服务类型。</param>
    /// <param name="factory">工厂方法，参数为当前解析上下文的容器。</param>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer AddTransient(Type serviceType, Func<DiContainer, object> factory)
        => Add(ServiceDescriptor.Transient(serviceType, sp => factory(sp.AsDiContainer())));

    // ---------------- 覆盖 / 清理 ----------------

    /// <summary>
    /// 不存在时才注册单例。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer TryAddSingleton<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>() where TService : class
        => TryAdd(ServiceDescriptor.Singleton<TService, TService>());

    /// <summary>
    /// 不存在时才注册单例（服务 + 实现）。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <typeparam name="TImpl">实现类型。</typeparam>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer TryAddSingleton<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImpl>() where TService : class where TImpl : class, TService
        => TryAdd(ServiceDescriptor.Singleton<TService, TImpl>());

    /// <summary>
    /// 不存在时才注册单例（工厂）。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <param name="factory">工厂方法，参数为当前解析上下文的容器。</param>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer TryAddSingleton<TService>(Func<DiContainer, TService> factory) where TService : class
        => TryAdd(ServiceDescriptor.Singleton<TService>(sp => factory(sp.AsDiContainer())));

    /// <summary>
    /// 不存在时才注册作用域。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer TryAddScoped<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>() where TService : class
        => TryAdd(ServiceDescriptor.Scoped<TService, TService>());

    /// <summary>
    /// 不存在时才注册作用域（服务 + 实现）。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <typeparam name="TImpl">实现类型。</typeparam>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer TryAddScoped<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImpl>() where TService : class where TImpl : class, TService
        => TryAdd(ServiceDescriptor.Scoped<TService, TImpl>());

    /// <summary>
    /// 不存在时才注册瞬态。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer TryAddTransient<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>() where TService : class
        => TryAdd(ServiceDescriptor.Transient<TService, TService>());

    /// <summary>
    /// 不存在时才注册瞬态（服务 + 实现）。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <typeparam name="TImpl">实现类型。</typeparam>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer TryAddTransient<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImpl>() where TService : class where TImpl : class, TService
        => TryAdd(ServiceDescriptor.Transient<TService, TImpl>());

    /// <summary>
    /// 不存在时才注册瞬态（工厂）。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <param name="factory">工厂方法，参数为当前解析上下文的容器。</param>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer TryAddTransient<TService>(Func<DiContainer, TService> factory) where TService : class
        => TryAdd(ServiceDescriptor.Transient<TService>(sp => factory(sp.AsDiContainer())));

    /// <summary>
    /// 替换单例（移除该类型的全部注册后追加一条，语义同 MS 的 Replace）。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <param name="instance">替换后的单例实例。</param>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer ReplaceSingleton<TService>(TService instance) where TService : class
        => RemoveAll<TService>().Add(ServiceDescriptor.Singleton(instance));

    /// <summary>
    /// 替换单例（服务 + 实现）。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <typeparam name="TImpl">实现类型。</typeparam>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer ReplaceSingleton<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImpl>() where TService : class where TImpl : class, TService
        => RemoveAll<TService>().Add(ServiceDescriptor.Singleton<TService, TImpl>());

    /// <summary>
    /// 替换单例（工厂）。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <param name="factory">工厂方法，参数为当前解析上下文的容器。</param>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer ReplaceSingleton<TService>(Func<DiContainer, TService> factory) where TService : class
        => RemoveAll<TService>().Add(ServiceDescriptor.Singleton<TService>(sp => factory(sp.AsDiContainer())));

    /// <summary>
    /// 替换作用域（实例）。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <param name="instance">替换后的服务实例。</param>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer ReplaceScoped<TService>(TService instance) where TService : class
        => RemoveAll<TService>().Add(ServiceDescriptor.Scoped(_ => instance));

    /// <summary>
    /// 替换作用域（服务 + 实现）。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <typeparam name="TImpl">实现类型。</typeparam>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer ReplaceScoped<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImpl>() where TService : class where TImpl : class, TService
        => RemoveAll<TService>().Add(ServiceDescriptor.Scoped<TService, TImpl>());

    /// <summary>
    /// 替换作用域（工厂）。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <param name="factory">工厂方法，参数为当前解析上下文的容器。</param>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer ReplaceScoped<TService>(Func<DiContainer, TService> factory) where TService : class
        => RemoveAll<TService>().Add(ServiceDescriptor.Scoped<TService>(sp => factory(sp.AsDiContainer())));

    /// <summary>
    /// 移除指定服务类型的全部注册。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer RemoveAll<TService>() => RemoveAll(typeof(TService));

    /// <summary>
    /// 移除指定服务类型的全部注册。
    /// </summary>
    /// <param name="serviceType">服务类型。</param>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer RemoveAll(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        lock (_sync)
        {
            ServiceCollectionMutationGuard();
            for (var i = _services.Count - 1; i >= 0; i--)
            {
                if (_services[i].ServiceType == serviceType) _services.RemoveAt(i);
            }
            return this;
        }
    }

    // ---------------- 构建 / 作用域 ----------------

    /// <summary>
    /// 构建容器（幂等：已构建直接返回本实例，不重建）。
    /// </summary>
    /// <param name="validateScopes">校验 scoped 服务不得从根解析（默认开启，与 AgentHostBuilder 现状一致）。</param>
    /// <param name="validateOnBuild">构建时校验全部注册可被实例化（默认开启）。</param>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer Build(bool validateScopes = true, bool validateOnBuild = true)
    {
        lock (_sync)
        {
            ThrowIfDisposed();
            if (!_canBuild)
                throw new InvalidOperationException("只有宿主根容器可以构建；视图与作用域容器不可构建。");

            if (_provider != null) return this;   // 幂等

            _provider = _services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateScopes = validateScopes,
                ValidateOnBuild = validateOnBuild
            });
            _ownsProvider = true;
            return this;
        }
    }

    /// <summary>
    /// 重建容器：每次重建并释放旧 provider。用于启动期「先注册、后整体构建」的宿主流程。
    /// </summary>
    /// <param name="validateScopes">校验 scoped 服务不得从根解析。</param>
    /// <param name="validateOnBuild">构建时校验全部注册。</param>
    /// <param name="deferOldDispose">旧 provider 的延迟释放时长；null 表示立即释放。</param>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer Rebuild(bool validateScopes = false, bool validateOnBuild = false, TimeSpan? deferOldDispose = null)
    {
        lock (_sync)
        {
            ThrowIfDisposed();
            if (!_canBuild)
                throw new InvalidOperationException("只有宿主根容器可以构建；视图与作用域容器不可构建。");

            var old = _provider;
            _provider = _services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateScopes = validateScopes,
                ValidateOnBuild = validateOnBuild
            });
            _ownsProvider = true;

            if (old != null && !ReferenceEquals(old, _provider))
                DisposeProvider(old, deferOldDispose);

            return this;
        }
    }

    /// <summary>
    /// 接管外部服务集合（如 ASP.NET 的 builder.Services）；不触碰已构建的 provider，接管后需调用 <see cref="Rebuild"/>。
    /// </summary>
    /// <param name="services">被接管的服务集合。</param>
    /// <returns>当前实例，便于链式调用。</returns>
    public DiContainer ResetServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        // 调用方可能把容器自身（它实现 IServiceCollection）传进来；此处短路，
        // 否则 _services 自引用会让 Build 枚举时无限递归。
        if (ReferenceEquals(services, this)) return this;
        lock (_sync)
        {
            ThrowIfDisposed();
            if (!_canBuild)
                throw new InvalidOperationException("只有宿主根容器可以替换服务集合。");
            _services = services;
            return this;
        }
    }

    /// <summary>
    /// 从根派生一个作用域容器。语义与 MS.DI 一致：<b>永远从根派生</b>，不继承调用方作用域的身份状态。
    /// </summary>
    /// <returns>新的作用域容器，释放它不影响根容器。</returns>
    public virtual DiContainer CreateScope()
    {
        ThrowIfDisposed();
        var root = _root ?? this;
        // 未构建时 RequireProvider 抛「容器尚未构建」
        var scope = root.RequireProvider().GetRequiredService<IServiceScopeFactory>().CreateScope();
        var child = new DiContainer(scope, root);
        root.TrackScope(child);
        return child;
    }

    /// <summary>
    /// 登记子作用域，供根容器统一释放。
    /// </summary>
    /// <param name="child">派生的子作用域容器。</param>
    private void TrackScope(DiContainer child)
    {
        lock (_sync) _children.Add(child);
    }

    /// <summary>
    /// 注销子作用域（子作用域先于根释放时调用）。
    /// </summary>
    /// <param name="child">被释放的子作用域容器。</param>
    private void DetachScope(DiContainer child)
    {
        lock (_sync) _children.Remove(child);
    }

    /// <summary>
    /// 释放旧 provider：立即释放，或延迟指定时长后再释放。
    /// </summary>
    /// <param name="provider">待释放的 provider。</param>
    /// <param name="deferOldDispose">延迟时长；null 表示立即释放。</param>
    private static void DisposeProvider(IServiceProvider provider, TimeSpan? deferOldDispose)
    {
        if (deferOldDispose is { } delay && delay > TimeSpan.Zero)
        {
            // 延迟销毁旧 provider，避免正在使用的服务被意外销毁
            _ = Task.Run(async () =>
            {
                await Task.Delay(delay).ConfigureAwait(false);
                if (provider is IAsyncDisposable ad) await ad.DisposeAsync().ConfigureAwait(false);
                else (provider as IDisposable)?.Dispose();
            });
            return;
        }

        if (provider is IDisposable disposable) disposable.Dispose();
    }

    // ---------------- 释放 ----------------

    /// <summary>
    /// 释放容器：先释放全部子作用域，再按所有权释放自身 provider。
    /// </summary>
    public virtual void Dispose() => DisposeCoreAsync().AsTask().GetAwaiter().GetResult();

    /// <summary>
    /// 异步释放容器：先释放全部子作用域，再按所有权释放自身 provider。
    /// </summary>
    /// <returns>释放任务。</returns>
    public virtual ValueTask DisposeAsync() => DisposeCoreAsync();

    /// <summary>
    /// 释放实现：幂等，作用域只释放自己的 scope，不释放根。
    /// </summary>
    /// <returns>释放任务。</returns>
    private async ValueTask DisposeCoreAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1) return;

        DiContainer[] children;
        lock (_sync)
        {
            children = _children.ToArray();
            _children.Clear();
        }
        foreach (var child in children) await child.DisposeAsync().ConfigureAwait(false);

        if (_scope != null)
        {
            // scope：只释放自己的 scope，不释放根
            _scope.Dispose();
            _scope = null;
            _root?.DetachScope(this);
        }
        else if (_ownsProvider && _provider != null)
        {
            if (_provider is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            else
                (_provider as IDisposable)?.Dispose();
        }

        _provider = null;
        GC.SuppressFinalize(this);
    }

    // ---------------- IServiceScopeFactory 显式实现 ----------------

    /// <summary>
    /// <see cref="IServiceScopeFactory"/> 显式实现（互操作兜底，产品代码请直接用 <see cref="CreateScope()"/>）。
    /// </summary>
    /// <returns>标准作用域。</returns>
    IServiceScope IServiceScopeFactory.CreateScope() => new DiContainerScope(this.CreateScope());

    // ---------------- IServiceCollection 显式实现 ----------------

    /// <summary>
    /// 当前模式下是否允许注册。
    /// </summary>
    private bool CanRegister =>
        _mode is DiContainerMode.Root or DiContainerMode.CollectionView && _provider == null;

    /// <summary>
    /// 服务描述符数量。
    /// </summary>
    int ICollection<ServiceDescriptor>.Count
    {
        get
        {
            lock (_sync) return _services.Count;
        }
    }

    /// <summary>
    /// 是否只读（不可注册的模式或已构建时为 true）。
    /// </summary>
    bool ICollection<ServiceDescriptor>.IsReadOnly
    {
        get
        {
            lock (_sync) return !CanRegister;
        }
    }

    /// <summary>
    /// 按索引读写服务描述符。
    /// </summary>
    /// <param name="index">索引。</param>
    /// <value>该索引位置的服务描述符。</value>
    ServiceDescriptor IList<ServiceDescriptor>.this[int index]
    {
        get
        {
            lock (_sync) return _services[index];
        }
        set
        {
            lock (_sync)
            {
                ServiceCollectionMutationGuard();
                _services[index] = value;
            }
        }
    }

    /// <summary>
    /// 追加一条服务描述符。
    /// </summary>
    /// <param name="item">服务描述符。</param>
    void ICollection<ServiceDescriptor>.Add(ServiceDescriptor item) => Add(item);

    /// <summary>
    /// 清空全部服务描述符。
    /// </summary>
    void ICollection<ServiceDescriptor>.Clear()
    {
        lock (_sync)
        {
            ServiceCollectionMutationGuard();
            _services.Clear();
        }
    }

    /// <summary>
    /// 是否包含指定服务描述符。
    /// </summary>
    /// <param name="item">服务描述符。</param>
    /// <returns>包含返回 true。</returns>
    bool ICollection<ServiceDescriptor>.Contains(ServiceDescriptor item)
    {
        lock (_sync) return _services.Contains(item);
    }

    /// <summary>
    /// 复制全部服务描述符到数组。
    /// </summary>
    /// <param name="array">目标数组。</param>
    /// <param name="arrayIndex">起始索引。</param>
    void ICollection<ServiceDescriptor>.CopyTo(ServiceDescriptor[] array, int arrayIndex)
    {
        lock (_sync) _services.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// 枚举服务描述符（快照，避免遍历期间被修改）。
    /// </summary>
    /// <returns>服务描述符枚举器。</returns>
    IEnumerator<ServiceDescriptor> IEnumerable<ServiceDescriptor>.GetEnumerator()
    {
        lock (_sync) return _services.ToList().GetEnumerator();
    }

    /// <summary>
    /// 枚举服务描述符（快照，避免遍历期间被修改）。
    /// </summary>
    /// <returns>非泛型枚举器。</returns>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        lock (_sync) return ((System.Collections.IEnumerable)_services.ToList()).GetEnumerator();
    }

    /// <summary>
    /// 查找指定服务描述符的索引。
    /// </summary>
    /// <param name="item">服务描述符。</param>
    /// <returns>索引，未找到返回 -1。</returns>
    int IList<ServiceDescriptor>.IndexOf(ServiceDescriptor item)
    {
        lock (_sync) return _services.IndexOf(item);
    }

    /// <summary>
    /// 在指定位置插入服务描述符。
    /// </summary>
    /// <param name="index">索引。</param>
    /// <param name="item">服务描述符。</param>
    void IList<ServiceDescriptor>.Insert(int index, ServiceDescriptor item)
    {
        lock (_sync)
        {
            ServiceCollectionMutationGuard();
            _services.Insert(index, item);
        }
    }

    /// <summary>
    /// 移除指定位置的服务描述符。
    /// </summary>
    /// <param name="index">索引。</param>
    void IList<ServiceDescriptor>.RemoveAt(int index)
    {
        lock (_sync)
        {
            ServiceCollectionMutationGuard();
            _services.RemoveAt(index);
        }
    }

    /// <summary>
    /// 移除指定服务描述符。
    /// </summary>
    /// <param name="item">服务描述符。</param>
    /// <returns>移除成功返回 true。</returns>
    bool ICollection<ServiceDescriptor>.Remove(ServiceDescriptor item)
    {
        lock (_sync)
        {
            ServiceCollectionMutationGuard();
            return _services.Remove(item);
        }
    }

    // ---------------- 内部 ----------------

    /// <summary>
    /// 追加注册并校验当前容器是否可注册。
    /// </summary>
    /// <param name="descriptor">服务描述符。</param>
    /// <returns>当前实例，便于链式调用。</returns>
    private DiContainer Add(ServiceDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        lock (_sync)
        {
            ServiceCollectionMutationGuard();
            _services.Add(descriptor);
            return this;
        }
    }

    /// <summary>
    /// 不存在同类型注册时才追加。
    /// </summary>
    /// <param name="descriptor">服务描述符。</param>
    /// <returns>当前实例，便于链式调用。</returns>
    private DiContainer TryAdd(ServiceDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        lock (_sync)
        {
            ServiceCollectionMutationGuard();
            for (var i = 0; i < _services.Count; i++)
                if (_services[i].ServiceType == descriptor.ServiceType) return this;
            _services.Add(descriptor);
            return this;
        }
    }

    /// <summary>
    /// 注册合法性：ProviderView / Scope 不可注册，已构建的根不可再注册。
    /// </summary>
    private void ServiceCollectionMutationGuard()
    {
        if (_mode is DiContainerMode.ProviderView or DiContainerMode.Scope)
            throw new InvalidOperationException("当前 DiContainer 视图不可注册服务。");
        if (_provider != null)
            throw new InvalidOperationException("容器已构建，不可再注册服务；请先 ResetServices 或新建容器。");
    }
}