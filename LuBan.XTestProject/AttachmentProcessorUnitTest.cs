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

    [TestMethod]
    public async Task ProcessAsync_Image_ProducesDataContent_WithImageMediaType()
    {
        var p = NewProcessor();
        var path = TestImageFactory.CreatePng(64, 48);
        try
        {
            var result = await p.ProcessAsync(path);
            Assert.AreEqual(AttachmentKind.Image, result.Info.Kind);
            Assert.IsInstanceOfType(result.Content, typeof(Microsoft.Extensions.AI.DataContent));
            var data = (Microsoft.Extensions.AI.DataContent)result.Content;
            StringAssert.StartsWith(data.MediaType, "image/");
            Assert.AreEqual("64x48", result.PixelSize);
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task ProcessAsync_Heic_PromptsConversion()
    {
        var p = NewProcessor();
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".heic");
        await File.WriteAllBytesAsync(path, new byte[] { 0, 0, 0, 0x18, 0x66, 0x74, 0x79, 0x70 });
        try
        {
            await Assert.ThrowsExactlyAsync<NotSupportedException>(() => p.ProcessAsync(path));
        }
        finally { File.Delete(path); }
    }
}