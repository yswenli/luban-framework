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
}