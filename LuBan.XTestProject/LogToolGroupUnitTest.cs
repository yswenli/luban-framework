using System.Text;
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

    [TestMethod]
    public async Task LogGrep_Output_Capped_At_MaxResultChars()
    {
        var lines = Enumerable.Range(0, 50).Select(i => $"INFO match_{i}_" + new string('x', 40));
        var path = await WriteTempAsync(string.Join("\n", lines));
        try
        {
            var result = await New(options: new LogToolOptions { MaxResultChars = 200 }).LogGrepAsync(path, "match");

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Data!.Length <= 200, $"长度 {result.Data.Length}");
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task LogGrep_Clamps_ContextLines()
    {
        var lines = Enumerable.Range(1, 300).Select(i => i == 150 ? "ERROR hit" : $"line{i}");
        var path = await WriteTempAsync(string.Join("\n", lines));
        try
        {
            var result = await New(options: new LogToolOptions { MaxResultChars = 100_000 })
                .LogGrepAsync(path, "hit", contextLines: 1_000_000);

            Assert.IsTrue(result.IsSuccess);
            StringAssert.Contains(result.Data, "line50");
            Assert.IsFalse(result.Data!.Contains("line49\n"), "上下文应被钳制到硬上限");
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task LogGrep_No_Truncation_Marker_When_Hits_Equal_Max()
    {
        var path = await WriteTempAsync(string.Join("\n", new[] { "INFO hit", "INFO hit" }));
        try
        {
            var result = await New().LogGrepAsync(path, "hit", maxMatches: 2);

            StringAssert.Contains(result.Data, "命中 2 处");
            Assert.IsFalse(result.Data!.Contains("已达上限"));
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task LogGrep_Marks_Truncation_When_More_Hits_Remain()
    {
        var path = await WriteTempAsync(string.Join("\n", new[] { "INFO hit", "INFO hit", "INFO hit" }));
        try
        {
            var result = await New().LogGrepAsync(path, "hit", maxMatches: 2);

            StringAssert.Contains(result.Data, "命中 2 处");
            StringAssert.Contains(result.Data, "已达上限");
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task LogGrep_Trailing_Context_Emitted_At_Cap()
    {
        var path = await WriteTempAsync(string.Join("\n", new[] { "line1", "line2", "ERROR hit", "line4", "line5" }));
        try
        {
            var result = await New().LogGrepAsync(path, "hit", maxMatches: 1, contextLines: 1);

            StringAssert.Contains(result.Data, "line4");
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task LogGrep_Times_Out_Returns_Partial()
    {
        var path = Path.GetTempFileName();
        try
        {
            await using (var writer = new StreamWriter(path, false, new UTF8Encoding(false)))
            {
                for (var i = 0; i < 200_000; i++)
                    await writer.WriteLineAsync($"2026-01-01 00:00:00 INFO 处理第 {i} 条记录");
            }

            var result = await New(options: new LogToolOptions { MaxDurationMs = 1 }).LogGrepAsync(path, "zzz");

            Assert.IsTrue(result.IsSuccess);
            StringAssert.Contains(result.Data, "时长上限");
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task LogSlice_Output_Capped()
    {
        var lines = Enumerable.Range(0, 1000).Select(i => $"line{i}_" + new string('y', 500));
        var path = await WriteTempAsync(string.Join("\n", lines));
        try
        {
            var result = await New(options: new LogToolOptions { MaxSliceLines = 1000 }).LogSliceAsync(path, 1, 1000);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Data!.Length <= 256 * 1024 + 100, $"长度 {result.Data.Length}");
        }
        finally { File.Delete(path); }
    }
}