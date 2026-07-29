using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Rules;
using Microsoft.Extensions.AI;

namespace LuBan.AIAgent.Tests;

[TestClass]
public class RuleCheckedAIFunctionTests
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

    private sealed class EchoFunction : AIFunction
    {
        public AIFunctionArguments? LastArguments;
        public override string Name => "echo";
        protected override ValueTask<object?> InvokeCoreAsync(
            AIFunctionArguments arguments, CancellationToken cancellationToken)
        {
            LastArguments = arguments;
            return new ValueTask<object?>("ok");
        }
    }

    private sealed class ModifyRule : IRule
    {
        public string Id => "modify-rule";
        public string Name => "修改参数";
        public string Description => "测试";
        public int Priority => 1;
        public bool IsEnabled { get; set; } = true;
        public bool IsApplicable(RuleContext context) => context.Target == "echo";
        public Task<RuleResult> ExecuteAsync(RuleContext context)
        {
            var args = new Dictionary<string, object?>(context.Arguments) { ["extra"] = "added" };
            return Task.FromResult(RuleResult.ModifyResult(args));
        }
    }

    [TestMethod]
    public async Task Invoke_NoApplicableRule_PassesThrough()
    {
        var engine = new RuleEngine(Array.Empty<IRule>());
        var echo = new EchoFunction();
        var checked_ = new RuleCheckedAIFunction(echo, engine);

        var result = await checked_.InvokeAsync(new AIFunctionArguments());

        Assert.AreEqual("ok", result);
        Assert.IsNotNull(echo.LastArguments);
    }

    [TestMethod]
    public async Task Invoke_DenyRule_BlocksAndReturnsMessage()
    {
        var cm = new ConfigManager(_tempPath);
        cm.AddCustomRule(new CustomRuleConfig
        {
            Id = "deny-echo",
            Name = "禁止echo",
            ActionTypePattern = "tool-call",
            TargetPattern = "echo",
            Action = "deny"
        });
        var engine = new RuleEngine(Array.Empty<IRule>(), cm);
        var echo = new EchoFunction();
        var checked_ = new RuleCheckedAIFunction(echo, engine);

        var result = await checked_.InvokeAsync(new AIFunctionArguments());

        Assert.IsNull(echo.LastArguments);
        StringAssert.Contains(result?.ToString(), "拒绝");
    }

    [TestMethod]
    public async Task Invoke_WildcardDenyRule_MatchesToolName()
    {
        var cm = new ConfigManager(_tempPath);
        cm.AddCustomRule(new CustomRuleConfig
        {
            Id = "deny-all",
            Name = "禁止所有工具",
            ActionTypePattern = "tool-call",
            TargetPattern = "*",
            Action = "deny"
        });
        var engine = new RuleEngine(Array.Empty<IRule>(), cm);
        var echo = new EchoFunction();
        var checked_ = new RuleCheckedAIFunction(echo, engine);

        var result = await checked_.InvokeAsync(new AIFunctionArguments());

        Assert.IsNull(echo.LastArguments);
    }

    [TestMethod]
    public async Task Invoke_ModifyRule_UsesModifiedArguments()
    {
        var engine = new RuleEngine(new IRule[] { new ModifyRule() });
        var echo = new EchoFunction();
        var checked_ = new RuleCheckedAIFunction(echo, engine);

        var result = await checked_.InvokeAsync(new AIFunctionArguments());

        Assert.AreEqual("ok", result);
        Assert.IsTrue(echo.LastArguments!.ContainsKey("extra"));
    }
}
