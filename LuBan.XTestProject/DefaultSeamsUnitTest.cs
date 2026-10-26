using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LuBan.XTestProject;

[TestClass]
public class DefaultSeamsUnitTest
{
    private static IOptions<LuBanAgentOptions> Options(string? root = null, string? defaultModel = null)
        => Microsoft.Extensions.Options.Options.Create(new LuBanAgentOptions
        {
            WorkspaceRoot = root,
            DefaultModel = defaultModel
        });

    [TestMethod]
    public async Task SingleWorkspaceResolver_ReturnsConfiguredRoot()
    {
        var resolver = new SingleWorkspaceResolver(Options(@"C:\work\demo"));

        var ws = await resolver.ResolveAsync("s1");

        Assert.AreEqual(@"C:\work\demo", ws.RootPath);
        Assert.AreEqual("default", ws.Id);
        Assert.AreEqual("Default", ws.Name);
        Assert.AreEqual("normal", ws.Type);
    }

    [TestMethod]
    public void DefaultAgentProfileResolver_AlwaysReturnsDefaultProfile()
    {
        var resolver = new DefaultAgentProfileResolver(new DefaultAgentProfile());

        var profile = resolver.Resolve(new WorkspaceDescriptor("w", "n", @"C:\w", "normal"));

        Assert.IsInstanceOfType(profile, typeof(DefaultAgentProfile));
    }

    [TestMethod]
    public void DefaultAgentUserResolver_FallsBackToLocal()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();
        var resolver = new DefaultAgentUserResolver();

        Assert.AreEqual("local", resolver.ResolveUserId(provider));
    }

    [TestMethod]
    public void DefaultAgentModelSelection_EmptyWhenNotConfigured()
    {
        var selection = new DefaultAgentModelSelection(Options());

        Assert.AreEqual(string.Empty, selection.QualifiedModelName);
        Assert.IsNull(selection.ProviderId);
        Assert.IsNull(selection.ModelId);
    }

    [TestMethod]
    public void DefaultAgentModelSelection_ParsesConfiguredDefault()
    {
        var selection = new DefaultAgentModelSelection(Options(defaultModel: "openai:gpt-4.1"));

        Assert.AreEqual("openai", selection.ProviderId);
        Assert.AreEqual("gpt-4.1", selection.ModelId);
        Assert.AreEqual("openai:gpt-4.1", selection.QualifiedModelName);
    }
}