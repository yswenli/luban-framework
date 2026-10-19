/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： CredentialStoreUnitTest
*描述：验证凭据键归一化、宿主别名回退与默认凭据存储（PRD §3.7 / §4.9）
*
*****************************************************************************/
using LuBan.AIAgent.Plugins.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

/// <summary>
/// 凭据存储与键归一化测试（PRD §3.7 / §4.9）。
/// </summary>
/// <remarks>
/// 框架只认通用规范拼写 <c>connector:&lt;实例名&gt;:&lt;字段&gt;</c>；厂商历史拼写
/// （如 <c>codeup:pat</c> 与 <c>yunxiao_pat</c>）的等价关系由宿主实现
/// <see cref="ICredentialKeyAliases"/> 提供——本测试用 <see cref="DemoCredentialKeyAliases"/> 验证该机制。
/// </remarks>
[TestClass]
public class CredentialStoreUnitTest
{
    private const string SectionName = "LuBanAgent:Credentials";

    private static ConfigurationCredentialStore Store(params (string Key, string Value)[] items)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(items.ToDictionary(x => x.Key, x => (string?)x.Value))
            .Build();

        return new ConfigurationCredentialStore(config, TestSecurity.Aliases);
    }

    // ---------------- 键归一化（框架：通用规则） ----------------

    [TestMethod]
    public void Normalize_UnknownKey_IsLowercasedAndTrimmed()
    {
        Assert.AreEqual("custom_key", CredentialKeys.Normalize("  Custom_Key "));
        Assert.AreEqual("connector:my-inst:token", CredentialKeys.Normalize("  Connector:My-Inst:Token "));
    }

    [TestMethod]
    public void ForConnectorKey_BuildsCanonicalSpelling()
    {
        Assert.AreEqual("connector:demo:token", CredentialKeys.ForConnectorToken("demo"));
        Assert.AreEqual("connector:demo:organizationid", CredentialKeys.ForConnectorField("Demo", " OrganizationId "));
    }

    [TestMethod]
    public void TryParseConnectorKey_SplitsInstanceAndField()
    {
        Assert.IsTrue(CredentialKeys.TryParseConnectorKey("Connector:Demo:Token", out var instance, out var field));
        Assert.AreEqual("demo", instance);
        Assert.AreEqual("token", field);

        Assert.IsFalse(CredentialKeys.TryParseConnectorKey("connector:token", out _, out _));
        Assert.IsFalse(CredentialKeys.TryParseConnectorKey("custom_key", out _, out _));
    }

    // ---------------- 键归一化（宿主：别名） ----------------

    [TestMethod]
    public void Aliases_MapLegacySpellingsToCanonicalKeys()
    {
        var aliases = TestSecurity.Aliases;

        foreach (var alias in new[] { "yunxiao_pat", "yunxiao:pat", "codeup_pat", "codeup:pat", "YUNXIAO_PAT" })
            Assert.AreEqual(TestSecurity.DemoTokenKey, aliases.Resolve(alias));

        foreach (var alias in new[] { "yunxiao_org_id", "yunxiao:org", "codeup_org_id", "codeup:org" })
            Assert.AreEqual(TestSecurity.DemoOrgKey, aliases.Resolve(alias));

        // 无别名的写法只做通用归一化
        Assert.AreEqual("custom_key", aliases.Resolve("  Custom_Key "));
    }

    [TestMethod]
    public void GetEnvironmentVariableName_MapsGenerically()
    {
        // 通用约定：connector:<实例名>:<字段> → LUBAN_CONNECTOR_<实例名>_<字段>
        Assert.AreEqual("LUBAN_CONNECTOR_DEMO_TOKEN", CredentialKeys.GetEnvironmentVariableName("connector:demo:token"));
        Assert.AreEqual("LUBAN_CONNECTOR_MY_INST_V2_TOKEN", CredentialKeys.GetEnvironmentVariableName("connector:my-inst.v2:token"));

        // 非连接器键 → LUBAN_<键>（非字母数字转下划线、大写）
        Assert.AreEqual("LUBAN_CUSTOM_KEY", CredentialKeys.GetEnvironmentVariableName("custom_key"));
        Assert.AreEqual("LUBAN_YUNXIAO_ORGANIZATION_ID", CredentialKeys.GetEnvironmentVariableName("yunxiao_organization_id"));
    }

    // ---------------- 默认存储：读 ----------------

    [TestMethod]
    public async Task GetAsync_ReadsFromConfiguration()
    {
        var store = Store(($"{SectionName}:{TestSecurity.DemoTokenKey}", "pat-value"));

        Assert.AreEqual("pat-value", await store.GetAsync(TestSecurity.DemoTokenKey));
    }

    [TestMethod]
    public async Task GetAsync_AliasKey_HitsCanonicalEntry()
    {
        var store = Store(($"{SectionName}:{TestSecurity.DemoTokenKey}", "pat-value"));

        Assert.AreEqual("pat-value", await store.GetAsync("codeup:pat"));
        Assert.AreEqual("pat-value", await store.GetAsync("YUNXIAO:PAT"));
    }

    [TestMethod]
    public async Task GetAsync_LegacyConfiguredKey_FallsBackToAlias()
    {
        // 旧配置仍用历史拼写：宿主别名保证无需人工迁移
        var store = Store(($"{SectionName}:yunxiao_pat", "legacy-value"));

        Assert.AreEqual("legacy-value", await store.GetAsync(TestSecurity.DemoTokenKey));
        Assert.AreEqual("legacy-value", await store.GetAsync("codeup:pat"));
    }

    [TestMethod]
    public async Task GetAsync_Unconfigured_ReturnsNull()
    {
        var store = Store();

        Assert.IsNull(await store.GetAsync("some_unknown_key"));
    }

    [TestMethod]
    public async Task GetAsync_BlankValue_TreatedAsMissing()
    {
        var store = Store(("LuBanAgent:Credentials:custom_key", "   "));

        Assert.IsNull(await store.GetAsync("custom_key"));
        Assert.IsFalse(store.Contains("custom_key"));
    }

    [TestMethod]
    public async Task Contains_ReflectsConfiguration()
    {
        var store = Store(($"{SectionName}:{TestSecurity.DemoTokenKey}", "pat-value"));

        Assert.IsTrue(store.Contains("codeup:pat"));
        Assert.IsFalse(store.Contains("missing_key"));
        await Task.CompletedTask;
    }

    // ---------------- 默认存储：只读 ----------------

    [TestMethod]
    public async Task SetAsync_DefaultImplementation_IsReadOnly()
    {
        var store = Store();

        await Assert.ThrowsExactlyAsync<NotSupportedException>(
            () => store.SetAsync(TestSecurity.DemoTokenKey, "x"));
    }

    [TestMethod]
    public void Constructor_NullConfiguration_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new ConfigurationCredentialStore(null!));
    }

    [TestMethod]
    public void Keys_ListConfiguredKeys_NormalizedToCanonical()
    {
        var store = Store(
            ($"{SectionName}:{TestSecurity.DemoTokenKey}", "pat-value"),
            ($"{SectionName}:yunxiao_org_id", "org-1"));

        var keys = store.Keys.ToArray();

        CollectionAssert.Contains(keys, TestSecurity.DemoTokenKey);
        CollectionAssert.Contains(keys, TestSecurity.DemoOrgKey);
    }
}