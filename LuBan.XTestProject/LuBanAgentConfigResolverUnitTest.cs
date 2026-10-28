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
            ("LuBanAgent:Models:0:Provider", "deepseek"),
            ("LuBanAgent:Models:0:Model", model),
            ("LuBanAgent:Models:0:ApiKey", "sk-host"),
            ("LuBanAgent:DefaultModel", defaultModel));

    private const string ValidDbJson = """
        {"Models":[{"Provider":"deepseek","Model":"db-model","ApiKey":"sk-db"}],"DefaultModel":"deepseek:db-model"}
        """;

    [TestMethod]
    public void Resolve_DbValidJson_EnabledAndDbWins()
    {
        var resolution = ResolveWithHost(ValidHost(), _ => ValidDbJson);

        Assert.IsTrue(resolution.Enabled);
        Assert.AreEqual("db", resolution.Source);
        Assert.IsNull(resolution.DisabledReason);
        Assert.IsNotNull(resolution.Configuration);
        Assert.AreEqual("db-model", resolution.Configuration!["LuBanAgent:Models:0:Model"]);
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
            {"Models":[{"Provider":"no-such-provider","Model":"m","ApiKey":"sk","BaseUrl":"https://api.example.com/v1/"}],"DefaultModel":"no-such-provider:m"}
            """;
        var resolution = ResolveWithHost(Host(), _ => dbJson);

        Assert.IsFalse(resolution.Enabled);
        StringAssert.Contains(resolution.DisabledReason, "no-such-provider");
    }

    [TestMethod]
    public void Resolve_BaseUrlNotUrl_Disabled()
    {
        var dbJson = """
            {"Models":[{"Provider":"deepseek","Model":"m","ApiKey":"sk","BaseUrl":"not-a-url"}],"DefaultModel":"deepseek:m"}
            """;
        var resolution = ResolveWithHost(Host(), _ => dbJson);

        Assert.IsFalse(resolution.Enabled);
        StringAssert.Contains(resolution.DisabledReason, "BaseUrl");
    }

    [TestMethod]
    public void Resolve_BaseUrlValid_Enabled()
    {
        var dbJson = """
            {"Models":[{"Provider":"deepseek","Model":"m","ApiKey":"sk","BaseUrl":"https://api.example.com/v1/"}],"DefaultModel":"deepseek:m"}
            """;
        var resolution = ResolveWithHost(Host(), _ => dbJson);

        Assert.IsTrue(resolution.Enabled);
    }

    [TestMethod]
    public void Resolve_WrappedWithSectionName_StillParsed()
    {
        var dbJson = """
            {"LuBanAgent":{"Models":[{"Provider":"deepseek","Model":"m","ApiKey":"sk"}],"DefaultModel":"deepseek:m"}}
            """;
        var resolution = ResolveWithHost(Host(), _ => dbJson);

        Assert.IsTrue(resolution.Enabled, "顶层已含 LuBanAgent 时应原样使用");
        Assert.AreEqual("m", resolution.Configuration!["LuBanAgent:Models:0:Model"]);
    }

    [TestMethod]
    public void Resolve_EmptyModelsArray_Disabled()
    {
        var host = Host(
            ("LuBanAgent:Models", null),
            ("LuBanAgent:DefaultModel", "deepseek:m"));
        var resolution = ResolveWithHost(host, _ => null);

        Assert.IsFalse(resolution.Enabled);
        StringAssert.Contains(resolution.DisabledReason, "Models");
    }

    [TestMethod]
    public void Resolve_LegacyFlatObject_Disabled()
    {
        // 旧版扁平对象写法（Models 直接是 {Provider,Model,ApiKey}）不再支持
        var legacy = Host(
            ("LuBanAgent:Models:Provider", "deepseek"),
            ("LuBanAgent:Models:Model", "m"),
            ("LuBanAgent:Models:ApiKey", "sk"));
        var resolution = ResolveWithHost(legacy, _ => null);

        Assert.IsFalse(resolution.Enabled);
        StringAssert.Contains(resolution.DisabledReason, "Models");
    }

    [TestMethod]
    public void Resolve_MultipleModels_Enabled()
    {
        var dbJson = """
            {"Models":[
              {"Provider":"deepseek","Model":"deepseek-v4-pro","ApiKey":"sk-db"},
              {"Provider":"kimi","Model":"kimi-k3","ApiKey":"sk-kimi"}
            ],"DefaultModel":"kimi:kimi-k3"}
            """;
        var resolution = ResolveWithHost(Host(), _ => dbJson);

        Assert.IsTrue(resolution.Enabled);
        Assert.AreEqual("kimi-k3", resolution.Configuration!["LuBanAgent:Models:1:Model"]);
    }

    [TestMethod]
    public void Resolve_DuplicateProviderSameCredentials_Enabled()
    {
        var dbJson = """
            {"Models":[
              {"Provider":"deepseek","Model":"deepseek-v4-pro","ApiKey":"sk-db","BaseUrl":"https://api.deepseek.com/v1/"},
              {"Provider":"DeepSeek","Model":"deepseek-v4-flash","ApiKey":"sk-db","BaseUrl":"https://api.deepseek.com/v1"}
            ]}
            """;
        var resolution = ResolveWithHost(Host(), _ => dbJson);

        Assert.IsTrue(resolution.Enabled, $"同一 Provider 凭据一致应允许，实际原因：{resolution.DisabledReason}");
    }

    [TestMethod]
    public void Resolve_DuplicateProviderDifferentApiKey_Disabled()
    {
        var dbJson = """
            {"Models":[
              {"Provider":"deepseek","Model":"a","ApiKey":"sk-1"},
              {"Provider":"deepseek","Model":"b","ApiKey":"sk-2"}
            ]}
            """;
        var resolution = ResolveWithHost(Host(), _ => dbJson);

        Assert.IsFalse(resolution.Enabled);
        StringAssert.Contains(resolution.DisabledReason, "ApiKey/BaseUrl 不一致");
    }

    [TestMethod]
    public void Resolve_DefaultModelNotInModels_Disabled()
    {
        var dbJson = """
            {"Models":[{"Provider":"deepseek","Model":"m","ApiKey":"sk"}],"DefaultModel":"deepseek:not-configured"}
            """;
        var resolution = ResolveWithHost(Host(), _ => dbJson);

        Assert.IsFalse(resolution.Enabled);
        StringAssert.Contains(resolution.DisabledReason, "未在 Models 数组中找到");
    }

    [TestMethod]
    public void Resolve_DefaultModelBadFormat_Disabled()
    {
        var dbJson = """
            {"Models":[{"Provider":"deepseek","Model":"m","ApiKey":"sk"}],"DefaultModel":"no-colon"}
            """;
        var resolution = ResolveWithHost(Host(), _ => dbJson);

        Assert.IsFalse(resolution.Enabled);
        StringAssert.Contains(resolution.DisabledReason, "provider:model");
    }

    [TestMethod]
    public void Resolve_MissingDefaultModel_EnabledAndFallsBackToFirst()
    {
        var host = ValidHost(defaultModel: null);
        var resolution = ResolveWithHost(host, _ => null);

        Assert.IsTrue(resolution.Enabled, "DefaultModel 可省略");
        Assert.IsNull(resolution.Configuration!["LuBanAgent:DefaultModel"]);

        var effective = LuBanAgentConfigResolver.CreateEffectiveConfiguration(resolution.Configuration!);
        Assert.AreEqual("deepseek:host-model", effective["LuBanAgent:DefaultModel"]);
    }

    [TestMethod]
    public void Resolve_DbOverride_OnlyTouchesAgentSection()
    {
        var host = Host(
            ("LuBanAgent:WorkspaceRoot", @"C:\work\demo"),
            ("DbConnectionOptions:EnableConsoleSql", "false"),
            ("LuBanAgent:Models:0:Provider", "deepseek"),
            ("LuBanAgent:Models:0:Model", "host-model"),
            ("LuBanAgent:Models:0:ApiKey", "sk-host"),
            ("LuBanAgent:DefaultModel", "deepseek:host-model"));

        var resolution = ResolveWithHost(host, _ => ValidDbJson);

        Assert.IsTrue(resolution.Enabled);
        // db 未给的键保留宿主值（异键互补）
        Assert.AreEqual(@"C:\work\demo", resolution.Configuration!["LuBanAgent:WorkspaceRoot"]);
        // 其他节不受影响
        Assert.AreEqual("false", resolution.Configuration!["DbConnectionOptions:EnableConsoleSql"]);
        Assert.AreEqual("db-model", resolution.Configuration!["LuBanAgent:Models:0:Model"]);
    }

    [TestMethod]
    public void Resolve_BlankDbValue_TreatedAsNoRecord()
    {
        // db 读到空白串等同「没有记录」，走宿主回退
        var resolution = ResolveWithHost(ValidHost(), _ => "   ");

        Assert.IsTrue(resolution.Enabled);
        Assert.AreEqual("host", resolution.Source);
    }

    [TestMethod]
    public void ResolveWorkspaceRoot_AbsolutePath_KeptAbsolute()
    {
        var resolved = LuBanAgentConfigResolver.ResolveWorkspaceRoot(@"C:\work\agent", @"C:\site");

        Assert.AreEqual(Path.GetFullPath(@"C:\work\agent"), resolved);
    }

    [TestMethod]
    public void ResolveWorkspaceRoot_RelativePath_UsesContentRoot()
    {
        var resolved = LuBanAgentConfigResolver.ResolveWorkspaceRoot(".", @"C:\site");

        Assert.AreEqual(Path.GetFullPath(@"C:\site"), resolved);
    }

    [TestMethod]
    public void ResolveWorkspaceRoot_RelativeChild_UsesContentRoot()
    {
        var resolved = LuBanAgentConfigResolver.ResolveWorkspaceRoot(Path.Combine("workspace", "demo"), @"C:\site");

        Assert.AreEqual(Path.GetFullPath(Path.Combine(@"C:\site", "workspace", "demo")), resolved);
    }

    [TestMethod]
    public void ResolveWorkspaceRoot_NoContentRoot_FallsBackToProgramDirectory()
    {
        var resolved = LuBanAgentConfigResolver.ResolveWorkspaceRoot("workspace");

        Assert.AreEqual(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "workspace")), resolved);
    }

    [TestMethod]
    public void CreateEffectiveConfiguration_RelativeRootAndMissingDefaultModel_Overridden()
    {
        var host = Host(
            ("LuBanAgent:WorkspaceRoot", "."),
            ("LuBanAgent:Models:0:Provider", "deepseek"),
            ("LuBanAgent:Models:0:Model", "m"),
            ("LuBanAgent:Models:0:ApiKey", "sk"),
            ("Other:Key", "keep"));

        var effective = LuBanAgentConfigResolver.CreateEffectiveConfiguration(host, @"C:\site");

        Assert.AreNotSame(host, effective);
        Assert.AreEqual(Path.GetFullPath(@"C:\site"), effective["LuBanAgent:WorkspaceRoot"]);
        Assert.AreEqual("deepseek:m", effective["LuBanAgent:DefaultModel"]);
        Assert.AreEqual("keep", effective["Other:Key"]);
    }

    [TestMethod]
    public void CreateEffectiveConfiguration_AlreadyAbsoluteAndDefaultConfigured_ReturnsSameInstance()
    {
        var host = Host(
            ("LuBanAgent:WorkspaceRoot", @"C:\work\agent"),
            ("LuBanAgent:DefaultModel", "deepseek:m"));

        var effective = LuBanAgentConfigResolver.CreateEffectiveConfiguration(host, @"C:\site");

        Assert.AreSame(host, effective);
    }
}