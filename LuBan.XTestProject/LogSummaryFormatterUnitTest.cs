using LuBan.AIAgent.LogAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

[TestClass]
public class LogSummaryFormatterUnitTest
{
    private static LogSummary Summary(params ErrorSignature[] errors) => new(
        "app.log",
        1024L * 1024 * 5,
        100,
        false,
        null,
        new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        new DateTimeOffset(2026, 1, 1, 0, 5, 0, TimeSpan.Zero),
        new Dictionary<LogSeverity, long> { [LogSeverity.Info] = 90, [LogSeverity.Error] = 10 },
        errors,
        0);

    [TestMethod]
    public void Renders_Header_Levels_And_Errors()
    {
        var text = LogSummaryFormatter.Format(
            Summary(new ErrorSignature("ERROR failed id=<N>", 7, new long[] { 4, 5, 6 })),
            8000);

        StringAssert.Contains(text, "app.log");
        StringAssert.Contains(text, "已处理行数：100");
        StringAssert.Contains(text, "Info: 90");
        StringAssert.Contains(text, "Error: 10");
        StringAssert.Contains(text, "×7");
        StringAssert.Contains(text, "ERROR failed id=<N>");
        StringAssert.Contains(text, "4, 5, 6");
    }

    [TestMethod]
    public void Marks_Truncated_And_OtherErrors()
    {
        var summary = Summary() with { Truncated = true, TruncationReason = "timeout", OtherErrorCount = 5 };
        var text = LogSummaryFormatter.Format(summary, 8000);

        StringAssert.Contains(text, "timeout");
        StringAssert.Contains(text, "未归类");
    }

    [TestMethod]
    public void Caps_At_MaxResultChars()
    {
        var errors = Enumerable.Range(0, 200)
            .Select(i => new ErrorSignature($"ERROR template_{i}_<N>", 10, new long[] { i }))
            .ToArray();

        var text = LogSummaryFormatter.Format(Summary(errors), 500);

        Assert.IsTrue(text.Length <= 500 + 64);
        StringAssert.Contains(text, "已截断");
    }
}
