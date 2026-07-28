using LuBan.AIAgent.Configuration;

namespace LuBan.AIAgent.Tests;

[TestClass]
public class ConfigManagerTests
{
    private string _tempPath = "";

    [TestInitialize]
    public void Setup()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), $"luban_test_{Guid.NewGuid():N}.json");
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (File.Exists(_tempPath)) File.Delete(_tempPath);
    }

    [TestMethod]
    public void CustomSkill_Add_PersistsAndReloads()
    {
        var cm = new ConfigManager(_tempPath);
        cm.AddCustomSkill(new CustomSkillConfig { Id = "s1", Name = "技能1", PromptTemplate = "做 {input}" });

        var cm2 = new ConfigManager(_tempPath);
        cm2.Load();

        Assert.AreEqual(1, cm2.CustomSkills.Count);
        Assert.AreEqual("s1", cm2.CustomSkills[0].Id);
        Assert.AreEqual("做 {input}", cm2.CustomSkills[0].PromptTemplate);
    }

    [TestMethod]
    public void CustomSkill_Add_DuplicateId_Throws()
    {
        var cm = new ConfigManager(_tempPath);
        cm.AddCustomSkill(new CustomSkillConfig { Id = "s1", Name = "a" });
        Assert.ThrowsException<InvalidOperationException>(() =>
            cm.AddCustomSkill(new CustomSkillConfig { Id = "s1", Name = "b" }));
    }

    [TestMethod]
    public void CustomSkill_Update_ModifiesFields()
    {
        var cm = new ConfigManager(_tempPath);
        cm.AddCustomSkill(new CustomSkillConfig { Id = "s1", Name = "a", PromptTemplate = "old" });
        cm.UpdateCustomSkill(new CustomSkillConfig { Id = "s1", Name = "b", PromptTemplate = "new" });
        Assert.AreEqual("b", cm.CustomSkills[0].Name);
        Assert.AreEqual("new", cm.CustomSkills[0].PromptTemplate);
    }

    [TestMethod]
    public void CustomSkill_Remove_DeletesEntry()
    {
        var cm = new ConfigManager(_tempPath);
        cm.AddCustomSkill(new CustomSkillConfig { Id = "s1", Name = "a" });
        cm.RemoveCustomSkill("s1");
        Assert.AreEqual(0, cm.CustomSkills.Count);
    }

    [TestMethod]
    public void CustomSkill_SetEnabled_TogglesFlag()
    {
        var cm = new ConfigManager(_tempPath);
        cm.AddCustomSkill(new CustomSkillConfig { Id = "s1", Name = "a" });
        cm.SetCustomSkillEnabled("s1", false);
        Assert.IsFalse(cm.CustomSkills[0].Enabled);
    }

    [TestMethod]
    public void BuiltinSkill_SetEnabled_WritesDisabledList()
    {
        var cm = new ConfigManager(_tempPath);
        cm.SetBuiltinSkillEnabled("brainstorming", false);
        Assert.IsTrue(cm.DisabledBuiltinSkills.Contains("brainstorming"));
        cm.SetBuiltinSkillEnabled("brainstorming", true);
        Assert.IsFalse(cm.DisabledBuiltinSkills.Contains("brainstorming"));
    }

    [TestMethod]
    public void CustomRule_Add_PersistsAndReloads()
    {
        var cm = new ConfigManager(_tempPath);
        cm.AddCustomRule(new CustomRuleConfig { Id = "r1", Name = "规则1", Action = "deny" });

        var cm2 = new ConfigManager(_tempPath);
        cm2.Load();

        Assert.AreEqual(1, cm2.CustomRules.Count);
        Assert.AreEqual("r1", cm2.CustomRules[0].Id);
        Assert.AreEqual("deny", cm2.CustomRules[0].Action);
    }

    [TestMethod]
    public void McpServer_Add_PersistsAndReloads()
    {
        var cm = new ConfigManager(_tempPath);
        cm.AddMcpServer(new McpServerConfig { Name = "fs", Command = "npx", Args = new List<string> { "-y", "pkg" } });

        var cm2 = new ConfigManager(_tempPath);
        cm2.Load();

        Assert.AreEqual(1, cm2.McpServers.Count);
        Assert.AreEqual("fs", cm2.McpServers[0].Name);
        Assert.AreEqual(2, cm2.McpServers[0].Args.Count);
    }

    [TestMethod]
    public void McpServer_Add_DuplicateName_Throws()
    {
        var cm = new ConfigManager(_tempPath);
        cm.AddMcpServer(new McpServerConfig { Name = "fs", Command = "npx" });
        Assert.ThrowsException<InvalidOperationException>(() =>
            cm.AddMcpServer(new McpServerConfig { Name = "fs", Command = "node" }));
    }
}
