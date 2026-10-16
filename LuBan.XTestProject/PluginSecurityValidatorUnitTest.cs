/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： PluginSecurityValidatorUnitTest
*描述：验证清单校验器接入权限白名单（PRD §3.7）：越界声明导致校验不通过
*
*****************************************************************************/
using LuBan.AIAgent.Plugins;
using LuBan.AIAgent.Plugins.Loading;
using LuBan.AIAgent.Plugins.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

/// <summary>
/// 清单校验器 × 权限白名单集成测试（PRD §3.7）。
/// </summary>
/// <remarks>
/// 不合规的权限声明（具体范围越界）必须计入 <see cref="ManifestValidationResult.Errors"/>，使插件在 <c>EnableAsync</c>
/// 阶段被直接判 <c>Disabled</c>（而不是加载后才在运行期拦截）。
/// 通配 <c>*</c> 声明不算越界：计入 <see cref="ManifestValidationResult.Warnings"/>，运行期以宿主白名单为上限。
/// </remarks>
[TestClass]
public class PluginSecurityValidatorUnitTest
{
    private static PluginManifest Manifest(PluginPermissions? permissions, List<SkillDefinition>? skills = null) => new()
    {
        Id = "com.demo.security",
        Name = "Security Demo",
        Version = "1.0.0",
        EntryAssembly = "Demo.dll",
        Permissions = permissions ?? new PluginPermissions(),
        Skills = skills ?? new List<SkillDefinition>(),
    };

    private static ManifestValidator Validator(PluginSecurityOptions? options = null)
        => new(new ManifestValidationOptions(), new PluginPermissionEvaluator(options));

    [TestMethod]
    public void Valid_Manifest_WithCompliantPermissions_Passes()
    {
        var permissions = new PluginPermissions
        {
            Network = new List<string> { "codeup.aliyun.com", "*.aliyuncs.com" },
            Credential = new List<string> { "yunxiao_pat" },
            Git = new List<string> { "clone_push" },
            Filesystem = new List<string> { "workspace" },
        };

        var result = Validator().Validate(Manifest(permissions));

        Assert.IsTrue(result.IsValid, result.ToSummary());
    }

    [TestMethod]
    public void OutOfWhitelist_Network_MakesManifestInvalid()
    {
        var permissions = new PluginPermissions { Network = new List<string> { "evil.example.com" } };

        var result = Validator().Validate(Manifest(permissions));

        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.Contains("permissions.network", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void BareNetworkWildcard_IsAcceptedWithWarning()
    {
        // 通配 = 「声明网络维度、范围由宿主白名单决定」：不算越界，但必须留下警告（运行期以白名单为上限）
        var permissions = new PluginPermissions { Network = new List<string> { "*" } };

        var result = Validator().Validate(Manifest(permissions));

        Assert.IsTrue(result.IsValid, result.ToSummary());
        Assert.IsTrue(result.Warnings.Any(w => w.Contains("permissions.network", StringComparison.Ordinal)),
            result.ToSummary());
        Assert.IsFalse(result.Errors.Any(), result.ToSummary());
    }

    [TestMethod]
    public void SkillLevel_Permissions_AreValidated_Too()
    {
        var permissions = new PluginPermissions { Process = new List<string> { "curl" } };
        var skills = new List<SkillDefinition>
        {
            new() { Id = "demo_skill", Name = "Demo", Permissions = permissions }
        };

        var result = Validator().Validate(Manifest(null, skills));

        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.Contains("skills[demo_skill].permissions", StringComparison.Ordinal)),
            result.ToSummary());
    }

    [TestMethod]
    public void HostWhitelist_Override_TightensAcceptance()
    {
        // 宿主收紧白名单后，原先合规的声明变为越界
        var options = new PluginSecurityOptions { NetworkWhitelist = new List<string> { "flow.aliyun.com" } };
        var permissions = new PluginPermissions { Network = new List<string> { "codeup.aliyun.com" } };

        var result = Validator(options).Validate(Manifest(permissions));

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void ManifestWithoutPermissions_IsUnaffected()
    {
        var result = Validator().Validate(Manifest(null));

        Assert.IsTrue(result.IsValid, result.ToSummary());
    }
}