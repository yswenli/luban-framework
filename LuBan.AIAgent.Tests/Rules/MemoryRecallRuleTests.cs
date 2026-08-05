using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.LocalMemory;
using LuBan.AIAgent.Rules;
using LuBan.AIAgent.Rules.BuiltIn;
using Microsoft.Extensions.Options;

namespace LuBan.AIAgent.Tests.Rules;

[TestClass]
public class MemoryRecallRuleTests
{
    private sealed class FakeMemory : ILocalMemoryService
    {
        public List<MemorySearchResult> Results { get; set; } = new();
        public Task<MemoryEntry> SaveAsync(string content, string category = "general", CancellationToken ct = default)
            => Task.FromResult(new MemoryEntry { Content = content, Category = category });
        public Task<IReadOnlyList<MemorySearchResult>> SearchAsync(string query, string? category = null, int topK = 5, CancellationToken ct = default)
            => Task.FromResult((IReadOnlyList<MemorySearchResult>)Results.Take(topK).ToList());
        public Task<IReadOnlyList<MemoryEntry>> ListAsync(string? category = null, int limit = 100, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<MemoryEntry>>(new List<MemoryEntry>());
        public Task<bool> DeleteAsync(string id, CancellationToken ct = default) => Task.FromResult(true);
    }

    private static MemoryRecallRule CreateRule(FakeMemory memory, bool enabled = true, double minScore = 0.3)
    {
        var opts = new LuBanAgentOptions { Tools = new ToolGroupOptions { LocalMemory = new LocalMemoryOptions { RecallEnabled = enabled, RecallMinScore = minScore, RecallTopK = 3 } } };
        return new MemoryRecallRule(memory, Options.Create(opts));
    }

    [TestMethod]
    public async Task Execute_InjectsRelevantMemories()
    {
        var memory = new FakeMemory
        {
            Results = new List<MemorySearchResult>
            {
                new() { Content = "用户偏好简洁", Category = "preference", Score = 0.8 }
            }
        };
        var rule = CreateRule(memory);
        var result = await rule.ExecuteAsync(new RuleContext { ActionType = "context-build", UserInput = "帮我按偏好回答" });

        Assert.IsTrue(result.Allow);
        Assert.AreEqual(1, result.Inject.Count);
        StringAssert.Contains(result.Inject[0], "[记忆上下文]");
        StringAssert.Contains(result.Inject[0], "用户偏好简洁");
    }

    [TestMethod]
    public async Task Execute_BelowThreshold_NoInject()
    {
        var memory = new FakeMemory
        {
            Results = new List<MemorySearchResult> { new() { Content = "噪音", Score = 0.1 } }
        };
        var rule = CreateRule(memory, minScore: 0.3);
        var result = await rule.ExecuteAsync(new RuleContext { ActionType = "context-build", UserInput = "hi" });

        Assert.AreEqual(0, result.Inject.Count);
    }

    [TestMethod]
    public async Task Execute_Disabled_NoInject()
    {
        var rule = CreateRule(new FakeMemory(), enabled: false);
        var result = await rule.ExecuteAsync(new RuleContext { ActionType = "context-build", UserInput = "hi" });

        Assert.AreEqual(0, result.Inject.Count);
    }

    [TestMethod]
    public async Task Execute_NotContextBuild_NotApplicable()
    {
        var rule = CreateRule(new FakeMemory());
        Assert.IsFalse(rule.IsApplicable(new RuleContext { ActionType = "tool-call", Target = "read-file" }));
    }
}
