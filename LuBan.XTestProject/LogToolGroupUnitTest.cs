using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.LogAnalysis;
using LuBan.AIAgent.Tools.Log;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

/// <summary>测试用确认服务桩（Task 10 复用）。</summary>
internal sealed class LogTestConfirmation : IToolConfirmationService
{
    public EnumConfirmationOutcome Outcome { get; set; } = EnumConfirmationOutcome.Allowed;

    public string? LastTool { get; private set; }

    public HashSet<string> AutoConfirmTools { get; set; } = [];

    public HashSet<string> AlwaysConfirmTools { get; set; } = [];

    public HashSet<string> ReadOnlyTools { get; set; } = [];

    public Task<EnumConfirmationOutcome> EvaluateAsync(
        string toolName, string? path, IReadOnlyDictionary<string, object?> arguments)
    {
        LastTool = toolName;
        return Task.FromResult(Outcome);
    }

    public Task<bool> RequestConfirmation(string toolName, IReadOnlyDictionary<string, object?> arguments)
        => Task.FromResult(true);

    public Task<bool> TryConfirmByPath(string toolName, string path, IReadOnlyDictionary<string, object?> arguments)
        => Task.FromResult(true);

    public string FormatArguments(IReadOnlyDictionary<string, object?> arguments, int maxLength = 200)
        => string.Empty;
}

[TestClass]
public class LogToolGroupUnitTest
{
    private static LogToolGroup New(EnumConfirmationOutcome outcome = EnumConfirmationOutcome.Allowed, LogToolOptions? options = null)
    {
        var effective = options ?? new LogToolOptions();
        return new LogToolGroup(
            new LogAggregator(Options.Create(effective)),
            effective,
            new LogTestConfirmation { Outcome = outcome });
    }

    private static async Task<string> WriteTempAsync(string content)
    {
        var path = Path.GetTempFileName();
        await File.WriteAllTextAsync(path, content);
        return path;
    }

    [TestMethod]
    public async Task LogStats_Returns_Overview()
    {
        var path = await WriteTempAsync(string.Join("\n", new[] { "INFO a", "ERROR b id=1", "ERROR b id=2" }));
        try
        {
            var result = await New().LogStatsAsync(path);

            Assert.IsTrue(result.IsSuccess);
            StringAssert.Contains(result.Data, "日志概览");
            StringAssert.Contains(result.Data, "Error: 2");
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task LogGrep_Finds_Matches()
    {
        var path = await WriteTempAsync(string.Join("\n", new[] { "INFO apple", "ERROR apple boom", "INFO banana" }));
        try
        {
            var result = await New().LogGrepAsync(path, "apple");

            Assert.IsTrue(result.IsSuccess);
            StringAssert.Contains(result.Data, "命中 2 处");
            StringAssert.Contains(result.Data, ":1: INFO apple");
            StringAssert.Contains(result.Data, ":2: ERROR apple boom");
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task LogGrep_Filters_By_Level()
    {
        var path = await WriteTempAsync(string.Join("\n", new[] { "INFO apple", "ERROR apple" }));
        try
        {
            var result = await New().LogGrepAsync(path, "apple", level: "error");

            StringAssert.Contains(result.Data, "命中 1 处");
            StringAssert.Contains(result.Data, ":2:");
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task LogGrep_Includes_Context_Lines()
    {
        var path = await WriteTempAsync(string.Join("\n", new[] { "line1", "line2", "ERROR hit", "line4", "line5" }));
        try
        {
            var result = await New().LogGrepAsync(path, "hit", contextLines: 1);

            StringAssert.Contains(result.Data, "line2");
            StringAssert.Contains(result.Data, "line4");
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task LogGrep_No_Match_Returns_Message()
    {
        var path = await WriteTempAsync("INFO nothing");
        try
        {
            var result = await New().LogGrepAsync(path, "zzz");

            Assert.IsTrue(result.IsSuccess);
            StringAssert.Contains(result.Data, "未找到匹配");
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task LogSlice_Returns_Range()
    {
        var path = await WriteTempAsync(string.Join("\n", new[] { "l1", "l2", "l3", "l4" }));
        try
        {
            var result = await New().LogSliceAsync(path, startLine: 2, lineCount: 2);

            Assert.IsTrue(result.IsSuccess);
            StringAssert.Contains(result.Data, "2: l2");
            StringAssert.Contains(result.Data, "3: l3");
            Assert.IsFalse(result.Data!.Contains("4: l4"));
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task Plan_Mode_Returns_Planned()
    {
        var path = await WriteTempAsync("INFO a");
        try
        {
            var result = await New(EnumConfirmationOutcome.Planned).LogStatsAsync(path);

            Assert.IsTrue(result.Planned);
            Assert.IsFalse(result.IsSuccess);
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task Denied_Returns_Cancelled()
    {
        var path = await WriteTempAsync("INFO a");
        try
        {
            var result = await New(EnumConfirmationOutcome.Denied).LogStatsAsync(path);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.UserCancelled);
        }
        finally { File.Delete(path); }
    }
}