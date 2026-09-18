using LuBan.AIAgent.Attachments;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

[TestClass]
public class AttachmentProcessorUnitTest
{
    private static DefaultAttachmentProcessor NewProcessor() => new();

    [TestMethod]
    public void IsSupported_RecognizesImageAndTextExtensions()
    {
        var p = NewProcessor();
        Assert.IsTrue(p.IsSupported("a.png"));
        Assert.IsTrue(p.IsSupported("a.JPEG"));
        Assert.IsTrue(p.IsSupported("a.cs"));
        Assert.IsTrue(p.IsSupported("a.svg"));
        Assert.IsFalse(p.IsSupported("a.exe"));
        Assert.IsFalse(p.IsSupported("a.pdf"));
    }

    [TestMethod]
    public void ResolveMediaType_MapsKnownExtensions()
    {
        var p = NewProcessor();
        Assert.AreEqual("image/png", p.ResolveMediaType("x.png"));
        Assert.AreEqual("image/jpeg", p.ResolveMediaType("x.jpg"));
        Assert.AreEqual("image/svg+xml", p.ResolveMediaType("x.svg"));
        Assert.AreEqual("application/json", p.ResolveMediaType("x.json"));
        Assert.AreEqual("text/plain", p.ResolveMediaType("x.unknownext"));
    }
}