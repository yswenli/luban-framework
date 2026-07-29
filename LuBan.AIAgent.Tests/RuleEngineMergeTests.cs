using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Rules;

namespace LuBan.AIAgent.Tests;

[TestClass]
public class RuleEngineMergeTests
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

    private sealed class FakeBuiltinRule : IRule
    {
        public string Id => "builtin-rule";
        public string Name => "内置规则";
        public string Description => "测试";
        public int Priority => 1;
        public bool IsEnabled { get; set; } = true;
        public bool IsApplicable(RuleContext context) => false;
        public Task<RuleResult> ExecuteAsync(RuleContext context)
            => Task.FromResult(RuleResult.AllowResult());
    }

    [TestMethod]
    public void GetAllRules_MergesBuiltinAndCustom()
    {
        var cm = new ConfigManager(_tempPath);
        cm.AddCustomRule(new CustomRuleConfig { Id = "cr1", Name = "自定义规则" });
        var engine = new RuleEngine(new IRule[] { new FakeBuiltinRule() }, cm);

        Assert.AreEqual(2, engine.GetAllRules().Count);
    }

    [TestMethod]
    public void GetEnabledRules_ExcludesDisabledCustom()
    {
        var cm = new ConfigManager(_tempPath);
        cm.AddCustomRule(new CustomRuleConfig { Id = "cr1", Name = "自定义规则" });
        cm.SetCustomRuleEnabled("cr1", false);
        var engine = new RuleEngine(new IRule[] { new FakeBuiltinRule() }, cm);

        Assert.AreEqual(1, engine.GetEnabledRules().Count);
    }

    [TestMethod]
    public void GetAllRules_ExcludesDisabledBuiltin()
    {
        var cm = new ConfigManager(_tempPath);
        cm.SetBuiltinRuleEnabled("builtin-rule", false);
        var engine = new RuleEngine(new IRule[] { new FakeBuiltinRule() }, cm);

        Assert.AreEqual(0, engine.GetAllRules().Count);
    }

    [TestMethod]
    public async Task EvaluateAsync_CustomDenyRule_BlocksToolCall()
    {
        var cm = new ConfigManager(_tempPath);
        cm.AddCustomRule(new CustomRuleConfig
        {
            Id = "no-shell",
            Name = "禁止Shell",
            ActionTypePattern = "tool-call",
            TargetPattern = "run*",
            Action = "deny"
        });
        var engine = new RuleEngine(Array.Empty<IRule>(), cm);

        var result = await engine.EvaluateAsync(new RuleContext
        {
            ActionType = "tool-call",
            Target = "RunShellAsync"
        });

        Assert.IsFalse(result.Allow);
    }

    [TestMethod]
    public void GetAllRules_SortedByPriorityDescending()
    {
        var cm = new ConfigManager(_tempPath);
        cm.AddCustomRule(new CustomRuleConfig { Id = "low", Name = "低", Priority = 10 });
        cm.AddCustomRule(new CustomRuleConfig { Id = "high", Name = "高", Priority = 999 });
        var engine = new RuleEngine(Array.Empty<IRule>(), cm);

        var rules = engine.GetAllRules();
        Assert.AreEqual("high", rules[0].Id);
        Assert.AreEqual("low", rules[1].Id);
    }

    [TestMethod]
    public void GetAllRules_DuplicateId_BuiltinWins()
    {
        var cm = new ConfigManager(_tempPath);
        cm.AddCustomRule(new CustomRuleConfig { Id = "builtin-rule", Name = "同名自定义" });
        var engine = new RuleEngine(new IRule[] { new FakeBuiltinRule() }, cm);

        var rules = engine.GetAllRules();
        Assert.AreEqual(1, rules.Count);
        Assert.AreEqual("内置规则", rules[0].Name);
    }

    [TestMethod]
    public void GetAllRules_DisabledCustom_StillListed()
    {
        var cm = new ConfigManager(_tempPath);
        cm.AddCustomRule(new CustomRuleConfig { Id = "cr1", Name = "自定义规则" });
        cm.SetCustomRuleEnabled("cr1", false);
        var engine = new RuleEngine(Array.Empty<IRule>(), cm);

        Assert.AreEqual(1, engine.GetAllRules().Count);
        Assert.IsFalse(engine.GetAllRules()[0].IsEnabled);
    }

    [TestMethod]
    public void GetRule_CaseInsensitive()
    {
        var cm = new ConfigManager(_tempPath);
        cm.AddCustomRule(new CustomRuleConfig { Id = "cr1", Name = "自定义规则" });
        var engine = new RuleEngine(Array.Empty<IRule>(), cm);

        Assert.IsNotNull(engine.GetRule("CR1"));
    }
}
