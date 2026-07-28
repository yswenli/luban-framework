using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Skills;

namespace LuBan.AIAgent.Tests;

[TestClass]
public class SkillRegistryTests
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

    private sealed class FakeBuiltinSkill : ISkill
    {
        public string Id => "builtin1";
        public string Name => "内置技能";
        public string Description => "测试用";
        public string Category => "builtin";
        public IEnumerable<string> Examples => Array.Empty<string>();
        public Task<SkillResult> ExecuteAsync(SkillContext context, string input)
            => Task.FromResult(SkillResult.Ok("ok"));
    }

    private sealed class ConfigurableBuiltinSkill : ISkill
    {
        private readonly string _id;

        public ConfigurableBuiltinSkill(string id) => _id = id;

        public string Id => _id;
        public string Name => "内置技能";
        public string Description => "测试用";
        public string Category => "builtin";
        public IEnumerable<string> Examples => Array.Empty<string>();
        public Task<SkillResult> ExecuteAsync(SkillContext context, string input)
            => Task.FromResult(SkillResult.Ok("ok"));
    }

    [TestMethod]
    public void GetAll_MergesBuiltinAndCustom()
    {
        var cm = new ConfigManager(_tempPath);
        cm.AddCustomSkill(new CustomSkillConfig { Id = "custom1", Name = "自定义", Category = "custom" });
        var registry = new SkillRegistry(new ISkill[] { new FakeBuiltinSkill() }, cm);

        var all = registry.GetAll();
        Assert.AreEqual(2, all.Count);
        Assert.IsTrue(all.Any(s => s.Id == "builtin1"));
        Assert.IsTrue(all.Any(s => s.Id == "custom1"));
    }

    [TestMethod]
    public void GetAll_DisabledCustom_Excluded()
    {
        var cm = new ConfigManager(_tempPath);
        cm.AddCustomSkill(new CustomSkillConfig { Id = "custom1", Name = "自定义" });
        cm.SetCustomSkillEnabled("custom1", false);
        var registry = new SkillRegistry(new ISkill[] { new FakeBuiltinSkill() }, cm);

        Assert.AreEqual(1, registry.GetAll().Count);
    }

    [TestMethod]
    public void GetAll_DisabledBuiltin_Excluded()
    {
        var cm = new ConfigManager(_tempPath);
        cm.SetBuiltinSkillEnabled("builtin1", false);
        var registry = new SkillRegistry(new ISkill[] { new FakeBuiltinSkill() }, cm);

        Assert.AreEqual(0, registry.GetAll().Count);
    }

    [TestMethod]
    public void Get_FindsCustomSkill()
    {
        var cm = new ConfigManager(_tempPath);
        cm.AddCustomSkill(new CustomSkillConfig { Id = "custom1", Name = "自定义" });
        var registry = new SkillRegistry(Array.Empty<ISkill>(), cm);

        Assert.IsNotNull(registry.Get("custom1"));
    }

    [TestMethod]
    public void GetAll_ReflectsConfigChangeImmediately()
    {
        var cm = new ConfigManager(_tempPath);
        var registry = new SkillRegistry(Array.Empty<ISkill>(), cm);

        Assert.AreEqual(0, registry.GetAll().Count);

        cm.AddCustomSkill(new CustomSkillConfig { Id = "late", Name = "后加的" });

        Assert.AreEqual(1, registry.GetAll().Count);
    }

    [TestMethod]
    public void GetCategories_IncludesCustomCategory()
    {
        var cm = new ConfigManager(_tempPath);
        cm.AddCustomSkill(new CustomSkillConfig { Id = "c1", Name = "n", Category = "custom" });
        var registry = new SkillRegistry(new ISkill[] { new FakeBuiltinSkill() }, cm);

        var cats = registry.GetCategories();
        Assert.IsTrue(cats.Contains("builtin"));
        Assert.IsTrue(cats.Contains("custom"));
    }

    [TestMethod]
    public void GetAll_DuplicateId_BuiltinWins()
    {
        var cm = new ConfigManager(_tempPath);
        cm.AddCustomSkill(new CustomSkillConfig { Id = "dup", Name = "自定义" });
        var builtin = new ConfigurableBuiltinSkill("dup");
        var registry = new SkillRegistry(new ISkill[] { builtin }, cm);

        var dups = registry.GetAll().Where(s => s.Id == "dup").ToList();
        Assert.AreEqual(1, dups.Count);
        Assert.AreSame(builtin, registry.Get("dup"));
    }

    [TestMethod]
    public void Get_DisabledBuiltin_FallsBackToNull()
    {
        var cm = new ConfigManager(_tempPath);
        cm.SetBuiltinSkillEnabled("builtin1", false);
        var registry = new SkillRegistry(new ISkill[] { new FakeBuiltinSkill() }, cm);

        Assert.IsNull(registry.Get("builtin1"));
    }
}
