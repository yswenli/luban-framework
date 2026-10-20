/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： ConnectorManageToolGroupUnitTest
*创建时间：2026/10/20
*描述：连接器管理工具组单测 —— 对话生成连接器的「配置驱动」落点
*      ① 提供方必须已注册；② 疑似令牌必须走凭据引用（禁止明文落盘）；
*      ③ 写入即在工作区落 <name>.json 并被 ConnectorRegistry 识别；
*      ④ 删除 / 覆盖语义；⑤ 触发重载。
*
* 说明：框架已去厂商化，此处一律用测试自带的中性提供方（demo）。
*
*****************************************************************************/
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Connectors;
using LuBan.AIAgent.Tools.ConnectorManage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LuBan.XTestProject;

[TestClass]
public class ConnectorManageToolGroupUnitTest
{
    private const string DemoProviderKey = "demo";

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

    private sealed class StubConnector : ConnectorBase
    {
        public StubConnector(ConnectorBuildContext ctx)
        {
            Name = ctx.Name;
            Key = ctx.ProviderKey;
        }

        public override string Key { get; }

        public override IReadOnlyList<ConnectorMcpExposure> McpExposures => Array.Empty<ConnectorMcpExposure>();
    }

    private sealed class FakeReloadTrigger : IAgentReloadTrigger
    {
        public int Count { get; private set; }

        public void RequestReload() => Count++;
    }

    private sealed class Harness : IDisposable
    {
        public ServiceProvider Provider { get; }
        public ConnectorRegistry Registry { get; }
        public FakeConfigReader Reader { get; } = new();
        public FakeReloadTrigger Reload { get; } = new();
        public ConnectorManageToolGroup Group { get; }
        public string Workspace { get; }

        public Harness(bool registerDemoProvider = true)
        {
            var services = new ServiceCollection();
            if (registerDemoProvider)
                services.AddConnector<StubConnector>(DemoProviderKey, ctx => new StubConnector(ctx));

            Provider = services.BuildServiceProvider();

            var factories = new ConnectorFactoryRegistry(Provider.GetServices<ConnectorFactoryRegistration>());
            var host = new ConnectorInstanceHost(Provider, factories);
            Registry = new ConnectorRegistry(host, factories, null, Reader, new ConfigurationBuilder().Build());
            Group = new ConnectorManageToolGroup(Registry, factories, new WorkspaceConnectorConfigWriter(), Reload);

            Workspace = Path.Combine(Path.GetTempPath(), "luban-connector-manage-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Workspace);
        }

        public void Dispose()
        {
            Provider.Dispose();
            if (Directory.Exists(Workspace)) Directory.Delete(Workspace, recursive: true);
        }
    }

    [TestMethod]
    public async Task WriteConnectorAsync_RejectsUnregisteredProvider()
    {
        using var h = new Harness(registerDemoProvider: false);

        var result = await h.Group.WriteConnectorAsync("generated", "unknown", null, enabled: true, overwrite: true, h.Workspace);

        Assert.IsFalse(result.IsSuccess);
        StringAssert.Contains(result.Message!, "未注册");
        Assert.AreEqual(0, h.Reload.Count, "失败不应触发重载");
        Assert.IsFalse(Directory.Exists(Path.Combine(h.Workspace, ".luban-agent", "connectors")));
    }

    [TestMethod]
    public async Task WriteConnectorAsync_RejectsPlaintextSecret()
    {
        using var h = new Harness();

        var settings = new Dictionary<string, string> { ["accessToken"] = "pat-literal-value" };
        var result = await h.Group.WriteConnectorAsync("generated", DemoProviderKey, settings, enabled: true, overwrite: true, h.Workspace);

        Assert.IsFalse(result.IsSuccess);
        StringAssert.Contains(result.Message!, "凭据引用");
        Assert.AreEqual(0, h.Reload.Count);
    }

    [TestMethod]
    public async Task WriteConnectorAsync_AcceptsCredentialReference()
    {
        using var h = new Harness();

        var settings = new Dictionary<string, string>
        {
            ["endpoint"] = "https://example.test",
            ["accessToken"] = "$credential:connector:demo:token"
        };

        var result = await h.Group.WriteConnectorAsync("generated", DemoProviderKey, settings, enabled: true, overwrite: true, h.Workspace);

        Assert.IsTrue(result.IsSuccess, result.Message);

        var json = await File.ReadAllTextAsync(Path.Combine(h.Workspace, ".luban-agent", "connectors", "generated.json"));
        StringAssert.Contains(json, "$credential:connector:demo:token");
        Assert.IsFalse(json.Contains("pat-literal-value"), "明文凭据绝不允许落盘");
        Assert.AreEqual(1, h.Reload.Count);
    }

    [TestMethod]
    public async Task WriteConnectorAsync_MakesRegistrySeeEnabledConnector()
    {
        using var h = new Harness();

        var settings = new Dictionary<string, string> { ["endpoint"] = "https://example.test" };
        var result = await h.Group.WriteConnectorAsync("generated", DemoProviderKey, settings, enabled: true, overwrite: true, h.Workspace);
        Assert.IsTrue(result.IsSuccess, result.Message);

        var states = h.Registry.GetStates(h.Workspace);
        var generated = states.Single(s => s.Name == "generated");
        Assert.AreEqual(DemoProviderKey, generated.ProviderKey);
        Assert.AreEqual(ConnectorState.Enabled, generated.State);
    }

    [TestMethod]
    public async Task WriteConnectorAsync_RequiresOverwriteForExisting()
    {
        using var h = new Harness();

        var first = await h.Group.WriteConnectorAsync("generated", DemoProviderKey, null, true, overwrite: true, h.Workspace);
        Assert.IsTrue(first.IsSuccess, first.Message);

        var second = await h.Group.WriteConnectorAsync("generated", DemoProviderKey, null, true, overwrite: false, h.Workspace);
        Assert.IsFalse(second.IsSuccess);
        StringAssert.Contains(second.Message!, "overwrite");
    }

    [TestMethod]
    public async Task DeleteConnectorAsync_RemovesDeclaration()
    {
        using var h = new Harness();

        await h.Group.WriteConnectorAsync("generated", DemoProviderKey, null, true, overwrite: true, h.Workspace);
        var before = h.Registry.GetStates(h.Workspace).SingleOrDefault(s => s.Name == "generated");
        Assert.IsNotNull(before);

        var result = await h.Group.DeleteConnectorAsync("generated", h.Workspace);

        Assert.IsTrue(result.IsSuccess, result.Message);
        Assert.IsFalse(File.Exists(Path.Combine(h.Workspace, ".luban-agent", "connectors", "generated.json")));
        Assert.IsFalse(h.Registry.GetStates(h.Workspace).Any(s => s.Name == "generated"));
        Assert.AreEqual(2, h.Reload.Count);
    }

    [TestMethod]
    public void ListConnectors_ReturnsDeclaredStates()
    {
        using var h = new Harness();
        h.Reader.Connectors.Add(new ConnectorConfig { Name = "primary", Provider = DemoProviderKey, Enabled = true });

        var result = h.Group.ListConnectors(h.Workspace);

        Assert.IsTrue(result.IsSuccess, result.Message);
        StringAssert.Contains(result.Data!, "primary");
        StringAssert.Contains(result.Data!, DemoProviderKey);
    }
}