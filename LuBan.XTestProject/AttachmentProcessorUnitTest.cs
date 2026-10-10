using LuBan.AIAgent.Attachments;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.LogAnalysis;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace LuBan.XTestProject;

[TestClass]
public class AttachmentProcessorUnitTest
{
    private static DefaultAttachmentProcessor NewProcessor(LogToolOptions? logOptions = null)
    {
        var options = logOptions ?? new LogToolOptions();
        return new DefaultAttachmentProcessor(
            Options.Create(options),
            new LogAggregator(Options.Create(options)));
    }

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

    [TestMethod]
    public async Task ProcessAsync_SmallNonPngImage_KeepsOriginalMediaType()
    {
        // 回归：原样返回原始字节（未缩放、未超限）时，媒体类型必须与载荷真实格式一致
        var p = NewProcessor();
        var path = TestImageFactory.CreateGif(32, 32);
        try
        {
            var result = await p.ProcessAsync(path);
            Assert.AreEqual(AttachmentKind.Image, result.Info.Kind);
            Assert.AreEqual("image/gif", result.Info.MediaType);
            var data = (Microsoft.Extensions.AI.DataContent)result.Content;
            Assert.AreEqual("image/gif", data.MediaType);
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task ProcessAsync_SmallText_InlinesContent()
    {
        var p = NewProcessor();
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".cs");
        await File.WriteAllTextAsync(path, "class A { }");
        try
        {
            var r = await p.ProcessAsync(path);
            Assert.AreEqual(AttachmentKind.TextFile, r.Info.Kind);
            Assert.IsFalse(r.IsLargeText);
            Assert.AreEqual("class A { }", ((Microsoft.Extensions.AI.TextContent)r.Content).Text);
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task ProcessAsync_LargeNonLogText_EmitsGuidanceWithReadFileAsync()
    {
        var p = NewProcessor();
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".txt");
        await File.WriteAllTextAsync(path, new string('x', 60 * 1024));
        try
        {
            var r = await p.ProcessAsync(path);
            Assert.IsTrue(r.IsLargeText);
            var text = ((Microsoft.Extensions.AI.TextContent)r.Content).Text;
            StringAssert.Contains(text, "ReadFileAsync");
            StringAssert.Contains(text, path);
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task ProcessAsync_NonLogTextOver10Mb_Throws()
    {
        var p = NewProcessor();
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".txt");
        await using (var fs = new FileStream(path, FileMode.CreateNew))
            fs.SetLength(11 * 1024 * 1024);
        try
        {
            await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => p.ProcessAsync(path));
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task ProcessAsync_SmallLog_InlinesContent()
    {
        var p = NewProcessor();
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".log");
        await File.WriteAllTextAsync(path, "2026-10-09 INFO small\n");
        try
        {
            var r = await p.ProcessAsync(path);
            Assert.IsFalse(r.IsLargeText);
            StringAssert.Contains(((Microsoft.Extensions.AI.TextContent)r.Content).Text, "small");
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task ProcessAsync_LargeLog_EmitsAggregatedOverview()
    {
        var p = NewProcessor();
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".log");
        var sb = new StringBuilder();
        for (int i = 0; i < 3000; i++)
            sb.AppendLine($"2026-10-09T10:00:00 INFO ok {i}");
        sb.AppendLine("2026-10-09T10:01:00 ERROR database connection failed id=1");
        sb.AppendLine("2026-10-09T10:01:01 ERROR database connection failed id=2");
        await File.WriteAllTextAsync(path, sb.ToString());
        try
        {
            var r = await p.ProcessAsync(path);
            Assert.AreEqual(AttachmentKind.TextFile, r.Info.Kind);
            Assert.IsTrue(r.IsLargeText);
            var text = ((Microsoft.Extensions.AI.TextContent)r.Content).Text;
            StringAssert.Contains(text, "# 日志概览");
            StringAssert.Contains(text, "database connection failed");
            StringAssert.Contains(text, "LogGrepAsync");
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task ProcessAsync_LogOverAggregateCap_EmitsGuideOnly()
    {
        var options = new LogToolOptions { MaxFileSizeBytes = 1024 };
        var p = NewProcessor(options);
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".log");
        await File.WriteAllTextAsync(path, new string('x', 60 * 1024));
        try
        {
            var r = await p.ProcessAsync(path);
            Assert.IsTrue(r.IsLargeText);
            var text = ((Microsoft.Extensions.AI.TextContent)r.Content).Text;
            StringAssert.Contains(text, "LogSliceAsync");
            Assert.IsFalse(text.Contains("# 日志概览"));
        }
        finally { File.Delete(path); }
    }
}
