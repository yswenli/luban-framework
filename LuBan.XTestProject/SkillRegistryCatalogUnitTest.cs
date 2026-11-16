/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： SkillRegistryCatalogUnitTest
*描述：验证 SkillRegistry.BuildCatalogPrompt 排序、截断、空集行为
*****************************************************************************/
using LuBan.AIAgent.Skills;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

[TestClass]
public class SkillRegistryCatalogUnitTest
{
    [TestMethod]
    public void BuildCatalogPrompt_ReturnsSortedAndTruncated()
    {
        // Arrange: 3 个测试 Skill
        var skills = new[]
        {
            new TestSkill("zebra", "Zebra Skill", "Z desc"),
            new TestSkill("alpha", "Alpha Skill", "A desc"),
            new TestSkill("beta", "Beta Skill", "B desc"),
        };
        var registry = new SkillRegistry(skills);

        // Act: 限制 2 条
        var prompt = registry.BuildCatalogPrompt(2);

        // Assert: 按 Id 排序，只保留前 2 条
        Assert.IsTrue(prompt.Contains("- **alpha**"));
        Assert.IsTrue(prompt.Contains("- **beta**"));
        Assert.IsFalse(prompt.Contains("- **zebra**"));
        Assert.IsTrue(prompt.StartsWith("## 可用 Skill 清单"));
    }

    [TestMethod]
    public void BuildCatalogPrompt_Empty_ReturnsFallback()
    {
        var registry = new SkillRegistry(Array.Empty<ISkill>());
        var prompt = registry.BuildCatalogPrompt(50);
        Assert.IsTrue(prompt.Contains("当前无可用 Skill"));
    }

    [TestMethod]
    public void IsBuiltin_ReturnsCorrectly()
    {
        var skills = new[] { new TestSkill("builtin-one", "B1", "desc") };
        var registry = new SkillRegistry(skills);

        Assert.IsTrue(registry.IsBuiltin("builtin-one"));
        Assert.IsFalse(registry.IsBuiltin("not-exists"));
    }

    [TestMethod]
    public void BuildCatalogPrompt_ZeroMaxEntries_Throws()
    {
        var registry = new SkillRegistry(Array.Empty<ISkill>());
        try
        {
            registry.BuildCatalogPrompt(0);
            Assert.Fail("应该抛出 ArgumentOutOfRangeException 异常");
        }
        catch (ArgumentOutOfRangeException)
        {
            // 预期的异常
        }
    }

    [TestMethod]
    public void BuildCatalogPrompt_NegativeMaxEntries_Throws()
    {
        var registry = new SkillRegistry(Array.Empty<ISkill>());
        try
        {
            registry.BuildCatalogPrompt(-1);
            Assert.Fail("应该抛出 ArgumentOutOfRangeException 异常");
        }
        catch (ArgumentOutOfRangeException)
        {
            // 预期的异常
        }
    }

    [TestMethod]
    public void IsBuiltin_NullId_Throws()
    {
        var registry = new SkillRegistry(Array.Empty<ISkill>());
        try
        {
            registry.IsBuiltin(null!);
            Assert.Fail("应该抛出 ArgumentNullException 异常");
        }
        catch (ArgumentNullException)
        {
            // 预期的异常
        }
    }

    [TestMethod]
    public void LoadFromWorkspace_FileOverridesBuiltinWithSameId()
    {
        var root = Path.Combine(Path.GetTempPath(), "luban-skillpriority-" + Guid.NewGuid().ToString("N"));
        var skillDir = Path.Combine(root, ".luban-agent", "skills", "builtin-one");
        Directory.CreateDirectory(skillDir);
        File.WriteAllText(Path.Combine(skillDir, "SKILL.md"),
            "---\nname: 覆盖版\ndescription: 来自工作区文件\ncategory: test\n---\n正文");

        try
        {
            var registry = new SkillRegistry(new[] { new TestSkill("builtin-one", "内置版", "内置描述") });
            registry.LoadFromWorkspace(root);

            var skill = registry.Get("builtin-one");
            Assert.IsNotNull(skill);
            Assert.AreEqual("覆盖版", skill.Name, "同名工作区文件应覆盖内置 Skill");
            Assert.AreEqual(1, registry.GetAll().Count(s => s.Id.Equals("builtin-one", StringComparison.OrdinalIgnoreCase)),
                "覆盖是替换而非并列，合并结果只保留一条");
            Assert.IsTrue(registry.IsBuiltin("builtin-one"), "内置身份不因被工作区文件覆盖而消失");
        }
        finally
        {
            try { Directory.Delete(root, recursive: true); } catch { /* 测试清理失败不影响断言 */ }
        }
    }

    private sealed class TestSkill : ISkill
    {
        public TestSkill(string id, string name, string desc)
        {
            Id = id;
            Name = name;
            Description = desc;
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
}