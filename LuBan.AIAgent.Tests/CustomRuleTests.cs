using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Rules;

namespace LuBan.AIAgent.Tests;

[TestClass]
public class CustomRuleTests
{
    private static CustomRule CreateRule(string actionTypePattern = "*", string targetPattern = "*", string action = "deny")
        => new(new CustomRuleConfig
        {
            Id = "r1",
            Name = "测试规则",
            ActionTypePattern = actionTypePattern,
            TargetPattern = targetPattern,
            Action = action
        });

    [TestMethod]
    public void IsApplicable_StarPattern_MatchesEverything()
    {
        var rule = CreateRule();
        Assert.IsTrue(rule.IsApplicable(new RuleContext { ActionType = "tool-call", Target = "read_file" }));
        Assert.IsTrue(rule.IsApplicable(new RuleContext { ActionType = "file-write", Target = "D:\\a.txt" }));
    }

    [TestMethod]
    public void IsApplicable_ExactPattern_MatchesCaseInsensitive()
    {
        var rule = CreateRule(targetPattern: "RunShellAsync");
        Assert.IsTrue(rule.IsApplicable(new RuleContext { ActionType = "tool-call", Target = "runshellasync" }));
        Assert.IsFalse(rule.IsApplicable(new RuleContext { ActionType = "tool-call", Target = "read_file" }));
    }

    [TestMethod]
    public void IsApplicable_PrefixWildcard_Matches()
    {
        var rule = CreateRule(targetPattern: "run*");
        Assert.IsTrue(rule.IsApplicable(new RuleContext { ActionType = "tool-call", Target = "RunPythonAsync" }));
        Assert.IsFalse(rule.IsApplicable(new RuleContext { ActionType = "tool-call", Target = "read_file" }));
    }

    [TestMethod]
    public void IsApplicable_SuffixWildcard_Matches()
    {
        var rule = CreateRule(targetPattern: "*.txt");
        Assert.IsTrue(rule.IsApplicable(new RuleContext { ActionType = "file-write", Target = "a.txt" }));
        Assert.IsFalse(rule.IsApplicable(new RuleContext { ActionType = "file-write", Target = "a.md" }));
    }

    [TestMethod]
    public void IsApplicable_ActionTypeMustAlsoMatch()
    {
        var rule = CreateRule(actionTypePattern: "file-*", targetPattern: "*");
        Assert.IsTrue(rule.IsApplicable(new RuleContext { ActionType = "file-write", Target = "x" }));
        Assert.IsFalse(rule.IsApplicable(new RuleContext { ActionType = "tool-call", Target = "x" }));
    }

    [TestMethod]
    public async Task ExecuteAsync_DenyAction_ReturnsDeny()
    {
        var rule = CreateRule(action: "deny");
        var result = await rule.ExecuteAsync(new RuleContext());
        Assert.IsFalse(result.Allow);
        StringAssert.Contains(result.Message, "测试规则");
    }

    [TestMethod]
    public async Task ExecuteAsync_AllowAction_ReturnsAllow()
    {
        var rule = CreateRule(action: "allow");
        var result = await rule.ExecuteAsync(new RuleContext());
        Assert.IsTrue(result.Allow);
    }
}
