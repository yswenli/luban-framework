using LuBan.Common.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

[TestClass]
public class FileUtilFormatFileSizeUnitTest
{
    [TestMethod]
    public void FormatFileSize_Negative_ReturnsZeroB()
    {
        Assert.AreEqual("0B", FileUtil.FormatFileSize(-1));
    }

    [TestMethod]
    public void FormatFileSize_Bytes()
    {
        Assert.AreEqual("0B", FileUtil.FormatFileSize(0));
        Assert.AreEqual("1023B", FileUtil.FormatFileSize(1023));
    }

    [TestMethod]
    public void FormatFileSize_Kilobytes()
    {
        Assert.AreEqual("1.0KB", FileUtil.FormatFileSize(1024));
        Assert.AreEqual("1.5KB", FileUtil.FormatFileSize(1536));
    }

    [TestMethod]
    public void FormatFileSize_Megabytes()
    {
        Assert.AreEqual("1.0MB", FileUtil.FormatFileSize(1024 * 1024));
        Assert.AreEqual("2.0MB", FileUtil.FormatFileSize(2 * 1024 * 1024));
    }

    [TestMethod]
    public void FormatFileSize_KilobyteUpperBoundary_RoundsTo10240KB()
    {
        Assert.AreEqual("1024.0KB", FileUtil.FormatFileSize(1048575));
    }

    [TestMethod]
    public void FormatFileSize_LongMaxValue_DoesNotThrowAndEndsWithMb()
    {
        Assert.IsTrue(FileUtil.FormatFileSize(long.MaxValue).EndsWith("MB"));
    }
}
