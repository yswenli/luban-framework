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
*命名空间：LuBan.XTestProject
*文件名： DiContainerUnitTest
*版本号： V1.0.0.0
*唯一标识：3f9604ab-5324-461b-a696-e6745f85bb90
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/30 11:11:28
*描述：LuBan 统一 DI 容器单元测试（注册、作用域与释放语义）
*
*=================================================
*修改标记
*修改时间：2026/10/30 11:11:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：LuBan 统一 DI 容器单元测试（注册、作用域与释放语义）
*
*****************************************************************************/

using LuBan.DI;
using Microsoft.Extensions.DependencyInjection;

namespace LuBan.XTestProject;

[TestClass]
public class DiContainerUnitTest
{
    private DiContainer? _previousGlobal;

    [TestInitialize]
    public void Setup() => _previousGlobal = DiContainer.Instance;

    [TestCleanup]
    public void Cleanup() => _previousGlobal?.UseAsGlobal();

    private sealed class Foo { }
    private sealed class Bar(Foo foo) { public Foo Foo { get; } = foo; }

    [TestMethod]
    public void InstanceMethod_WinsOverExtensionMethod()
    {
        var di = new DiContainer();
        var bar = new Bar(new Foo());
        di.AddSingleton<Bar>(bar);              // 实例重载（instance）
        di.Build();

        Assert.AreSame(bar, di.GetRequiredService<Bar>());
    }

    [TestMethod]
    public void FactoryReceivesDiContainer()
    {
        var di = new DiContainer();
        DiContainer? seen = null;
        di.AddSingleton<Foo>();
        di.AddSingleton<Bar>(d => { seen = d; return new Bar(d.GetRequiredService<Foo>()); });
        di.Build();

        var bar = di.GetRequiredService<Bar>();
        Assert.IsNotNull(seen);
        Assert.IsInstanceOfType<DiContainer>(seen);
        Assert.AreSame(bar.Foo, di.GetRequiredService<Foo>());
    }

    [TestMethod]
    public void AsDiContainer_ThreeOverloads_Resolve()
    {
        var di = new DiContainer();
        di.AddSingleton<Foo>();
        di.Build();

        // 1) DiContainer → 自身
        Assert.AreSame(di, di.AsDiContainer());

        // 2) IServiceProvider → 已构建视图（可解析）
        var providerView = ((IServiceProvider)di).AsDiContainer();
        Assert.IsNotNull(providerView.GetRequiredService<Foo>());

        // 3) IServiceCollection → 注册视图（可注册、不可 Build）
        var collectionView = new ServiceCollection().AsDiContainer();
        collectionView.AddSingleton<Foo>();
        Assert.IsFalse(collectionView.IsBuilt);
    }

    [TestMethod]
    public void Build_IsIdempotent_AndViewCannotBuild()
    {
        var di = new DiContainer();
        di.AddSingleton<Foo>();
        Assert.AreSame(di, di.Build());
        Assert.AreSame(di, di.Build());                       // 幂等，未重建

        var view = ((IServiceProvider)di).AsDiContainer();
        Assert.ThrowsExactly<InvalidOperationException>(() => view.Build());
        Assert.ThrowsExactly<InvalidOperationException>(() => new ServiceCollection().AsDiContainer().Build());
    }

    [TestMethod]
    public void CreateScope_DerivesFromRoot()
    {
        var di = new DiContainer();
        di.AddScoped<Foo>();
        di.Build();

        var scope1 = di.CreateScope();
        var scope2 = scope1.CreateScope();      // 从 scope 派生的仍然来自根

        Assert.AreSame(di, scope1.Parent);
        Assert.AreSame(di, scope2.Parent);
        Assert.AreNotSame(scope1.GetRequiredService<Foo>(), scope2.GetRequiredService<Foo>());
        Assert.AreSame(scope1.GetRequiredService<Foo>(), scope1.GetRequiredService<Foo>());

        scope1.Dispose();
        scope2.Dispose();
    }

    [TestMethod]
    public void OwnsProvider_DisposeSemantics()
    {
        var built = new DiContainer();
        built.AddSingleton<Foo>().Build();
        built.Dispose();
        Assert.ThrowsExactly<ObjectDisposedException>(() => built.GetRequiredService<Foo>());

        var inner = new ServiceCollection().AddSingleton<Foo>().BuildServiceProvider();
        var view = ((IServiceProvider)inner).AsDiContainer();
        view.Dispose();                                        // 不得释放被包装对象
        Assert.IsNotNull(inner.GetService<Foo>());
        inner.Dispose();
    }

    [TestMethod]
    public void GetServiceBeforeBuild_ReturnsNull_RequiredThrows()
    {
        var di = new DiContainer();
        di.AddSingleton<Foo>();

        Assert.IsNull(di.GetService<Foo>());
        Assert.ThrowsExactly<InvalidOperationException>(() => di.GetRequiredService<Foo>());
    }

    [TestMethod]
    public void ResetServices_ThenRebuild_RefreshesGlobalProvider()
    {
        var di = new DiContainer().UseAsGlobal();
        var services = new ServiceCollection();
        services.AddSingleton<Foo>();

        di.ResetServices(services);
        di.Rebuild();

        Assert.AreSame(di, DiContainer.Instance);
        Assert.IsNotNull(DiContainer.Instance.GetRequiredService<Foo>());
    }

    [TestMethod]
    public void ScopeFactoryAdapter_ReturnsWorkingScope()
    {
        var di = new DiContainer();
        di.AddScoped<Foo>();
        di.Build();

        var factory = (IServiceScopeFactory)di;
        using var scope = factory.CreateScope();
        Assert.IsNotNull(scope.ServiceProvider.GetRequiredService<Foo>());
    }

    [TestMethod]
    public void ReplaceAndTryAdd_Semantics()
    {
        var di = new DiContainer();
        di.AddSingleton<Foo>();
        di.AddSingleton<Foo>();          // 两条注册
        var replacement = new Foo();
        di.ReplaceSingleton<Foo>(replacement);
        di.TryAddSingleton<Bar>();       // 不存在才加
        di.TryAddSingleton<Bar>();       // 第二次忽略
        di.Build();

        Assert.AreSame(replacement, di.GetRequiredService<Foo>());
        Assert.AreEqual(1, di.GetServices<Foo>().Count());   // Replace 必须只留一条
        Assert.AreEqual(1, di.GetServices<Bar>().Count());
    }
}
