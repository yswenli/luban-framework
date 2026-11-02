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
*文件名： MediaTypeUtilUnitTest
*版本号： V1.0.0.0
*唯一标识：c366bd43-5002-498b-90fe-12fd2a611d45
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/13 10:44:11
*描述：媒体类型映射与图片判定单元测试
*
*=================================================
*修改标记
*修改时间：2026/10/13 10:44:11
*修改人： yswenli
*版本号： V1.0.0.0
*描述：媒体类型映射与图片判定单元测试
*
*****************************************************************************/

using LuBan.Common.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

[TestClass]
public class MediaTypeUtilUnitTest
{
    [TestMethod]
    public void ExtensionToMediaType_DoesNotContainPdf()
    {
        Assert.IsFalse(MediaTypeUtil.ExtensionToMediaType.ContainsKey(".pdf"));
    }

    [TestMethod]
    public void ExtensionToMediaType_Has42Entries()
    {
        Assert.AreEqual(42, MediaTypeUtil.ExtensionToMediaType.Count);
    }

    [TestMethod]
    public void IsImage_RecognizesImageExtensions_CaseInsensitive()
    {
        Assert.IsTrue(MediaTypeUtil.IsImage("a.png"));
        Assert.IsTrue(MediaTypeUtil.IsImage("a.JPEG"));
        Assert.IsTrue(MediaTypeUtil.IsImage("a.tiff"));
        Assert.IsTrue(MediaTypeUtil.IsImage("a.heic"));
        Assert.IsTrue(MediaTypeUtil.IsImage("a.heif"));
    }

    [TestMethod]
    public void IsImage_ExcludesSvgPdfAndUnknown()
    {
        Assert.IsFalse(MediaTypeUtil.IsImage("a.svg"));
        Assert.IsFalse(MediaTypeUtil.IsImage("a.pdf"));
        Assert.IsFalse(MediaTypeUtil.IsImage("a.exe"));
    }

    [TestMethod]
    public void IsHeicLike_OnlyHeicAndHeif()
    {
        Assert.IsTrue(MediaTypeUtil.IsHeicLike("a.heic"));
        Assert.IsTrue(MediaTypeUtil.IsHeicLike("a.HEIF"));
        Assert.IsFalse(MediaTypeUtil.IsHeicLike("a.png"));
    }

    [TestMethod]
    public void ResolveMediaType_MapsKnownExtensions()
    {
        Assert.AreEqual("image/png", MediaTypeUtil.ResolveMediaType("x.png"));
        Assert.AreEqual("image/jpeg", MediaTypeUtil.ResolveMediaType("x.jpg"));
        Assert.AreEqual("image/svg+xml", MediaTypeUtil.ResolveMediaType("x.svg"));
        Assert.AreEqual("application/json", MediaTypeUtil.ResolveMediaType("x.json"));
    }

    [TestMethod]
    public void ResolveMediaType_DefaultsToTextPlain()
    {
        Assert.AreEqual("text/plain", MediaTypeUtil.ResolveMediaType("x.unknownext"));
    }

    [TestMethod]
    public void ResolveMediaType_HonorsDefaultParameter()
    {
        Assert.AreEqual("application/octet-stream", MediaTypeUtil.ResolveMediaType("x.unknownext", "application/octet-stream"));
        Assert.AreEqual("application/octet-stream", MediaTypeUtil.ResolveMediaType("x.pdf", "application/octet-stream"));
    }

    [TestMethod]
    public void IsImage_NullEmptyAndExtensionless_ReturnsFalse()
    {
        Assert.IsFalse(MediaTypeUtil.IsImage(null!));
        Assert.IsFalse(MediaTypeUtil.IsImage(""));
        Assert.IsFalse(MediaTypeUtil.IsImage("noextension"));
    }

    [TestMethod]
    public void IsHeicLike_Null_ReturnsFalse()
    {
        Assert.IsFalse(MediaTypeUtil.IsHeicLike(null!));
    }

    [TestMethod]
    public void ResolveMediaType_Null_ReturnsDefault()
    {
        Assert.AreEqual("text/plain", MediaTypeUtil.ResolveMediaType(null!));
        Assert.AreEqual("application/octet-stream", MediaTypeUtil.ResolveMediaType(null!, "application/octet-stream"));
    }

    [TestMethod]
    public void ImageExtensions_Has10Entries_NoSvg()
    {
        Assert.AreEqual(10, MediaTypeUtil.ImageExtensions.Count);
        Assert.IsFalse(MediaTypeUtil.ImageExtensions.Contains(".svg"));
    }

    [TestMethod]
    public void ResolveMediaType_IgnoresDotInDirectorySegment()
    {
        Assert.AreEqual("image/png", MediaTypeUtil.ResolveMediaType(@"C:\a.b\file.png"));
    }
}
