/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： PluginPermissionsUnitTest
*描述：验证 PluginPermissions 的数组式/对象式解析、通配回退、去重规范化与序列化往返
*
*****************************************************************************/
using System.Text.Json;
using LuBan.AIAgent.Plugins;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

/// <summary>
/// 插件权限声明（PRD §3.7）解析与规范化测试。
/// </summary>
[TestClass]
public class PluginPermissionsUnitTest
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// 把一段 permissions JSON 片段塞进合法清单后反序列化（走真实转换器路径）。
    /// </summary>
    private static PluginManifest Parse(string permissionsJson)
    {
        var json = """
        {
          "id": "com.tjc.demo",
          "name": "Demo",
          "version": "1.0.0",
          "entryAssembly": "Demo.dll",
          "permissions": __PERMISSIONS__
        }
        """.Replace("__PERMISSIONS__", permissionsJson);

        return JsonSerializer.Deserialize<PluginManifest>(json, JsonOpts)
               ?? throw new AssertFailedException("清单反序列化失败");
    }

    [TestMethod]
    public void ArrayForm_DimensionWithoutScope_UsesWildcard()
    {
        var manifest = Parse("""[ "network" ]""");

        Assert.IsTrue(manifest.Permissions.Declares("network"));
        CollectionAssert.AreEqual(new[] { "*" }, manifest.Permissions.GetScopes("network").ToArray());
    }

    [TestMethod]
    public void ArrayForm_DimensionWithScope_KeepsScope()
    {
        var manifest = Parse("""[ "filesystem:workspace", "git:repo" ]""");

        CollectionAssert.AreEqual(new[] { "workspace" }, manifest.Permissions.GetScopes("filesystem").ToArray());
        CollectionAssert.AreEqual(new[] { "repo" }, manifest.Permissions.GetScopes("git").ToArray());
    }

    [TestMethod]
    public void ArrayForm_EmptyScopeAfterColon_UsesWildcard()
    {
        var manifest = Parse("""[ "process:" ]""");

        CollectionAssert.AreEqual(new[] { "*" }, manifest.Permissions.GetScopes("process").ToArray());
    }

    [TestMethod]
    public void ObjectForm_ParsesAllDimensions()
    {
        var manifest = Parse("""{ "filesystem": [ "workspace" ], "network": [ "*" ], "database": [ "read" ] }""");

        CollectionAssert.AreEqual(new[] { "workspace" }, manifest.Permissions.GetScopes("filesystem").ToArray());
        CollectionAssert.AreEqual(new[] { "*" }, manifest.Permissions.GetScopes("network").ToArray());
        CollectionAssert.AreEqual(new[] { "read" }, manifest.Permissions.GetScopes("database").ToArray());
    }

    [TestMethod]
    public void Null_BecomesEmptyPermissions()
    {
        var manifest = Parse("null");

        Assert.IsTrue(manifest.Permissions.IsEmpty);
        Assert.AreEqual(0, manifest.Permissions.DeclaredDimensions.Count());
    }

    [TestMethod]
    public void MissingField_BecomesEmptyPermissions()
    {
        var json = """
        { "id": "com.tjc.demo", "name": "Demo", "version": "1.0.0", "entryAssembly": "Demo.dll" }
        """;

        var manifest = JsonSerializer.Deserialize<PluginManifest>(json, JsonOpts)!;

        Assert.IsTrue(manifest.Permissions.IsEmpty);
    }

    [TestMethod]
    public void ArrayForm_UnknownDimension_ThrowsJsonException()
    {
        try
        {
            Parse("""[ "filesystem", "teleport" ]""");
            Assert.Fail("未知权限维度应该抛出 JsonException");
        }
        catch (JsonException)
        {
            // 预期：未知维度必须显式失败，避免拼写错误被静默忽略
        }
    }

    [TestMethod]
    public void ArrayForm_NonStringElement_ThrowsJsonException()
    {
        try
        {
            Parse("""[ "filesystem", 42 ]""");
            Assert.Fail("非字符串权限项应该抛出 JsonException");
        }
        catch (JsonException)
        {
            // 预期
        }
    }

    [TestMethod]
    public void Normalize_TrimsAndDedupesIgnoringCase()
    {
        var permissions = new PluginPermissions
        {
            Filesystem = new List<string> { "  workspace ", "workspace", "WORKSPACE", "", "repo" }
        };

        permissions.Normalize();

        CollectionAssert.AreEqual(new[] { "workspace", "repo" }, permissions.Filesystem.ToArray());
    }

    [TestMethod]
    public void GetScopes_IsCaseInsensitive_AndUnknownReturnsEmpty()
    {
        var permissions = new PluginPermissions { Network = new List<string> { "*" } };

        Assert.IsTrue(permissions.Declares("NETWORK"));
        Assert.AreEqual(1, permissions.GetScopes("  Network ").Count);
        Assert.AreEqual(0, permissions.GetScopes("teleport").Count);
        Assert.IsFalse(permissions.Declares("teleport"));
    }

    [TestMethod]
    public void DeclaredDimensions_FollowsCanonicalOrder()
    {
        var permissions = new PluginPermissions
        {
            Credential = new List<string> { "*" },
            Filesystem = new List<string> { "workspace" },
            Git = new List<string> { "repo" }
        };

        CollectionAssert.AreEqual(
            new[] { "filesystem", "git", "credential" },
            permissions.DeclaredDimensions.ToArray());
    }

    [TestMethod]
    public void Write_RoundTripsThroughManifest()
    {
        var original = new PluginManifest
        {
            Id = "com.tjc.demo",
            Name = "Demo",
            Version = "1.0.0",
            EntryAssembly = "Demo.dll",
            Permissions = new PluginPermissions
            {
                Filesystem = new List<string> { "workspace" },
                Network = new List<string> { "*" }
            }
        };

        var json = JsonSerializer.Serialize(original, JsonOpts);
        var restored = JsonSerializer.Deserialize<PluginManifest>(json, JsonOpts)!;

        // 写出为对象详写形式，且空维度不落盘
        Assert.IsTrue(json.Contains("\"filesystem\""));
        Assert.IsFalse(json.Contains("\"database\""));
        CollectionAssert.AreEqual(new[] { "workspace" }, restored.Permissions.GetScopes("filesystem").ToArray());
        Assert.IsTrue(restored.Permissions.Declares("network"));
    }

    [TestMethod]
    public void KnownDimensions_MatchesPrdSixDimensions()
    {
        CollectionAssert.AreEqual(
            new[] { "filesystem", "network", "git", "database", "process", "credential" },
            PluginPermissions.KnownDimensions.ToArray());
    }
}