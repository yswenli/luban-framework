using Cronos;
using LuBan.Service.Core;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

[TestClass]
public class CronJobUnitTest
{
    [TestMethod]
    [DataRow(1000, "*/1 * * * * *")]
    [DataRow(5000, "*/5 * * * * *")]
    [DataRow(10000, "*/10 * * * * *")]
    [DataRow(60000, "0 */1 * * * *")]
    [DataRow(300000, "0 */5 * * * *")]
    [DataRow(1800000, "0 */30 * * * *")]
    [DataRow(3600000, "0 0 */1 * * *")]
    [DataRow(86400000, "0 0 0 * * *")]
    public void MapIntervalToCron_Valid(int ms, string expected)
    {
        Assert.AreEqual(expected, BaseBackgroundService.MapIntervalToCron(ms));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(500)]
    [DataRow(7000)]
    [DataRow(11000)]
    [DataRow(1500)]
    [DataRow(59000)]
    [DataRow(70000)]
    [DataRow(90000)]
    [DataRow(3660000)]
    [DataRow(172800000)]
    public void MapIntervalToCron_NonMappable_ReturnsNull(int ms)
    {
        Assert.IsNull(BaseBackgroundService.MapIntervalToCron(ms));
    }

    [TestMethod]
    public void Constructor_Interval_10s_SetsCron()
    {
        var job = new TestCronJob(10000);
        Assert.AreEqual("*/10 * * * * *", job.Cron);
    }

    [TestMethod]
    public void Constructor_Interval_5m_SetsCron()
    {
        var job = new TestCronJob(300000);
        Assert.AreEqual("0 */5 * * * *", job.Cron);
    }

    [TestMethod]
    public void Constructor_Interval_NonMappable_NoCron()
    {
        var job = new TestCronJob(1500);
        Assert.IsNull(job.Cron);
    }

    [TestMethod]
    public void Constructor_TimePoint_SetsCron()
    {
        var job = new TestCronJob(2, 30, 0);
        Assert.AreEqual("0 30 2 * * *", job.Cron);
    }

    [TestMethod]
    public void Constructor_String_HHmmss_SetsCron()
    {
        var job = new TestCronJob("02:30:00");
        Assert.AreEqual("0 30 2 * * *", job.Cron);
    }

    [TestMethod]
    public void Constructor_String_Cron_SetsCron()
    {
        var job = new TestCronJob("0 0 8 * * 1");
        Assert.AreEqual("0 0 8 * * 1", job.Cron);
    }

    [TestMethod]
    public void Constructor_String_InvalidFormat_Throws()
    {
        Assert.Throws<ArgumentException>(() => new TestCronJob("abc"));
    }

    [TestMethod]
    public void Constructor_String_InvalidCron_Throws()
    {
        Assert.Throws<ArgumentException>(() => new TestCronJob("99 99 99 99 99 99"));
    }

    [TestMethod]
    public void GetNextOccurrence_ReturnsFutureTime()
    {
        var job = new TestCronJob("0 0 8 * * *");
        var next = job.GetNextOccurrence();
        Assert.IsNotNull(next);
        Assert.IsTrue(next > DateTime.Now);
    }

    [TestMethod]
    public void GetNextOccurrence_NullCron_ReturnsNull()
    {
        var job = new TestCronJob(1500);
        Assert.IsNull(job.GetNextOccurrence());
    }

    [TestMethod]
    public void SetCron_ValidExpression_UpdatesCron()
    {
        var job = new TestCronJob(10000);
        Assert.AreEqual("*/10 * * * * *", job.Cron);
        job.SetCron("0 0 12 * * *");
        Assert.AreEqual("0 0 12 * * *", job.Cron);
    }

    [TestMethod]
    public void SetCron_InvalidExpression_Throws()
    {
        var job = new TestCronJob(10000);
        Assert.Throws<ArgumentException>(() => job.SetCron("invalid"));
    }

    [TestMethod]
    public void SetCron_Null_ClearsCron()
    {
        var job = new TestCronJob(10000);
        job.SetCron(null);
        Assert.IsNull(job.Cron);
    }

    [TestMethod]
    public void Cron_Property_Setter_DelegatesToSetCron()
    {
        var job = new TestCronJob(10000);
        job.Cron = "0 0 18 * * *";
        Assert.AreEqual("0 0 18 * * *", job.Cron);
    }

    [TestMethod]
    public void CronExpression_Parse_6Segment_Works()
    {
        var expr = CronExpression.Parse("0 */5 * * * *", CronFormat.IncludeSeconds);
        Assert.IsNotNull(expr);
        var next = expr.GetNextOccurrence(DateTimeOffset.Now, TimeZoneInfo.Local);
        Assert.IsNotNull(next);
    }

    [TestMethod]
    public void CronExpression_Parse_5Segment_WithIncludeSeconds_Throws()
    {
        Assert.Throws<CronFormatException>(() =>
            CronExpression.Parse("*/5 * * * *", CronFormat.IncludeSeconds));
    }

    [TestMethod]
    public void Constructor_Interval_1m_SetsCron()
    {
        var job = new TestCronJob(60000);
        Assert.AreEqual("0 */1 * * * *", job.Cron);
    }

    [TestMethod]
    public void Constructor_Interval_1h_SetsCron()
    {
        var job = new TestCronJob(3600000);
        Assert.AreEqual("0 0 */1 * * *", job.Cron);
    }

    [TestMethod]
    public void Constructor_Once_Cron()
    {
        var job = new TestCronJob(2, 30, 0, once: true);
        Assert.AreEqual("0 30 2 * * *", job.Cron);
    }
}

public class TestCronJob : BaseBackgroundService
{
    public TestCronJob(int intervalTime, bool sequentially = true, bool userLog = false)
        : base(intervalTime, sequentially, userLog) { }

    public TestCronJob(int hour, int minute, int second, bool once = false, bool sequentially = true, bool userLog = false)
        : base(hour, minute, second, once, sequentially, userLog) { }

    public TestCronJob(string hourMinuteSeconds, bool once = false, bool sequentially = true, bool userLog = false)
        : base(hourMinuteSeconds, once, sequentially, userLog) { }
}
