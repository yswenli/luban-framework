using System.Text;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.LogAnalysis;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

[TestClass]
public class LogAggregatorUnitTest
{
    private static LogAggregator New(LogToolOptions? options = null)
        => new(Options.Create(options ?? new LogToolOptions()));

    private static async Task<string> WriteTempAsync(string content)
    {
        var path = Path.GetTempFileName();
        await File.WriteAllTextAsync(path, content);
        return path;
    }

    [TestMethod]
    public async Task Counts_Levels_Timestamps_And_TopErrors()
    {
        var path = await WriteTempAsync(string.Join("\n", new[]
        {
            "2026-01-01 00:00:01 INFO start",
            "2026-01-01 00:00:02 INFO work",
            "2026-01-01 00:00:03 WARN slow id=123",
            "2026-01-01 00:00:04 ERROR failed id=456",
            "2026-01-01 00:00:05 ERROR failed id=789",
            "2026-01-01 00:00:06 FATAL crash",
        }));
        try
        {
            var summary = await New().AggregateAsync(path);

            Assert.AreEqual(6L, summary.TotalLines);
            Assert.AreEqual(2L, summary.LevelCounts[LogSeverity.Info]);
            Assert.AreEqual(1L, summary.LevelCounts[LogSeverity.Warn]);
            Assert.AreEqual(2L, summary.LevelCounts[LogSeverity.Error]);
            Assert.AreEqual(1L, summary.LevelCounts[LogSeverity.Fatal]);
            Assert.IsFalse(summary.Truncated);
            Assert.AreEqual(new DateTimeOffset(2026, 1, 1, 0, 0, 1, TimeSpan.Zero), summary.FirstTimestamp);
            Assert.AreEqual(new DateTimeOffset(2026, 1, 1, 0, 0, 6, TimeSpan.Zero), summary.LastTimestamp);

            var error = summary.TopErrors.Single(e => e.Template.Contains("failed"));
            Assert.AreEqual(2L, error.Count);
            CollectionAssert.AreEqual(new long[] { 4, 5 }, error.SampleLineNumbers.ToArray());
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task Sample_Lines_Are_Capped()
    {
        var path = await WriteTempAsync(string.Join("\n", new[]
        {
            "ERROR boom id=1",
            "ERROR boom id=2",
            "ERROR boom id=3",
            "ERROR boom id=4",
        }));
        try
        {
            var summary = await New().AggregateAsync(path);
            var signature = summary.TopErrors.Single();

            Assert.AreEqual(4L, signature.Count);
            Assert.AreEqual(3, signature.SampleLineNumbers.Count);
            CollectionAssert.AreEqual(new long[] { 1, 2, 3 }, signature.SampleLineNumbers.ToArray());
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task Overflow_Signatures_Go_To_OtherErrorCount()
    {
        var path = await WriteTempAsync(string.Join("\n", new[]
        {
            "ERROR distinct_one xyz",
            "ERROR distinct_two xyz",
            "ERROR distinct_three xyz",
        }));
        try
        {
            var summary = await New(new LogToolOptions { MaxDistinctSignatures = 2 }).AggregateAsync(path);

            Assert.AreEqual(2, summary.TopErrors.Count);
            Assert.AreEqual(1L, summary.OtherErrorCount);
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task Missing_File_Throws()
    {
        var agg = New();
        var missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".log");
        await Assert.ThrowsExactlyAsync<FileNotFoundException>(() => agg.AggregateAsync(missing));
    }

    [TestMethod]
    public async Task Oversize_File_Returns_Truncated_Size_Without_Scanning()
    {
        var path = await WriteTempAsync("ERROR something\n");
        try
        {
            var summary = await New(new LogToolOptions { MaxFileSizeBytes = 1 }).AggregateAsync(path);

            Assert.IsTrue(summary.Truncated);
            Assert.AreEqual("size", summary.TruncationReason);
            Assert.AreEqual(0L, summary.TotalLines);
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task Aggregate_Is_Constant_Memory_On_Large_File()
    {
        var path = Path.GetTempFileName();
        try
        {
            await using (var writer = new StreamWriter(path, false, new UTF8Encoding(false)))
            {
                for (var i = 0; i < 200_000; i++)
                    await writer.WriteLineAsync($"2026-01-01 00:00:{i % 60:D2} INFO 处理第 {i} 条记录 value={i}");
            }

            var before = GC.GetTotalMemory(forceFullCollection: true);
            var summary = await New().AggregateAsync(path);
            var after = GC.GetTotalMemory(forceFullCollection: true);

            Assert.AreEqual(200_000L, summary.TotalLines);
            var delta = Math.Max(0, after - before);
            Assert.IsTrue(delta < 64L * 1024 * 1024, $"内存增长过大：{delta} 字节");
        }
        finally { File.Delete(path); }
    }
}
