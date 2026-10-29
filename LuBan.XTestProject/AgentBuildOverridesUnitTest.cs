/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.XTestProject
*文件名： AgentBuildOverridesUnitTest
*版本号： V1.0.0.0
*唯一标识：d2a6f1c9-8e35-4b70-9f42-5c0d7a3e1b86
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/29
*描述：按轮构建覆盖项（人设 / 工具开关）单元测试
*
*=================================================
*修改标记
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：按轮构建覆盖项（人设 / 工具开关）单元测试
*
*****************************************************************************/

using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Hosting;
using LuBan.AIAgent.Rules;

namespace LuBan.XTestProject;

[TestClass]
public class AgentBuildOverridesUnitTest
{
    [TestMethod]
    public void Equals_ComparesBothFields()
    {
        var a = new AgentBuildOverrides { SystemPrompt = "x", EnableTools = true };
        var b = new AgentBuildOverrides { SystemPrompt = "x", EnableTools = true };
        var c = new AgentBuildOverrides { SystemPrompt = "x", EnableTools = false };

        Assert.IsTrue(a.Matches(b));
        Assert.IsFalse(a.Matches(c));
        Assert.IsTrue(a.Matches(new AgentBuildOverrides { SystemPrompt = "x" }));
    }

    [TestMethod]
    public void NullMeansDefaults()
    {
        Assert.IsTrue(new AgentBuildOverrides().Matches(null) is false);
        Assert.IsNull(new AgentBuildOverrides { SystemPrompt = null }.SystemPrompt);
        Assert.IsTrue(new AgentBuildOverrides().IsDefault);
        Assert.IsFalse(new AgentBuildOverrides { SystemPrompt = "x" }.IsDefault);
        Assert.IsFalse(new AgentBuildOverrides { EnableTools = false }.IsDefault);
    }

    [TestMethod]
    public void DisableTools_KillsSkillCatalogInjection()
    {
        var profile = new TestProfile();

        var options = profile.ResolveToolOptions(new AgentBuildOverrides { EnableTools = false });

        Assert.IsFalse(options.Skill.CatalogEnabled);
    }

    [TestMethod]
    public void Prompt_WithSystemOverrideAndNoTools_SkipsToolSections()
    {
        var profile = new TestProfile();
        var ws = new WorkspaceDescriptor("w", "W", Path.GetTempPath(), "normal");

        var prompt = profile.Build(ws, new AgentBuildOverrides
        {
            SystemPrompt = "你是合同审阅助手",
            EnableTools = false
        });

        StringAssert.StartsWith(prompt, "你是合同审阅助手");
        StringAssert.Contains(prompt, "## 当前工作区");
        Assert.IsFalse(prompt.Contains("## 临时文件管理"));
        Assert.IsFalse(prompt.Contains("## 长期记忆"));
        Assert.IsFalse(prompt.Contains("## 路径使用说明"));
    }

    [TestMethod]
    public void Prompt_WithTools_KeepsToolSections()
    {
        var profile = new TestProfile();
        var ws = new WorkspaceDescriptor("w", "W", Path.GetTempPath(), "normal");

        var prompt = profile.Build(ws, null);

        StringAssert.Contains(prompt, "## 临时文件管理");
    }

    private sealed class TestProfile : AgentProfileBase
    {
        public override string SystemPrompt => "默认人设";
        public override string[]? ToolGroups => ["file"];
        public override string? RetrievalMode => null;

        public new ToolGroupOptions ResolveToolOptions(AgentBuildOverrides? o) => base.ResolveToolOptions(o);

        public string Build(WorkspaceDescriptor ws, AgentBuildOverrides? o)
            => BuildFullPrompt(ws, new RuleEngine(Array.Empty<IRule>()), o);
    }
}