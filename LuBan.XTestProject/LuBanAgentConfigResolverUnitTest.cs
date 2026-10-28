using LuBan.Common;
using LuBan.Web.Core.AIAgent;

using Microsoft.Extensions.Configuration;

using System.Reflection;

namespace LuBan.XTestProject;

[TestClass]
public class LuBanAgentConfigResolverUnitTest
{
    /// <summary>造一个宿主配置。</summary>
    private static IConfiguration Host(params (string Key, string? Value)[] pairs)
        => new ConfigurationBuilder()
            .AddInMemoryCollection(pairs.Select(p => new KeyValuePair<string, string?>(p.Key, p.Value)))
            .Build();

    /// <summary>
    /// 按方案 B 调解析器：宿主路径走静态 ConfigUtil，必须先把宿主配置挂上去。
    /// </summary>
    private static LuBanAgentResolution ResolveWithHost(IConfiguration host, Func<string, string?> dbReader)
    {
        ConfigUtil.InitConfigUtil(host);
        try
        {
            return LuBanAgentConfigResolver.Resolve(host, dbReader);
        }
        finally
        {
            // ConfigUtil 没有公开的重置入口，置回 null 恢复「未初始化」状态，避免污染其他用例
            typeof(ConfigUtil)
                .GetField("_configuration", BindingFlags.NonPublic | BindingFlags.Static)
                ?.SetValue(null, null);
        }
    }

    /// <summary>合法宿主配置（可覆盖个别键）。</summary>
    private static IConfiguration ValidHost(string model = "host-model", string? defaultModel = "deepseek:host-model")
        => Host(
            ("LuBanAgent:Models:Provider", "deepseek"),
            ("LuBanAgent:Models:Model", model),
            ("LuBanAgent:Models:ApiKey", "sk-host"),
            ("LuBanAgent:DefaultModel", defaultModel));

    private const string ValidDbJson = """
        {"Models":{"Provider":"deepseek","Model":"db-model","ApiKey":"sk-db"},"DefaultModel":"deepseek:db-model"}
        """;

    [TestMethod]
    public void Resolve_DbValidJson_EnabledAndDbWins()
    {
        var resolution = ResolveWithHost(ValidHost(), _ => ValidDbJson);

        Assert.IsTrue(resolution.Enabled);
        Assert.AreEqual("db", resolution.Source);
        Assert.IsNull(resolution.DisabledReason);
        Assert.IsNotNull(resolution.Configuration);
        Assert.AreEqual("db-model", resolution.Configuration!["LuBanAgent:Models:Model"]);
        Assert.AreEqual("deepseek:db-model", resolution.Configuration!["LuBanAgent:DefaultModel"]);
    }

    [TestMethod]
    public void Resolve_DbInvalidJson_DisabledAndNoFallback()
    {
        var resolution = ResolveWithHost(ValidHost(), _ => "{not json");

        Assert.IsFalse(resolution.Enabled, "db 记录坏掉时不得回退到宿主配置");
        Assert.AreEqual("db", resolution.Source);
        StringAssert.Contains(resolution.DisabledReason, "JSON");
    }

    [TestMethod]
    public void Resolve_NoDbRecord_HostValid_EnabledFromHost()
    {
        var host = ValidHost();
        var resolution = ResolveWithHost(host, _ => null);

        Assert.IsTrue(resolution.Enabled);
        Assert.AreEqual("host", resolution.Source);
        Assert.AreSame(host, resolution.Configuration);
        Assert.IsNull(resolution.DisabledReason);
    }

    [TestMethod]
    public void Resolve_NoDbRecord_NoHostSection_Disabled()
    {
        var resolution = ResolveWithHost(Host(), _ => null);

        Assert.IsFalse(resolution.Enabled);
        Assert.AreEqual("host", resolution.Source);
        StringAssert.Contains(resolution.DisabledReason, "未找到");
    }

    [TestMethod]
    public void Resolve_DbReaderThrows_TreatedAsNoRecord()
    {
        var resolution = ResolveWithHost(ValidHost(), _ => throw new InvalidOperationException("db down"));

        Assert.IsTrue(resolution.Enabled);
        Assert.AreEqual("host", resolution.Source);
    }

    [TestMethod]
    public void Resolve_UnknownProvider_DisabledEvenWithValidBaseUrl()
    {
        var dbJson = """
            {"Models":{"Provider":"no-such-provider","Model":"m","ApiKey":"sk","BaseUrl":"https://api.example.com/v1/"},"DefaultModel":"x:y"}
            """;
        var resolution = ResolveWithHost(Host(), _ => dbJson);

        Assert.IsFalse(resolution.Enabled);
        StringAssert.Contains(resolution.DisabledReason, "no-such-provider");
    }

    [TestMethod]
    public void Resolve_BaseUrlNotUrl_Disabled()
    {
        var dbJson = """
            {"Models":{"Provider":"deepseek","Model":"m","ApiKey":"sk","BaseUrl":"not-a-url"},"DefaultModel":"deepseek:m"}
            """;
        var resolution = ResolveWithHost(Host(), _ => dbJson);

        Assert.IsFalse(resolution.Enabled);
        StringAssert.Contains(resolution.DisabledReason, "BaseUrl");
    }

    [TestMethod]
    public void Resolve_BaseUrlValid_Enabled()
    {
        var dbJson = """
            {"Models":{"Provider":"deepseek","Model":"m","ApiKey":"sk","BaseUrl":"https://api.example.com/v1/"},"DefaultModel":"deepseek:m"}
            """;
        var resolution = ResolveWithHost(Host(), _ => dbJson);

        Assert.IsTrue(resolution.Enabled);
    }

    [TestMethod]
    public void Resolve_WrappedWithSectionName_StillParsed()
    {
        var dbJson = """
            {"LuBanAgent":{"Models":{"Provider":"deepseek","Model":"m","ApiKey":"sk"},"DefaultModel":"deepseek:m"}}
            """;
        var resolution = ResolveWithHost(Host(), _ => dbJson);

        Assert.IsTrue(resolution.Enabled, "顶层已含 LuBanAgent 时应原样使用");
        Assert.AreEqual("m", resolution.Configuration!["LuBanAgent:Models:Model"]);
    }

    [TestMethod]
    public void Resolve_MissingDefaultModel_Disabled()
    {
        var host = ValidHost(defaultModel: null);
        var resolution = ResolveWithHost(host, _ => null);

        Assert.IsFalse(resolution.Enabled);
        StringAssert.Contains(resolution.DisabledReason, "DefaultModel");
    }

    [TestMethod]
    public void Resolve_DbOverride_OnlyTouchesAgentSection()
    {
        var host = Host(
            ("LuBanAgent:WorkspaceRoot", @"C:\work\demo"),
            ("DbConnectionOptions:EnableConsoleSql", "false"),
            ("LuBanAgent:Models:Provider", "deepseek"),
            ("LuBanAgent:Models:Model", "host-model"),
            ("LuBanAgent:Models:ApiKey", "sk-host"),
            ("LuBanAgent:DefaultModel", "deepseek:host-model"));

        var resolution = ResolveWithHost(host, _ => ValidDbJson);

        Assert.IsTrue(resolution.Enabled);
        // db 未给的键保留宿主值（异键互补）
        Assert.AreEqual(@"C:\work\demo", resolution.Configuration!["LuBanAgent:WorkspaceRoot"]);
        // 其他节不受影响
        Assert.AreEqual("false", resolution.Configuration!["DbConnectionOptions:EnableConsoleSql"]);
        Assert.AreEqual("db-model", resolution.Configuration!["LuBanAgent:Models:Model"]);
    }

    [TestMethod]
    public void Resolve_BlankDbValue_TreatedAsNoRecord()
    {
        // db 读到空白串等同「没有记录」，走宿主回退
        var resolution = ResolveWithHost(ValidHost(), _ => "   ");

        Assert.IsTrue(resolution.Enabled);
        Assert.AreEqual("host", resolution.Source);
    }
}