using LuBan.AIAgent.Rules;

namespace LuBan.AIAgent.Tests;

[TestClass]
public class RuleEngineInjectTests
{
    private sealed class InjectRule : RuleBase
    {
        public override string Id => "inject-rule";
        public override string Name => "注入规则";
        public override string Description => "";
        public override bool IsApplicable(RuleContext context) => context.ActionType == "context-build";
        public override Task<RuleResult> ExecuteAsync(RuleContext context)
        {
            var r = RuleResult.AllowResult();
            r.Inject.Add("[测试注入] hello");
            return Task.FromResult(r);
        }
    }

    [TestMethod]
    public async Task Evaluate_ContextBuild_AggregatesInject()
    {
        var engine = new RuleEngine(new IRule[] { new InjectRule() });
        var result = await engine.EvaluateAsync(new RuleContext { ActionType = "context-build", UserInput = "hi" });

        Assert.IsTrue(result.Allow);
        Assert.AreEqual(1, result.Inject.Count);
        Assert.AreEqual("[测试注入] hello", result.Inject[0]);
    }

    [TestMethod]
    public async Task Evaluate_ToolCall_DoesNotMatchContextRule()
    {
        var engine = new RuleEngine(new IRule[] { new InjectRule() });
        var result = await engine.EvaluateAsync(new RuleContext { ActionType = "tool-call", Target = "read-file" });

        Assert.AreEqual(0, result.Inject.Count);
    }
}
