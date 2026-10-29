/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.XTestProject
*文件名： AttachmentEventProjectorUnitTest
*版本号： V1.0.0.0
*唯一标识：f1c8d3b6-5a27-4e90-8b41-3d7f2a6c9e05
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/29
*描述：附件事件投影纯函数单元测试
*
*=================================================
*修改标记
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：附件事件投影纯函数单元测试
*
*****************************************************************************/

using LuBan.AIAgent.Attachments;
using LuBan.AIAgent.Attachments.Ingestion;
using LuBan.AIAgent.Hosting;

using Microsoft.Extensions.AI;

namespace LuBan.XTestProject;

[TestClass]
public class AttachmentEventProjectorUnitTest
{
    private static ProcessedAttachment Text(string path, bool large) => new()
    {
        Info = new AttachmentInfo(Path.GetFileName(path), "text/plain",
                                  large ? 60 * 1024 : 10, AttachmentKind.TextFile, path),
        Content = new TextContent("x"),
        IsLargeText = large,
        Source = path
    };

    private static ProcessedAttachment Image(string path) => new()
    {
        Info = new AttachmentInfo(Path.GetFileName(path), "image/png", 10, AttachmentKind.Image, path),
        Content = new DataContent(new byte[] { 1 }, "image/png"),
        Source = path
    };

    [TestMethod]
    public void LargeText_WithTools_IsAttached()
    {
        var result = AttachmentEventProjector.Project(
            new[] { Text("big.log", large: true) }, Array.Empty<AttachmentSourceFailure>(), enableTools: true);

        Assert.AreEqual(1, result.Attached.Count);
        Assert.AreEqual(0, result.Skipped.Count);
    }

    [TestMethod]
    public void LargeText_WithoutTools_IsSkipped_With50KbReason()
    {
        var result = AttachmentEventProjector.Project(
            new[] { Text("big.log", large: true) }, Array.Empty<AttachmentSourceFailure>(), enableTools: false);

        Assert.AreEqual(0, result.Attached.Count);
        Assert.AreEqual(1, result.Skipped.Count);
        Assert.AreEqual(AttachmentEventProjector.LargeTextWithoutToolsReason, result.Skipped[0].Reason);
        StringAssert.Contains(result.Skipped[0].Reason, "50KB");
    }

    [TestMethod]
    public void LargeText_WithoutTools_KeepsSource()
    {
        var result = AttachmentEventProjector.Project(
            new[] { Text(@"C:\ws\big.log", large: true) },
            Array.Empty<AttachmentSourceFailure>(), enableTools: false);

        Assert.AreEqual(@"C:\ws\big.log", result.Skipped[0].Source);
    }

    [TestMethod]
    public void Image_NeverSkipped_EvenWithoutTools()
    {
        var result = AttachmentEventProjector.Project(
            new[] { Image("a.png") }, Array.Empty<AttachmentSourceFailure>(), enableTools: false);

        Assert.AreEqual(1, result.Attached.Count);
        Assert.AreEqual(0, result.Skipped.Count);
    }

    [TestMethod]
    public void SmallText_WithoutTools_IsAttached()
    {
        var result = AttachmentEventProjector.Project(
            new[] { Text("a.txt", large: false) }, Array.Empty<AttachmentSourceFailure>(), enableTools: false);

        Assert.AreEqual(1, result.Attached.Count);
        Assert.AreEqual(0, result.Skipped.Count);
    }

    [TestMethod]
    public void Failures_BecomeSkippedEvents_AfterAttachedSkips()
    {
        var failures = new[]
        {
            new AttachmentSourceFailure("https://x/a.pdf", "不支持的附件类型"),
            new AttachmentSourceFailure("https://x/b.png", "下载失败: HTTP 404")
        };

        var result = AttachmentEventProjector.Project(
            new[] { Text("big.log", large: true) }, failures, enableTools: false);

        Assert.AreEqual(0, result.Attached.Count);
        Assert.AreEqual(3, result.Skipped.Count);
        StringAssert.Contains(result.Skipped[0].Reason, "50KB");
        Assert.AreEqual("https://x/a.pdf", result.Skipped[1].Source);
        Assert.AreEqual("不支持的附件类型", result.Skipped[1].Reason);
        Assert.AreEqual("https://x/b.png", result.Skipped[2].Source);
    }

    [TestMethod]
    public void Empty_ProducesEmpty()
    {
        var result = AttachmentEventProjector.Project(
            Array.Empty<ProcessedAttachment>(), Array.Empty<AttachmentSourceFailure>(), enableTools: true);

        Assert.AreEqual(0, result.Attached.Count);
        Assert.AreEqual(0, result.Skipped.Count);
    }
}