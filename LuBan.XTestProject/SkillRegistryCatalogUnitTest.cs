/****************************************************************************
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