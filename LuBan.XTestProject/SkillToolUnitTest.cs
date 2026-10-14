/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： SkillToolUnitTest
*描述：验证 ListSkillsAsync / UseSkillAsync 工具行为
*****************************************************************************/
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Skills;
using LuBan.AIAgent.Tools.Skill;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

[TestClass]
public class SkillToolUnitTest
{
    [TestMethod]
    public void ListSkillsAsync_ReturnsSorted()
    {
        var skills = new[]
        {
            new TestSkill("b-skill", "B", "B desc"),
            new TestSkill("a-skill", "A", "A desc"),
        };
        var registry = new SkillRegistry(skills);
        var group = new SkillToolGroup(registry, new SkillToolOptions());

        var result = group.ListSkillsAsync(null);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(2, result.Data?.Count);
        Assert.AreEqual("a-skill", result.Data?[0].Id);
        Assert.AreEqual("b-skill", result.Data?[1].Id);
    }

    [TestMethod]
    public void UseSkillAsync_ReturnsTemplate()
    {
        var skill = new TestSkillWithTemplate("demo", "Demo", "Demo desc", "Hello {input}");
        var registry = new SkillRegistry(new[] { skill });
        var group = new SkillToolGroup(registry, new SkillToolOptions());

        var result = group.UseSkillAsync("demo");

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.Data?.PromptTemplate?.Contains("<用户问题>") == true);
    }

    [TestMethod]
    public void UseSkillAsync_NotFound_ReturnsFail()
    {
        var registry = new SkillRegistry(Array.Empty<ISkill>());
        var group = new SkillToolGroup(registry, new SkillToolOptions());

        var result = group.UseSkillAsync("not-exist");

        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Message?.Contains("未找到 Skill") == true);
    }

    private class TestSkill : ISkill
    {
        public TestSkill(string id, string name, string desc)
        {
            Id = id; Name = name; Description = desc;
        }
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public string Category => "test";
        public IEnumerable<string> Examples => Array.Empty<string>();
        public IEnumerable<string> TriggerKeywords => Array.Empty<string>();
        public string? PromptTemplate => null;
        public Task<SkillResult> ExecuteAsync(SkillContext context, string input)
            => Task.FromResult(SkillResult.Ok("ok"));
    }

    private sealed class TestSkillWithTemplate : ISkill
    {
        private readonly string _template;
        public TestSkillWithTemplate(string id, string name, string desc, string template)
        {
            Id = id; Name = name; Description = desc; _template = template;
        }
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public string Category => "test";
        public IEnumerable<string> Examples => Array.Empty<string>();
        public IEnumerable<string> TriggerKeywords => Array.Empty<string>();
        public string? PromptTemplate => _template;
        public Task<SkillResult> ExecuteAsync(SkillContext context, string input)
            => Task.FromResult(SkillResult.Ok("ok"));
    }
}