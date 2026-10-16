/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： CredentialStoreUnitTest
*描述：验证凭据键归一化与默认凭据存储（PRD §3.7 / §4.9）
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
/// PRD 中 <c>codeup:pat</c> 与 <c>yunxiao_pat</c> 为同一凭据，须归一化到同一键，
/// 否则「声明期」与「取用期」会对不上。
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

        return new ConfigurationCredentialStore(config);
    }

    // ---------------- 键归一化 ----------------

    [TestMethod]
    public void Normalize_AliasesMapToCanonicalKeys()
    {
        foreach (var alias in new[] { "yunxiao_pat", "yunxiao:pat", "codeup_pat", "codeup:pat", "YUNXIAO_PAT" })
            Assert.AreEqual(CredentialKeys.YunxiaoPat, CredentialKeys.Normalize(alias));

        foreach (var alias in new[] { "yunxiao_org_id", "yunxiao:org", "codeup_org_id", "codeup:org" })
            Assert.AreEqual(CredentialKeys.YunxiaoOrganizationId, CredentialKeys.Normalize(alias));
    }

    [TestMethod]
    public void Normalize_UnknownKey_IsLowercasedAndTrimmed()
    {
        Assert.AreEqual("custom_key", CredentialKeys.Normalize("  Custom_Key "));
    }

    [TestMethod]
    public void GetEnvironmentVariableName_MapsKnownKeysOnly()
    {
        Assert.AreEqual("LUBAN_YUNXIAO_PAT", CredentialKeys.GetEnvironmentVariableName("codeup:pat"));
        Assert.AreEqual("LUBAN_YUNXIAO_ORGANIZATION_ID", CredentialKeys.GetEnvironmentVariableName("yunxiao_org_id"));
        Assert.IsNull(CredentialKeys.GetEnvironmentVariableName("custom_key"));
    }

    // ---------------- 默认存储：读 ----------------

    [TestMethod]
    public async Task GetAsync_ReadsFromConfiguration()
    {
        var store = Store(($"{SectionName}:{CredentialKeys.YunxiaoPat}", "pat-value"));

        Assert.AreEqual("pat-value", await store.GetAsync(CredentialKeys.YunxiaoPat));
    }

    [TestMethod]
    public async Task GetAsync_AliasKey_HitsCanonicalEntry()
    {
        var store = Store(($"{SectionName}:{CredentialKeys.YunxiaoPat}", "pat-value"));

        Assert.AreEqual("pat-value", await store.GetAsync("codeup:pat"));
        Assert.AreEqual("pat-value", await store.GetAsync("YUNXIAO:PAT"));
    }

    [TestMethod]
    public async Task GetAsync_Unconfigured_ReturnsNull()
    {
        var store = Store();

        // 已知键可能被环境变量兜底（LUBAN_YUNXIAO_PAT），断言成「无配置时的期望值」而非硬编码 null
        var expected = Environment.GetEnvironmentVariable("LUBAN_YUNXIAO_PAT");
        Assert.AreEqual(string.IsNullOrWhiteSpace(expected) ? null : expected,
            await store.GetAsync("codeup:pat"));

        Assert.IsNull(await store.GetAsync("some_unknown_key"));
    }

    [TestMethod]
    public async Task GetAsync_BlankValue_TreatedAsMissing()
    {
        // 已知键可能被环境变量兜底；用未知键验证「空白即缺失」
        var store = Store(("LuBanAgent:Credentials:custom_key", "   "));

        Assert.IsNull(await store.GetAsync("custom_key"));
        Assert.IsFalse(store.Contains("custom_key"));
    }

    [TestMethod]
    public async Task Contains_ReflectsConfiguration()
    {
        var store = Store(($"{SectionName}:{CredentialKeys.YunxiaoPat}", "pat-value"));

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
            () => store.SetAsync(CredentialKeys.YunxiaoPat, "x"));
    }

    [TestMethod]
    public void Constructor_NullConfiguration_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new ConfigurationCredentialStore(null!));
    }

    [TestMethod]
    public void Keys_ListConfiguredKeys_Normalized()
    {
        var store = Store(
            ($"{SectionName}:{CredentialKeys.YunxiaoPat}", "pat-value"),
            ($"{SectionName}:{CredentialKeys.YunxiaoOrganizationId}", "org-1"));

        var keys = store.Keys.ToArray();

        CollectionAssert.Contains(keys, CredentialKeys.YunxiaoPat);
        CollectionAssert.Contains(keys, CredentialKeys.YunxiaoOrganizationId);
    }
}