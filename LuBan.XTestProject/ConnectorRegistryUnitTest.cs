/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： ConnectorRegistryUnitTest
*唯一标识：连接器抽象族单测
*创建时间：2026/10/19
*描述：验证 ① MCP 客户端对连接器是借用关系（反注册不得释放连接器，Q17 回归）；
*      ② 连接器状态解析（声明/启用/不可用/可用四态）；③ 按 MCP 名派生客户端并注入权限环境。
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
}