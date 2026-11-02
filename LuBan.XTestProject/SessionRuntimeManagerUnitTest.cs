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
*文件名： SessionRuntimeManagerUnitTest
*版本号： V1.0.0.0
*唯一标识：3bd8b689-e82e-4028-8198-33c89976e1c8
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/26 14:55:57
*描述：会话运行时管理器单元测试（会话复用与初始化器）
*
*=================================================
*修改标记
*修改时间：2026/10/26 14:55:57
*修改人： yswenli
*版本号： V1.0.0.0
*描述：会话运行时管理器单元测试（会话复用与初始化器）
*
*****************************************************************************/

using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Defaults;
using LuBan.AIAgent.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LuBan.XTestProject;

[TestClass]
public class SessionRuntimeManagerUnitTest
{
    private static (SessionRuntimeManager Manager, ServiceProvider Root) Build()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging();
        services.AddOptions();
        services.AddScoped<DefaultContextState>();
        services.AddScoped<ISessionContext, DefaultSessionContext>();
        services.AddSingleton<ISessionRuntimeInitializer, RecordingInitializer>();
        var root = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
        return (new SessionRuntimeManager(root.AsDiContainer(), config), root);
    }

    [TestMethod]
    public async Task GetOrCreate_FillsContextState_AndCallsInitializers()
    {
        var (manager, _) = Build();

        var runtime = manager.GetOrCreate("s1", "u1", "w1", @"C:\ws");

        var state = runtime.Services.GetRequiredService<DefaultContextState>();
        Assert.AreEqual("s1", state.SessionId);
        Assert.AreEqual("w1", state.WorkspaceId);
        Assert.AreEqual("u1", state.UserId);
        Assert.AreEqual(@"C:\ws", state.WorkspaceRoot);
        Assert.IsTrue(state.AllowedRoots.Contains(@"C:\ws"));
        Assert.IsTrue(RecordingInitializer.Called);

        await manager.DisposeAsync();
    }

    [TestMethod]
    public async Task GetOrCreate_SameSession_ReturnsSameRuntime()
    {
        var (manager, _) = Build();

        var a = manager.GetOrCreate("s1", "u1", "w1", @"C:\ws");
        var b = manager.GetOrCreate("s1", "u1", "w1", @"C:\ws");

        Assert.AreSame(a, b);
        await manager.DisposeAsync();
    }

    private sealed class RecordingInitializer : ISessionRuntimeInitializer
    {
        public static bool Called;

        public Task InitializeAsync(SessionRuntimeSeed seed, CancellationToken ct = default)
        {
            Called = true;
            return Task.CompletedTask;
        }
    }
}