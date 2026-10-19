/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： ConnectorRegistryUnitTest
*唯一标识：连接器抽象族单测
*创建时间：2026/10/19
*描述：验证 ① MCP 客户端对连接器是借用关系（反注册不得释放连接器，Q17 回归）；
*      ② 连接器状态解析（声明/启用/不可用/可用四态）；③ 按 MCP 名派生客户端并注入权限环境；
*      ④ 宿主登记的工厂按实例名各造一个独立连接器（暴露面名跟随实例名）；
*      ⑤ 按 MCP 名查找优先命中声明实例名且不实例化其它连接器、每实例只初始化一次。
*
* 说明：框架已去厂商化，此处一律用测试自带的中性连接器（StubConnector / SpyConnector）；
*      厂商连接器（如云效）的行为测试在对应产品测试项目内。
*
*****************************************************************************/
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Connectors;
using LuBan.AIAgent.MCP;
using LuBan.AIAgent.Plugins.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LuBan.XTestProject;

[TestClass]
public class ConnectorRegistryUnitTest
{
    private const string DemoProviderKey = "demo";
    private const string DemoConnectorName = "demo";
    private const string SpareProviderKey = "spare";

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

    /// <summary>中性测试 MCP 客户端：可释放（用于借用关系回归），可承载显式权限环境。</summary>
    private sealed class StubMcpClient : MCPClientBase, IDisposable
    {
        public StubMcpClient(string name) => Name = name;

        public override string Name { get; }
        public override string Description => "测试 MCP 客户端";

        /// <summary>显式权限环境（Q10）。</summary>
        public PluginPermissionScope? PermissionScope { get; set; }

        public bool Disposed { get; private set; }

        public override Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
        {
            IsConnected = true;
            return Task.FromResult(true);
        }

        public override Task<IEnumerable<MCPTool>> ListToolsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IEnumerable<MCPTool>>(Array.Empty<MCPTool>());

        public override Task<MCPToolResult> CallToolAsync(
            string toolName,
            Dictionary<string, object?> arguments,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Ok(toolName));

        public void Dispose()
        {
            Disposed = true;
            DisconnectAsync().GetAwaiter().GetResult();
        }
    }

    /// <summary>中性测试连接器（派生自框架抽象基类 <see cref="ConnectorBase"/>）。</summary>
    private class StubConnector : ConnectorBase
    {
        public StubConnector(string providerKey) => Key = providerKey;

        public override string Key { get; }

        /// <summary>暴露面名跟随实例名（命名约定：MCP 名 == 实例名）。</summary>
        public override IReadOnlyList<ConnectorMcpExposure> McpExposures => new[]
        {
            new ConnectorMcpExposure(
                Name,
                "测试连接器",
                (_, scope) => new StubMcpClient(Name) { PermissionScope = scope })
        };

        public int InitializeCount { get; private set; }
        public bool Disposed { get; private set; }

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            InitializeCount++;
            return Task.CompletedTask;
        }

        protected override ValueTask OnDisposeAsync()
        {
            Disposed = true;
            return ValueTask.CompletedTask;
        }
    }

    private static ServiceProvider BuildRoot()
    {
        var services = new ServiceCollection();
        services.AddConnector<StubConnector>(DemoProviderKey, ctx => new StubConnector(ctx.ProviderKey) { Name = ctx.Name });
        // 有工厂但不会被声明的提供方（用于 Available 状态）
        services.AddConnector<StubConnector>(SpareProviderKey, ctx => new StubConnector(ctx.ProviderKey) { Name = ctx.Name });
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
    /// 分层回归：<see cref="ConnectorServiceCollectionExtensions.AddBuiltinConnectors"/> 只登记基础设施，
    /// <b>不</b>登记任何厂商连接器（厂商连接器由宿主 / 产品 / 插件经 <c>AddConnector</c> 注册）。
    /// </summary>
    [TestMethod]
    public void AddBuiltinConnectors_RegistersNoVendorConnector()
    {
        var services = new ServiceCollection();
        services.AddBuiltinConnectors(new ConfigurationBuilder().Build());

        using var root = services.BuildServiceProvider();
        var factories = new ConnectorFactoryRegistry(root.GetServices<ConnectorFactoryRegistration>());

        Assert.AreEqual(0, factories.ProviderKeys.Count, "框架不得登记厂商连接器提供方");
        Assert.IsFalse(factories.ProviderKeys.Any(k => k.Contains("aliyun", StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// Q17 回归：MCP 客户端释放（或经注册表反注册）后，被借用的连接器必须仍然可用。
    /// </summary>
    [TestMethod]
    public async Task DisposeMCPClient_DoesNotDisposeBorrowedConnector()
    {
        using var root = BuildRoot();
        var factories = new ConnectorFactoryRegistry(root.GetServices<ConnectorFactoryRegistration>());
        var host = new ConnectorInstanceHost(root, factories);
        var connector = (StubConnector)host.GetOrCreate(DemoConnectorName, DemoProviderKey)!;

        var client = (StubMcpClient)connector.McpExposures.Single().CreateClient(root, null);
        client.Dispose();

        Assert.IsTrue(client.Disposed);
        Assert.IsFalse(connector.Disposed, "客户端释放不得连坐释放被借用的连接器");

        // 修复前：此处会抛 ObjectDisposedException（客户端连坐释放了共享连接器）
        await connector.InitializeAsync();
    }

    /// <summary>
    /// 会话 A 反注册 MCP 后，会话 B 仍可正常调用（连接器为宿主级单例，不被任一会话释放）。
    /// </summary>
    [TestMethod]
    public void UnregisterPluginClient_KeepsConnectorUsableForOtherSessions()
    {
        using var root = BuildRoot();
        var factories = new ConnectorFactoryRegistry(root.GetServices<ConnectorFactoryRegistration>());
        var host = new ConnectorInstanceHost(root, factories);
        var connector = (StubConnector)host.GetOrCreate(DemoConnectorName, DemoProviderKey)!;

        var sessionA = new MCPRegistry(Array.Empty<IMCPClient>());
        var sessionB = new MCPRegistry(Array.Empty<IMCPClient>());

        sessionA.RegisterPluginClient(connector.McpExposures.Single().CreateClient(root, null));
        sessionB.RegisterPluginClient(connector.McpExposures.Single().CreateClient(root, null));

        Assert.IsTrue(sessionA.UnregisterPluginClient(DemoConnectorName));

        // 会话 A 已注销，会话 B 与连接器均须不受影响
        Assert.IsFalse(sessionA.Contains(DemoConnectorName));
        Assert.IsTrue(sessionB.Contains(DemoConnectorName));
        Assert.IsNotNull(sessionB.Get(DemoConnectorName));
        Assert.IsFalse(connector.Disposed);
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

        reader.Connectors.Add(new ConnectorConfig { Name = "primary", Provider = DemoProviderKey, Enabled = true });
        reader.Connectors.Add(new ConnectorConfig { Name = "idle", Provider = DemoProviderKey, Enabled = false });
        reader.Connectors.Add(new ConnectorConfig { Name = "ghost", Provider = "notRegistered", Enabled = true });

        var states = registry.GetStates();

        Assert.AreEqual(ConnectorState.Enabled, states.Single(s => s.Name == "primary").State);
        Assert.AreEqual(ConnectorState.Declared, states.Single(s => s.Name == "idle").State);
        Assert.AreEqual(ConnectorState.Unavailable, states.Single(s => s.Name == "ghost").State);

        // 工厂已注册但未被 config.json 使用 → Available（连接器 id == providerKey）
        Assert.AreEqual(ConnectorState.Available, states.Single(s => s.Name == SpareProviderKey).State);

        // 已声明的提供方不再重复产出 Available 行
        Assert.IsFalse(states.Any(s => s.State == ConnectorState.Available && s.Name != SpareProviderKey));
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

        reader.Connectors.Add(new ConnectorConfig { Name = DemoConnectorName, Provider = DemoProviderKey, Enabled = true });

        var scope = new PluginPermissionScope("LubanAgent.Plugin.NewProject", null);
        var client = registry.CreateMcpClient(DemoConnectorName, scope);

        Assert.IsNotNull(client);
        Assert.AreEqual(DemoConnectorName, client!.Name);

        var typed = client as StubMcpClient;
        Assert.IsNotNull(typed);
        Assert.AreSame(scope, typed!.PermissionScope);
    }

    /// <summary>
    /// R1 回归：宿主登记的工厂必须按<b>实例名</b>各造一个独立连接器（修复前复用容器单例并改写共享
    /// <c>Name</c>，导致多实例互相覆盖、暴露面名恒为旧值）。
    /// </summary>
    [TestMethod]
    public void RegisteredFactory_CreatesIndependentConnectorPerInstanceName()
    {
        var services = new ServiceCollection();
        services.AddBuiltinConnectors(new ConfigurationBuilder().Build());
        services.AddConnector<StubConnector>(DemoProviderKey, ctx => new StubConnector(ctx.ProviderKey) { Name = ctx.Name });

        using var root = services.BuildServiceProvider();
        var factories = new ConnectorFactoryRegistry(root.GetServices<ConnectorFactoryRegistration>());
        var host = new ConnectorInstanceHost(root, factories);

        var primary = host.GetOrCreate("demo1", DemoProviderKey);
        var secondary = host.GetOrCreate("demo2", DemoProviderKey);
        // 同一实例名重复取用必须是同一对象（宿主级单例语义，O2 双检）
        var again = host.GetOrCreate("demo2", DemoProviderKey);

        Assert.IsNotNull(primary);
        Assert.IsNotNull(secondary);
        Assert.AreNotSame(primary, secondary);
        Assert.AreSame(secondary, again);

        Assert.AreEqual("demo1", primary!.Name);
        Assert.AreEqual("demo2", secondary!.Name);

        // 暴露面名（MCP 名）与客户端名均跟随实例名
        Assert.AreEqual("demo1", primary.McpExposures.Single().Name);
        Assert.AreEqual("demo2", secondary.McpExposures.Single().Name);
        Assert.AreEqual("demo1", primary.McpExposures.Single().CreateClient(root, null).Name);
        Assert.AreEqual("demo2", secondary.McpExposures.Single().CreateClient(root, null).Name);
    }

    /// <summary>
    /// R5 回归：按 MCP 名查找优先命中「声明实例名 == MCP 名」的连接器，命中即返回——
    /// 其余已声明连接器<b>不</b>被顺带实例化；且 <c>InitializeAsync</c> 每实例只执行一次。
    /// </summary>
    [TestMethod]
    public void CreateMcpClient_PrefersInstanceNameMatch_AndInitializesOnce()
    {
        var otherCreated = 0;

        var services = new ServiceCollection();
        services.AddConnector<StubConnector>("spy", ctx => new StubConnector(ctx.ProviderKey) { Name = ctx.Name });
        services.AddSingleton(new ConnectorFactoryRegistration("other", _ =>
        {
            otherCreated++;
            return new StubConnector("other");
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

        var spy = (StubConnector)host.GetOrCreate("spy", "spy")!;
        Assert.AreEqual(1, spy.InitializeCount, "InitializeAsync 每个实例只能执行一次");
        Assert.AreEqual(0, otherCreated, "命中的是实例名精确匹配者，其它已声明连接器不应被实例化");
    }
}