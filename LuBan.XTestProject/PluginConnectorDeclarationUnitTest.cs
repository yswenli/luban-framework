/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： PluginConnectorDeclarationUnitTest
*创建时间：2026/10/20
*描述：插件「连接器提供方」声明（第 7 权限维度 connectors）单测
*      ① plugin.json 的 connectors 字符串简写 / 对象详写解析；
*      ② permissions.connectors 的数组简写 / 对象详写解析；
*      ③ 声明必须落在 permissions.connectors 之内（否则校验失败）；
*      ④ 具体范围必须落在宿主 ConnectorWhitelist 内；通配以白名单为上限。
*
* 说明：框架已去厂商化，白名单由「示例宿主」TestSecurity 提供。
*
*****************************************************************************/
using System.Text.Json;
using LuBan.AIAgent.Plugins;
using LuBan.AIAgent.Plugins.Loading;

namespace LuBan.XTestProject;

[TestClass]
public class PluginConnectorDeclarationUnitTest
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static ManifestValidator Evaluator() =>
        new(new ManifestValidationOptions(), TestSecurity.Evaluator());

    private static PluginManifest Manifest(params string[] connectorProviderKeys)
    {
        var manifest = new PluginManifest
        {
            Id = "com.tjc.connector-demo",
            Name = "连接器演示",
            Version = "1.0.0",
            EntryAssembly = "Demo.dll"
        };

        manifest.Connectors.AddRange(connectorProviderKeys.Select(key => new PluginConnectorDeclaration { ProviderKey = key }));
        return manifest;
    }

    [TestMethod]
    public void Connectors_SupportStringShorthand_AndObjectForm()
    {
        const string json = """
        {
          "id": "com.tjc.connector-demo",
          "name": "连接器演示",
          "version": "1.0.0",
          "entryAssembly": "Demo.dll",
          "connectors": [ "demo", { "providerKey": "aliyunDevOps", "displayName": "阿里云云效" } ]
        }
        """;

        var manifest = JsonSerializer.Deserialize<PluginManifest>(json, JsonOpts)!;

        Assert.AreEqual(2, manifest.Connectors.Count);
        Assert.AreEqual("demo", manifest.Connectors[0].ProviderKey);
        Assert.IsNull(manifest.Connectors[0].DisplayName);
        Assert.AreEqual("aliyunDevOps", manifest.Connectors[1].ProviderKey);
        Assert.AreEqual("阿里云云效", manifest.Connectors[1].DisplayName);
    }

    [TestMethod]
    public void Permissions_ConnectorsDimension_SupportsBothForms()
    {
        const string arrayJson = """{ "permissions": [ "connectors:demo" ] }""";
        var arrayForm = JsonSerializer.Deserialize<PluginManifest>(arrayJson, JsonOpts)!;
        CollectionAssert.AreEqual(new[] { "demo" }, arrayForm.Permissions.Connectors);

        const string objectJson = """{ "permissions": { "connectors": [ "demo", "aliyunDevOps" ] } }""";
        var objectForm = JsonSerializer.Deserialize<PluginManifest>(objectJson, JsonOpts)!;
        CollectionAssert.AreEqual(new[] { "demo", "aliyunDevOps" }, objectForm.Permissions.Connectors);
    }

    [TestMethod]
    public void UnknownDimension_IncludesConnectorsAsKnown()
    {
        Assert.IsTrue(PluginPermissions.KnownDimensions.Contains("connectors"));
        Assert.AreEqual(7, PluginPermissions.KnownDimensions.Count);

        var permissions = new PluginPermissions { Connectors = { "demo" } };
        Assert.IsTrue(permissions.Declares("connectors"));
        Assert.IsFalse(permissions.IsEmpty);
    }

    [TestMethod]
    public void Declaration_RequiresPermissionCoverage()
    {
        var manifest = Manifest("demo");
        // 未声明 permissions.connectors → 越界

        var result = Evaluator().Validate(manifest);

        Assert.IsFalse(result.IsValid, result.ToSummary());
        Assert.IsTrue(result.Errors.Any(e => e.Contains("connectors")), result.ToSummary());
    }

    [TestMethod]
    public void Declaration_WithinPermission_AndWhitelist_Passes()
    {
        var manifest = Manifest("demo");
        manifest.Permissions.Connectors.Add("demo");

        var result = Evaluator().Validate(manifest);

        Assert.IsTrue(result.IsValid, result.ToSummary());
    }

    [TestMethod]
    public void Declaration_WhitelistScoped_Passes_ButUnknownKeyFails()
    {
        var within = Manifest("aliyunDevOps");
        within.Permissions.Connectors.Add("aliyunDevOps");
        Assert.IsTrue(Evaluator().Validate(within).IsValid);

        // 不在示例宿主 ConnectorWhitelist（demo / aliyunDevOps）内的提供方：声明期越界
        var outside = Manifest("unknownVendor");
        outside.Permissions.Connectors.Add("unknownVendor");
        var result = Evaluator().Validate(outside);

        Assert.IsFalse(result.IsValid, result.ToSummary());
        Assert.IsTrue(result.Errors.Any(e => e.Contains("连接器白名单")), result.ToSummary());
    }

    [TestMethod]
    public void Declaration_MissingProviderKey_AndDuplicates_Reported()
    {
        var manifest = Manifest();
        manifest.Connectors.Add(new PluginConnectorDeclaration());          // 缺 providerKey
        manifest.Connectors.Add(new PluginConnectorDeclaration { ProviderKey = "demo" });
        manifest.Connectors.Add(new PluginConnectorDeclaration { ProviderKey = "demo" }); // 重复
        manifest.Permissions.Connectors.Add("demo");

        var result = Evaluator().Validate(manifest);

        Assert.AreEqual(2, result.Errors.Count, result.ToSummary());
        Assert.IsTrue(result.Errors.Any(e => e.Contains("缺少 providerKey")));
        Assert.IsTrue(result.Errors.Any(e => e.Contains("重复 providerKey")));
    }

    [TestMethod]
    public void WildcardPermission_UsesHostWhitelistAsCeiling()
    {
        var manifest = Manifest("demo");
        manifest.Permissions.Connectors.Add("*");

        var result = Evaluator().Validate(manifest);

        // 通配声明：声明期放行（记警告），运行期以宿主白名单为上限
        Assert.IsTrue(result.IsValid, result.ToSummary());
        Assert.IsTrue(result.Warnings.Any(w => w.Contains("宿主白名单")), result.ToSummary());
    }
}