using LuBan.AIAgent.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

[TestClass]
public class LogToolOptionsUnitTest
{
    [TestMethod]
    public void Defaults_Match_Spec()
    {
        var o = new LogToolOptions();
        Assert.IsTrue(o.Enabled);
        CollectionAssert.AreEqual(new[] { "log" }, o.AggregateExtensions);
        Assert.AreEqual(1024L * 1024 * 1024, o.MaxFileSizeBytes);
        Assert.AreEqual(30_000, o.MaxDurationMs);
        Assert.AreEqual(65_536, o.MaxLineBytes);
        Assert.AreEqual(20, o.TopN);
        Assert.AreEqual(200, o.MaxDistinctSignatures);
        Assert.IsNull(o.LevelPattern);
        Assert.IsNull(o.TimestampPattern);
        Assert.AreEqual(200, o.MaxMatches);
        Assert.AreEqual(2000, o.MaxSliceLines);
        Assert.AreEqual(8000, o.MaxResultChars);
        Assert.AreEqual(5000, o.RegexMatchTimeoutMs);
        Assert.AreEqual(3, o.SampleLinesPerSignature);
    }

    [TestMethod]
    public void ApplyHardCaps_Clamps_To_Hard_Limits()
    {
        var o = new LogToolOptions
        {
            MaxFileSizeBytes = 5L * 1024 * 1024 * 1024,
            TopN = 999,
            MaxDistinctSignatures = 99999,
            MaxSliceLines = 99999
        };
        o.ApplyHardCaps();
        Assert.AreEqual(LogToolOptions.HardMaxFileSizeBytes, o.MaxFileSizeBytes);
        Assert.AreEqual(LogToolOptions.HardMaxTopN, o.TopN);
        Assert.AreEqual(LogToolOptions.HardMaxDistinctSignatures, o.MaxDistinctSignatures);
        Assert.AreEqual(LogToolOptions.HardMaxSliceLines, o.MaxSliceLines);
    }

    [TestMethod]
    public void ApplyHardCaps_Restores_NonPositive_To_Defaults_And_Is_Idempotent()
    {
        var o = new LogToolOptions { MaxFileSizeBytes = -1, TopN = 0, MaxDurationMs = 0, MaxLineBytes = 0 };
        o.ApplyHardCaps();
        Assert.AreEqual(1024L * 1024 * 1024, o.MaxFileSizeBytes);
        Assert.AreEqual(20, o.TopN);
        Assert.AreEqual(30_000, o.MaxDurationMs);
        Assert.AreEqual(65_536, o.MaxLineBytes);

        var snapshot = (o.MaxFileSizeBytes, o.TopN, o.MaxDurationMs, o.MaxLineBytes);
        o.ApplyHardCaps();
        Assert.AreEqual(snapshot, (o.MaxFileSizeBytes, o.TopN, o.MaxDurationMs, o.MaxLineBytes));
    }

    [TestMethod]
    public void ApplyHardCaps_Clamps_LineBytes_And_RegexTimeout_To_Hard_Limits()
    {
        var o = new LogToolOptions
        {
            MaxLineBytes = int.MaxValue,
            RegexMatchTimeoutMs = int.MaxValue
        };
        o.ApplyHardCaps();
        Assert.AreEqual(LogToolOptions.HardMaxLineBytes, o.MaxLineBytes);
        Assert.AreEqual(LogToolOptions.HardMaxRegexMatchTimeoutMs, o.RegexMatchTimeoutMs);
    }

    [TestMethod]
    public void ApplyHardCaps_Preserves_InRange_Values()
    {
        var o = new LogToolOptions
        {
            MaxFileSizeBytes = 1024,
            TopN = 5,
            MaxDistinctSignatures = 10,
            MaxSliceLines = 50,
            MaxLineBytes = 2048,
            RegexMatchTimeoutMs = 1000
        };
        o.ApplyHardCaps();
        Assert.AreEqual(1024L, o.MaxFileSizeBytes);
        Assert.AreEqual(5, o.TopN);
        Assert.AreEqual(10, o.MaxDistinctSignatures);
        Assert.AreEqual(50, o.MaxSliceLines);
        Assert.AreEqual(2048, o.MaxLineBytes);
        Assert.AreEqual(1000, o.RegexMatchTimeoutMs);
    }

    [TestMethod]
    public void ApplyHardCaps_Clamps_SampleLinesPerSignature()
    {
        var capped = new LogToolOptions { SampleLinesPerSignature = int.MaxValue };
        capped.ApplyHardCaps();
        Assert.AreEqual(LogToolOptions.HardMaxSampleLinesPerSignature, capped.SampleLinesPerSignature);

        var inRange = new LogToolOptions { SampleLinesPerSignature = 5 };
        inRange.ApplyHardCaps();
        Assert.AreEqual(5, inRange.SampleLinesPerSignature);
    }

    [TestMethod]
    public void IsAggregateExtension_Dot_Optional_And_Case_Insensitive()
    {
        var o = new LogToolOptions();
        Assert.IsTrue(o.IsAggregateExtension("app.log"));
        Assert.IsTrue(o.IsAggregateExtension("APP.LOG"));
        Assert.IsFalse(o.IsAggregateExtension("app.txt"));

        var custom = new LogToolOptions { AggregateExtensions = new[] { ".trace", "out" } };
        Assert.IsTrue(custom.IsAggregateExtension("x.trace"));
        Assert.IsTrue(custom.IsAggregateExtension("x.out"));
        Assert.IsFalse(custom.IsAggregateExtension("x.log"));
    }

    [TestMethod]
    public void IsAggregateExtension_Null_Or_Padded_Candidates_Safe()
    {
        var nullExt = new LogToolOptions { AggregateExtensions = null! };
        Assert.IsFalse(nullExt.IsAggregateExtension("app.log"));

        var padded = new LogToolOptions { AggregateExtensions = new[] { " .trace ", "out" } };
        Assert.IsTrue(padded.IsAggregateExtension("x.trace"));
        Assert.IsTrue(padded.IsAggregateExtension("x.out"));
        Assert.IsFalse(padded.IsAggregateExtension("x.log"));

        var noExt = new LogToolOptions();
        Assert.IsFalse(noExt.IsAggregateExtension("app"));
    }
}