/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.XTestProject
*文件名： LogSummaryFormatterUnitTest
*版本号： V1.0.0.0
*唯一标识：922bb56e-7072-48a9-a00d-6f4a9bed3ca6
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/9 18:14:18
*描述：日志摘要格式化单元测试（截断标记与不变文化）
*
*=================================================
*修改标记
*修改时间：2026/10/9 18:14:18
*修改人： yswenli
*版本号： V1.0.0.0
*描述：日志摘要格式化单元测试（截断标记与不变文化）
*
*****************************************************************************/

using System.Globalization;
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

        Assert.IsTrue(text.Length <= 500);
        StringAssert.Contains(text, "已截断");
    }

    [TestMethod]
    public void Renders_NoError_Placeholder_When_No_Errors()
    {
        var text = LogSummaryFormatter.Format(Summary(), 8000);
        StringAssert.Contains(text, "（无 ERROR/FATAL 行）");
    }

    [TestMethod]
    public void Renders_Unknown_Time_When_Timestamps_Missing()
    {
        var summary = Summary() with { FirstTimestamp = null, LastTimestamp = null };
        var text = LogSummaryFormatter.Format(summary, 8000);
        StringAssert.Contains(text, "(未识别)");
    }

    [TestMethod]
    public void Uses_Invariant_Culture_For_Size()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            var text = LogSummaryFormatter.Format(Summary(), 8000);
            StringAssert.Contains(text, "5.00 MB");
        }
        finally { CultureInfo.CurrentCulture = original; }
    }
}
