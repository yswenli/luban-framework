/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： ManifestValidatorUnitTest
*描述：验证 plugin.json 清单校验的必填项、id/版本格式、目标宿主、重复项与入口程序集存在性
*
*****************************************************************************/
using LuBan.AIAgent.Plugins;
using LuBan.AIAgent.Plugins.Loading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

/// <summary>
/// <see cref="ManifestValidator"/> 测试（PRD §3.2 清单字段约束）。
/// </summary>
[TestClass]
public class ManifestValidatorUnitTest
{
    private readonly ManifestValidator _validator = new();

    private static PluginManifest Valid() => new()
    {
        Id = "com.tjc.newproject",
        Name = "新建项目",
        Version = "1.0.0",
        EntryAssembly = "NewProject.dll",
        TargetApp = "LubanAgentCodex"
    };

    private static ManifestValidator ForHost(string targetApp, string hostVersion) => new(new ManifestValidationOptions
    {
        TargetApp = targetApp,
        HostVersion = hostVersion
    });

    [TestMethod]
    public void ValidManifest_HasNoErrors()
    {
        var result = _validator.Validate(Valid());

        Assert.IsTrue(result.IsValid, result.ToSummary());
    }

    [TestMethod]
    public void Null_ReportsParseFailure()
    {
        var result = _validator.Validate(null);

        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.ToSummary().Contains("解析失败"));
    }

    [TestMethod]
    public void MissingRequiredFields_ReportsFourErrors()
    {
        var result = _validator.Validate(new PluginManifest());

        Assert.AreEqual(4, result.Errors.Count, result.ToSummary());
    }

    [TestMethod]
    [DataRow("com.tjc.newproject")]      // 反向域名
    [DataRow("newproject")]              // 单词
    [DataRow("com-tjc-newproject")]      // 中划线
    [DataRow("com_tjc_newproject")]      // 下划线
    [DataRow("a1")]                      // 字母开头 + 数字
    public void PluginId_AcceptedFormats(string id)
    {
        var manifest = Valid();
        manifest.Id = id;

        Assert.IsTrue(_validator.Validate(manifest).IsValid);
    }

    [TestMethod]
    [DataRow("1com.tjc")]                // 数字开头
    [DataRow(".com.tjc")]                // 点开头
    [DataRow("com tjc")]                 // 空格
    [DataRow("com/tjc")]                 // 斜杠
    [DataRow("a")]                       // 少于 2 字符
    public void PluginId_RejectedFormats(string id)
    {
        var manifest = Valid();
        manifest.Id = id;

        var result = _validator.Validate(manifest);

        Assert.IsFalse(result.IsValid, $"id 应被拒绝：{id}");
        Assert.IsTrue(result.ToSummary().Contains("id 格式非法"));
    }

    [TestMethod]
    [DataRow("1.0")]
    [DataRow("1.0.0")]
    [DataRow("2026.10.15.2")]
    [DataRow("1.0.0-beta.1")]
    public void Version_AcceptedFormats(string version)
    {
        var manifest = Valid();
        manifest.Version = version;

        Assert.IsTrue(_validator.Validate(manifest).IsValid, $"版本应被接受：{version}");
    }

    [TestMethod]
    [DataRow("1")]
    [DataRow("v1.0.0")]
    [DataRow("1.0.0.0.0")]
    [DataRow("abc")]
    public void Version_RejectedFormats(string version)
    {
        var manifest = Valid();
        manifest.Version = version;

        var result = _validator.Validate(manifest);

        Assert.IsFalse(result.IsValid, $"版本应被拒绝：{version}");
        Assert.IsTrue(result.ToSummary().Contains("语义化版本"));
    }

    [TestMethod]
    public void MinHostVersion_InvalidFormat_ReportsError()
    {
        var manifest = Valid();
        manifest.MinHostVersion = "latest";

        var result = _validator.Validate(manifest);

        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.ToSummary().Contains("minHostVersion"));
    }

    [TestMethod]
    public void MinHostVersion_HigherThanHost_ReportsError()
    {
        var manifest = Valid();
        manifest.MinHostVersion = "2027.1.1";

        var result = ForHost("LubanAgentCodex", "2026.10.15").Validate(manifest);

        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.ToSummary().Contains("宿主版本过低"));
    }

    [TestMethod]
    public void MinHostVersion_EqualToHost_IsValid()
    {
        var manifest = Valid();
        manifest.MinHostVersion = "2026.10.15";

        Assert.IsTrue(ForHost("LubanAgentCodex", "2026.10.15").Validate(manifest).IsValid);
    }

    [TestMethod]
    public void TargetApp_Mismatch_ReportsError()
    {
        var manifest = Valid();
        manifest.TargetApp = "SomeOtherHost";

        var result = ForHost("LubanAgentCodex", "2026.10.15").Validate(manifest);

        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.ToSummary().Contains("targetApp 不匹配"));
    }

    [TestMethod]
    public void TargetApp_Missing_IsWarningNotError()
    {
        var manifest = Valid();
        manifest.TargetApp = null;

        var result = ForHost("LubanAgentCodex", "2026.10.15").Validate(manifest);

        Assert.IsTrue(result.IsValid, result.ToSummary());
        Assert.AreEqual(1, result.Warnings.Count);
    }

    [TestMethod]
    public void TargetApp_SkippedWhenHostNotConfigured()
    {
        var manifest = Valid();
        manifest.TargetApp = "SomeOtherHost";

        Assert.IsTrue(_validator.Validate(manifest).IsValid);
    }

    [TestMethod]
    public void UiContributions_DuplicateId_ReportsError()
    {
        var manifest = Valid();
        manifest.UiContributions.Add(new PluginSidebarEntry { Id = "project", Label = "项目", PanelType = "P" });
        manifest.UiContributions.Add(new PluginSidebarEntry { Id = "PROJECT", Label = "项目2", PanelType = "P2" });

        var result = _validator.Validate(manifest);

        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.ToSummary().Contains("重复 id"));
    }

    [TestMethod]
    public void UiContributions_MissingLabelAndPanelType_AreWarnings()
    {
        var manifest = Valid();
        manifest.UiContributions.Add(new PluginSidebarEntry { Id = "project" });

        var result = _validator.Validate(manifest);

        Assert.IsTrue(result.IsValid, result.ToSummary());
        Assert.AreEqual(2, result.Warnings.Count);
    }

    [TestMethod]
    public void Skills_DotInId_ReportsError()
    {
        var manifest = Valid();
        manifest.Skills.Add(new SkillDefinition { Id = "newproject.create", Name = "创建项目" });

        var result = _validator.Validate(manifest);

        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.ToSummary().Contains("skill id 格式非法"));
    }

    [TestMethod]
    public void Skills_AcceptedIdFormat()
    {
        var manifest = Valid();
        manifest.Skills.Add(new SkillDefinition { Id = "newproject-create_2", Name = "创建项目" });

        Assert.IsTrue(_validator.Validate(manifest).IsValid);
    }

    [TestMethod]
    public void Skills_DuplicateId_ReportsError()
    {
        var manifest = Valid();
        manifest.Skills.Add(new SkillDefinition { Id = "newproject", Name = "A" });
        manifest.Skills.Add(new SkillDefinition { Id = "NewProject", Name = "B" });

        var result = _validator.Validate(manifest);

        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.ToSummary().Contains("重复 id"));
    }

    [TestMethod]
    public void McpServers_DuplicateName_ReportsError()
    {
        var manifest = Valid();
        manifest.McpServers.Add(new PluginMcpServerDeclaration { Name = "yunxiao" });
        manifest.McpServers.Add(new PluginMcpServerDeclaration { Name = "Yunxiao" });

        var result = _validator.Validate(manifest);

        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.ToSummary().Contains("重复 name"));
    }

    [TestMethod]
    public void McpServers_NonInprocTransport_ReportsError()
    {
        var manifest = Valid();
        manifest.McpServers.Add(new PluginMcpServerDeclaration { Name = "yunxiao", Transport = "stdio" });

        var result = _validator.Validate(manifest);

        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.ToSummary().Contains("仅支持 inproc"));
    }

    [TestMethod]
    public void McpServers_InprocOrDefaultTransport_IsValid()
    {
        var manifest = Valid();
        manifest.McpServers.Add(new PluginMcpServerDeclaration { Name = "yunxiao", Transport = "inproc" });
        manifest.McpServers.Add(new PluginMcpServerDeclaration { Name = "other" });

        Assert.IsTrue(_validator.Validate(manifest).IsValid);
    }

    [TestMethod]
    public void EnabledMcpServers_ExcludesDisabledDeclarations()
    {
        var manifest = Valid();
        manifest.McpServers.Add(new PluginMcpServerDeclaration { Name = "yunxiao", Enabled = true });
        manifest.McpServers.Add(new PluginMcpServerDeclaration { Name = "flow", Enabled = false });

        CollectionAssert.AreEqual(new[] { "yunxiao" }, manifest.EnabledMcpServers.Select(s => s.Name).ToArray());
    }

    [TestMethod]
    public void Dependencies_SelfReference_ReportsError()
    {
        var manifest = Valid();
        manifest.Dependencies.Add("com.tjc.newproject");

        var result = _validator.Validate(manifest);

        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.ToSummary().Contains("不允许自引用"));
    }

    [TestMethod]
    public void EntryAssembly_MissingOnDisk_ReportsError()
    {
        var directory = PluginTestDirectory.Create();
        try
        {
            var manifest = Valid();
            manifest.EntryAssembly = "NotThere.dll";

            var result = _validator.Validate(manifest, directory.Path);

            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.ToSummary().Contains("入口程序集不存在"));
        }
        finally
        {
            directory.Dispose();
        }
    }

    [TestMethod]
    public void EntryAssembly_InLibFolder_IsValid()
    {
        var directory = PluginTestDirectory.Create();
        try
        {
            directory.WriteFile(Path.Combine("lib", "NewProject.dll"), "x");

            Assert.IsTrue(_validator.Validate(Valid(), directory.Path).IsValid);
        }
        finally
        {
            directory.Dispose();
        }
    }

    [TestMethod]
    public void RequireSignature_EmitsWarning()
    {
        var validator = new ManifestValidator(new ManifestValidationOptions { RequireSignature = true });

        var result = validator.Validate(Valid());

        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(1, result.Warnings.Count);
        Assert.IsTrue(result.Warnings[0].Contains("签名校验"));
    }
}