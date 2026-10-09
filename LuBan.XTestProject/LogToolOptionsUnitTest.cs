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
}