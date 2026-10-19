/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： ConnectorRegistryUnitTest
*唯一标识：连接器抽象族单测
*创建时间：2026/10/19
*描述：验证 ① MCP 客户端对连接器是借用关系（反注册不得释放连接器，Q17 回归）；
*      ② 连接器状态解析（声明/启用/不可用/可用四态）；③ 按 MCP 名派生客户端并注入权限环境；
*      ④ 内置工厂按实例名各造一个独立连接器（暴露面名/凭据键跟随实例名）；
*      ⑤ 按 MCP 名查找优先命中声明实例名且不实例化其它连接器、每实例只初始化一次；
*      ⑥ PAT 按连接器实例名取用与泛化凭据键的环境变量映射。
*
*****************************************************************************/
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Connectors;
using LuBan.AIAgent.Connectors.AliyunDevOps;
using LuBan.AIAgent.Connectors.AliyunDevOps.Mcp;
using LuBan.AIAgent.MCP;
using LuBan.AIAgent.Plugins.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LuBan.XTestProject;

[TestClass]
public class ConnectorRegistryUnitTest
{
    private sealed class StubCredentialProvider : IYunxiaoCredentialProvider
    {
        public Task<string?> GetPatAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<string?>("stub-pat");
    }

    private sealed class FakeConfigReader : IAppConfigReader
    {
        public List<ProviderConfigData> Providers { get; } = new();
        public string? SelectedModel => null;
        public List<CustomSkillConfig> CustomSkills { get; } = new();
        public List<CustomRuleConfig> CustomRules { get; } = new();
        public List<McpServerConfig> McpServers { get; } = new();
        public List<ConnectorConfig> Connectors { get; } = new();
        public List<string> DisabledBuiltinSkills { get; } = new();
        public List<string> DisabledBuiltinRules { get; } = new();
        public List<string> DisabledBuiltinMcpClients { get; } = new();
    }

    private sealed class DummyConnector : IConnector
    {
        public string Key => "demo";
        public string Name => "demo";
        public string DisplayName => "演示连接器";
        public IReadOnlyList<ConnectorMcpExposure> McpExposures => Array.Empty<ConnectorMcpExposure>();
        public ValueTask InitializeAsync(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private static ServiceProvider BuildRoot()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new YunxiaoApiOptions());
        services.AddSingleton<IYunxiaoCredentialProvider>(new StubCredentialProvider());
        services.AddSingleton(sp => new YunxiaoConnector(
            sp.GetRequiredService<YunxiaoApiOptions>(),
            sp.GetRequiredService<IYunxiaoCredentialProvider>()));
        services.AddSingleton(new ConnectorFactoryRegistration(
            YunxiaoConnector.ProviderKey,
            ctx => ctx.Services.GetRequiredService<YunxiaoConnector>()));
        // 有工厂但未被 config.json 声明的提供方（用于 Available 状态）
        services.AddSingleton(new ConnectorFactoryRegistration("demo", _ => new DummyConnector()));
        return services.BuildServiceProvider();
    }

    private static (ConnectorRegistry Registry, FakeConfigReader Reader) CreateRegistry(ServiceProvider root)
    {
        var factories = new ConnectorFactoryRegistry(root.GetServices<ConnectorFactoryRegistration>());
        var host = new ConnectorInstanceHost(root, factories);
        var reader = new FakeConfigReader();
        var registry = new ConnectorRegistry(host, factories, null, reader, new ConfigurationBuilder().Build());
        return (registry, reader);
    }

    /// <summary>
    /// Q17 回归：MCP 客户端释放（或经注册表反注册）后，被借用的连接器必须仍然可用。
    /// </summary>
    [TestMethod]
    public void DisposeMCPClient_DoesNotDisposeBorrowedConnector()
    {
        using var root = BuildRoot();
        var connector = root.GetRequiredService<YunxiaoConnector>();

        var client = new YunxiaoMCPClient(connector);
        client.Dispose();

        // 修复前：此处会抛 ObjectDisposedException（客户端连坐释放了共享连接器）
        var codeup = connector.GetCodeupClient();
        Assert.IsNotNull(codeup);
    }

    /// <summary>
    /// 会话 A 反注册 MCP 后，会话 B 仍可正常调用（连接器为宿主级单例，不被任一 sf 会话释放）。
    /// </summary>
    [TestMethod]
    public void UnregisterPluginClient_KeepsConnectorUsableForOtherSessions()
    {
        using var root = BuildRoot();
        var connector = root.GetRequiredService<YunxiaoConnector>();

        var sessionA = new MCPRegistry(Array.Empty<IMCPClient>());
        var sessionB = new MCPRegistry(Array.Empty<IMCPClient>());

        sessionA.RegisterPluginClient(new YunxiaoMCPClient(connector));
        sessionB.RegisterPluginClient(new YunxiaoMCPClient(connector));

        Assert.IsTrue(sessionA.UnregisterPluginClient("yunxiao"));

        // 会话 A 已注销，会话 B 与连接器均须不受影响
        Assert.IsFalse(sessionA.Contains("yunxiao"));
        Assert.IsTrue(sessionB.Contains("yunxiao"));
        Assert.IsNotNull(sessionB.Get("yunxiao"));
        Assert.IsNotNull(connector.GetCodeupClient());
    }

    /// <summary>
    /// 状态解析：已声明且启用 → Enabled；已声明但未启用 → Declared；提供方无工厂 → Unavailable；
    /// 有工厂但未声明 → Available。
    /// </summary>
    [TestMethod]
    public void GetStates_ResolvesDeclaredEnabledAndUnavailable()
    {
        using var root = BuildRoot();
        var (registry, reader) = CreateRegistry(root);

        reader.Connectors.Add(new ConnectorConfig { Name = "yunxiao", Provider = "aliyunDevOps", Enabled = true });
        reader.Connectors.Add(new ConnectorConfig { Name = "ghost", Provider = "notRegistered", Enabled = true });

        var states = registry.GetStates();

        var yunxiao = states.Single(s => s.Name == "yunxiao");
        Assert.AreEqual(ConnectorState.Enabled, yunxiao.State);

        var ghost = states.Single(s => s.Name == "ghost");
        Assert.AreEqual(ConnectorState.Unavailable, ghost.State);

        // 工厂已注册但未被 config.json 使用 → Available
        var demo = states.Single(s => s.ProviderKey == "demo");
        Assert.AreEqual(ConnectorState.Available, demo.State);

        // 已声明的连接器不再重复产出 Available 行
        Assert.IsFalse(states.Any(s => s.Name == "yunxiao" && s.State == ConnectorState.Available));
    }

    /// <summary>
    /// 工作区启用了宿主未声明的连接器 → Unavailable（按未安装处理，不放行）。
    /// </summary>
    [TestMethod]
    public void GetStates_WorkspaceEnablingUndeclaredConnector_IsUnavailable()
    {
        using var root = BuildRoot();
        var (registry, _) = CreateRegistry(root);

        var workspace = Path.Combine(Path.GetTempPath(), "luban-connectors-" + Guid.NewGuid().ToString("N"));
        var dir = Path.Combine(workspace, ".luban-agent", "connectors");
        Directory.CreateDirectory(dir);

        try
        {
            File.WriteAllText(Path.Combine(dir, "ghost.json"), "{\"enabled\":true}");

            var states = registry.GetStates(workspace);

            var ghost = states.Single(s => s.Name == "ghost");
            Assert.AreEqual(ConnectorState.Unavailable, ghost.State);
        }
        finally
        {
            Directory.Delete(workspace, recursive: true);
        }
    }

    /// <summary>
    /// 按 MCP 名派生客户端：必须来自连接器暴露面，且注入显式权限环境（Q10）。
    /// </summary>
    [TestMethod]
    public void CreateMcpClient_InjectsExplicitPermissionScope()
    {
        using var root = BuildRoot();
        var (registry, reader) = CreateRegistry(root);

        reader.Connectors.Add(new ConnectorConfig { Name = "yunxiao", Provider = "aliyunDevOps", Enabled = true });

        var scope = new PluginPermissionScope("LubanAgent.Plugin.NewProject", null);
        var client = registry.CreateMcpClient("yunxiao", scope);

        Assert.IsNotNull(client);
        Assert.AreEqual("yunxiao", client!.Name);

        var typed = client as YunxiaoMCPClient;
        Assert.IsNotNull(typed);
        Assert.AreSame(scope, typed!.PermissionScope);
    }

    /// <summary>
    /// R1 回归：内置工厂必须按<b>实例名</b>各造一个独立连接器（修复前复用容器单例并改写共享
    /// <c>Name</c>，导致多实例互相覆盖、暴露面名恒为 <c>yunxiao</c>）。
    /// </summary>
    [TestMethod]
    public void BuiltinConnectorFactory_CreatesIndependentConnectorPerInstanceName()
    {
        var services = new ServiceCollection();
        services.AddBuiltinConnectors(new ConfigurationBuilder().Build());

        using var root = services.BuildServiceProvider();
        var factories = new ConnectorFactoryRegistry(root.GetServices<ConnectorFactoryRegistration>());
        var host = new ConnectorInstanceHost(root, factories);

        var primary = host.GetOrCreate("yunxiao", YunxiaoConnector.ProviderKey);
        var secondary = host.GetOrCreate("yunxiao2", YunxiaoConnector.ProviderKey);
        // 同一实例名重复取用必须是同一对象（宿主级单例语义，O2 双检）
        var again = host.GetOrCreate("yunxiao2", YunxiaoConnector.ProviderKey);

        Assert.IsNotNull(primary);
        Assert.IsNotNull(secondary);
        Assert.AreNotSame(primary, secondary);
        Assert.AreSame(secondary, again);

        Assert.AreEqual("yunxiao", primary!.Name);
        Assert.AreEqual("yunxiao2", secondary!.Name);

        // 暴露面名（MCP 名）与客户端名均跟随实例名
        Assert.AreEqual("yunxiao", primary.McpExposures.Single().Name);
        Assert.AreEqual("yunxiao2", secondary.McpExposures.Single().Name);
        Assert.AreEqual("yunxiao", primary.McpExposures.Single().CreateClient(root, null).Name);
        Assert.AreEqual("yunxiao2", secondary.McpExposures.Single().CreateClient(root, null).Name);
    }

    /// <summary>
    /// R5 回归：按 MCP 名查找优先命中「声明实例名 == MCP 名」的连接器，命中即返回——
    /// 其余已声明连接器<b>不</b>被顺带实例化；且 <c>InitializeAsync</c> 每实例只执行一次。
    /// </summary>
    [TestMethod]
    public void CreateMcpClient_PrefersInstanceNameMatch_AndInitializesOnce()
    {
        var initCount = 0;
        var otherCreated = 0;
        var innerYunxiao = new YunxiaoConnector(new YunxiaoApiOptions(), new StubCredentialProvider());

        var services = new ServiceCollection();
        services.AddSingleton(new ConnectorFactoryRegistration("spy",
            _ => new SpyConnector(innerYunxiao, () => initCount++)));
        services.AddSingleton(new ConnectorFactoryRegistration("other", _ =>
        {
            otherCreated++;
            return new DummyConnector();
        }));

        using var root = services.BuildServiceProvider();
        var factories = new ConnectorFactoryRegistry(root.GetServices<ConnectorFactoryRegistration>());
        var host = new ConnectorInstanceHost(root, factories);
        var reader = new FakeConfigReader();
        var registry = new ConnectorRegistry(host, factories, null, reader, new ConfigurationBuilder().Build());

        reader.Connectors.Add(new ConnectorConfig { Name = "spy", Provider = "spy", Enabled = true });
        reader.Connectors.Add(new ConnectorConfig { Name = "other", Provider = "other", Enabled = true });

        Assert.IsNotNull(registry.CreateMcpClient("spy"));
        Assert.IsNotNull(registry.CreateMcpClient("spy"));

        Assert.AreEqual(1, initCount, "InitializeAsync 每个实例只能执行一次");
        Assert.AreEqual(0, otherCreated, "命中的是实例名精确匹配者，其它已声明连接器不应被实例化");
    }

    /// <summary>
    /// R2 回归：PAT 按<b>连接器实例名</b>取用（凭据键 <c>connector:&lt;实例名&gt;:token</c>）。
    /// </summary>
    [TestMethod]
    public async Task YunxiaoCredentialProvider_ResolvesPatByConnectorInstanceName()
    {
        var store = new FakeCredentialStore();
        store.Set("connector:yunxiao2:token", "pat-of-instance-2");

        var services = new ServiceCollection();
        services.AddSingleton<ICredentialStore>(store);
        services.AddYunxiaoConnector(new ConfigurationBuilder().Build());

        using var root = services.BuildServiceProvider();
        var provider = root.GetRequiredService<IYunxiaoCredentialProvider>();

        Assert.AreEqual("pat-of-instance-2", await provider.GetPatAsync("yunxiao2"));
        Assert.IsNull(await provider.GetPatAsync("yunxiao"));
        Assert.IsNull(await provider.GetPatAsync());
    }

    /// <summary>
    /// Q15：泛化连接器令牌键的环境变量映射（非默认实例）。
    /// </summary>
    [TestMethod]
    public void CredentialKeys_EnvironmentVariableName_CoversConnectorInstances()
    {
        Assert.AreEqual("LUBAN_YUNXIAO_PAT", CredentialKeys.GetEnvironmentVariableName("connector:yunxiao:token"));
        Assert.AreEqual("LUBAN_CONNECTOR_YUNXIAO2_TOKEN", CredentialKeys.GetEnvironmentVariableName("connector:yunxiao2:token"));
        Assert.AreEqual("LUBAN_CONNECTOR_MY_INST_V2_TOKEN", CredentialKeys.GetEnvironmentVariableName("connector:my-inst.v2:token"));
        Assert.AreEqual("LUBAN_YUNXIAO_ORGANIZATION_ID", CredentialKeys.GetEnvironmentVariableName("yunxiao_organization_id"));
        Assert.IsNull(CredentialKeys.GetEnvironmentVariableName("connector:token"));
        Assert.IsNull(CredentialKeys.GetEnvironmentVariableName("some_unknown_key"));
    }

    /// <summary>可计数连接器：暴露面名跟随实例名，客户端借用一个真实云效连接器构造。</summary>
    private sealed class SpyConnector : IConnector
    {
        private readonly YunxiaoConnector _inner;
        private readonly Action _onInitialize;

        public SpyConnector(YunxiaoConnector inner, Action onInitialize)
        {
            _inner = inner;
            _onInitialize = onInitialize;
        }

        public string Key => "spy";
        public string Name { get; set; } = "spy";
        public string DisplayName => "计数连接器";

        public IReadOnlyList<ConnectorMcpExposure> McpExposures => new[]
        {
            new ConnectorMcpExposure(Name, "计数连接器", (_, scope) => new YunxiaoMCPClient(_inner) { PermissionScope = scope })
        };

        public ValueTask InitializeAsync(CancellationToken cancellationToken = default)
        {
            _onInitialize();
            return ValueTask.CompletedTask;
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    /// <summary>内存凭据存储（键不做归一化，便于验证实例名维度的取用）。</summary>
    private sealed class FakeCredentialStore : ICredentialStore
    {
        private readonly Dictionary<string, string> _values = new(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyCollection<string> Keys => _values.Keys;

        public void Set(string key, string value) => _values[key] = value;

        public bool Contains(string key) => _values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value);

        public Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
            => Task.FromResult(_values.TryGetValue(key, out var value) ? value : null);

        public Task SetAsync(string key, string? value, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(value)) _values.Remove(key);
            else _values[key] = value;

            return Task.CompletedTask;
        }
    }
}