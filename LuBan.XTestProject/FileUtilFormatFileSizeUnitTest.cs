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
*文件名： FileUtilFormatFileSizeUnitTest
*版本号： V1.0.0.0
*唯一标识：65202a88-3ff1-4691-8542-2ad8c0bcc34e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/13 10:53:53
*描述：FileUtil 文件大小格式化单元测试
*
*=================================================
*修改标记
*修改时间：2026/10/13 10:53:53
*修改人： yswenli
*版本号： V1.0.0.0
*描述：FileUtil 文件大小格式化单元测试
*
*****************************************************************************/

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
