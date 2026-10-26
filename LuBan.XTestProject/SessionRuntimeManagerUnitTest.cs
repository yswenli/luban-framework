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
        return (new SessionRuntimeManager(root, config), root);
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