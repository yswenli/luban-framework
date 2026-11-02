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
*文件名： LogPatternsUnitTest
*版本号： V1.0.0.0
*唯一标识：28f4dbea-2a62-4110-ba28-ad639c4b0527
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/9 17:34:38
*描述：日志级别与时间戳解析模式单元测试
*
*=================================================
*修改标记
*修改时间：2026/10/9 17:34:38
*修改人： yswenli
*版本号： V1.0.0.0
*描述：日志级别与时间戳解析模式单元测试
*
*****************************************************************************/

using LuBan.AIAgent.LogAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

[TestClass]
public class LogPatternsUnitTest
{
    private static LogPatterns New() => new(null, null, 5000);

    [TestMethod]
    [DataRow("2026-01-02 03:04:05 INFO hello", LogSeverity.Info)]
    [DataRow("12:00:00 DEBUG x", LogSeverity.Debug)]
    [DataRow("WARN something", LogSeverity.Warn)]
    [DataRow("WARNING something", LogSeverity.Warn)]
    [DataRow("ERROR boom", LogSeverity.Error)]
    [DataRow("FATAL boom", LogSeverity.Fatal)]
    [DataRow("CRITICAL boom", LogSeverity.Fatal)]
    [DataRow("TRACE t", LogSeverity.Trace)]
    [DataRow("INFORMATION info", LogSeverity.Info)]
    [DataRow("no level here", LogSeverity.Unknown)]
    public void ParseLevel_Default_Pattern(string line, LogSeverity expected)
        => Assert.AreEqual(expected, New().ParseLevel(line));

    [TestMethod]
    public void ParseLevel_Is_Case_Insensitive()
        => Assert.AreEqual(LogSeverity.Error, New().ParseLevel("error happened"));

    [TestMethod]
    public void ParseLevel_Uses_Custom_Named_Group()
    {
        var p = new LogPatterns(@"SEV=(?<lvl>info|error)", null, 5000);
        Assert.AreEqual(LogSeverity.Error, p.ParseLevel("SEV=error x"));
        Assert.AreEqual(LogSeverity.Info, p.ParseLevel("SEV=info x"));
    }

    [TestMethod]
    public void ParseLevel_Falls_Back_On_Invalid_Custom_Pattern()
    {
        var p = new LogPatterns("(?<lvl>", null, 5000);
        Assert.AreEqual(LogSeverity.Warn, p.ParseLevel("WARN x"));
    }

    [TestMethod]
    public void TryParseTimestamp_Parses_Common_Formats()
    {
        var p = New();

        Assert.IsTrue(p.TryParseTimestamp("2026-01-02 03:04:05.123 INFO x", out var withSpace));
        Assert.AreEqual(TimeSpan.Zero, withSpace.Offset);
        Assert.AreEqual(
            new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero).AddMilliseconds(123),
            withSpace);

        Assert.IsTrue(p.TryParseTimestamp("2026-01-02T03:04:05Z ERROR x", out var withZ));
        Assert.AreEqual(TimeSpan.Zero, withZ.Offset);
        Assert.AreEqual(new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero), withZ);

        Assert.IsTrue(p.TryParseTimestamp("2026-01-02T03:04:05,500+08:00 x", out var withOffset));
        Assert.AreEqual(TimeSpan.Zero, withOffset.Offset);
        Assert.AreEqual(
            new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.FromHours(8)).AddMilliseconds(500),
            withOffset);
    }

    [TestMethod]
    public void TryParseTimestamp_Returns_False_When_Absent()
        => Assert.IsFalse(New().TryParseTimestamp("no timestamp", out _));
}