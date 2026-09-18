using LuBan.AIAgent.Attachments;
using Microsoft.Extensions.AI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

[TestClass]
public class AttachmentMessageBuilderUnitTest
{
    [TestMethod]
    public void Build_InlinesSmallTextAndKeepsImageAsDataContent()
    {
        var text = new ProcessedAttachment
        {
            Info = new AttachmentInfo("a.cs", "text/plain", 8, AttachmentKind.TextFile, "/tmp/a.cs"),
            Content = new TextContent("var x=1;")
        };
        var img = new ProcessedAttachment
        {
            Info = new AttachmentInfo("b.png", "image/png", 3, AttachmentKind.Image, "/tmp/b.png"),
            Content = new DataContent(new byte[] { 1, 2, 3 }, "image/png")
        };

        var msg = AttachmentMessageBuilder.Build("看看这两个", new[] { text, img });

        Assert.AreEqual(ChatRole.User, msg.Role);
        Assert.AreEqual(2, msg.Contents.Count);
        var head = (TextContent)msg.Contents[0];
        Assert.IsTrue(head.Text!.Contains("看看这两个"));
        Assert.IsTrue(head.Text!.Contains("--- 附件 a.cs ---"));
        Assert.IsTrue(head.Text!.Contains("var x=1;"));
        Assert.IsInstanceOfType(msg.Contents[1], typeof(DataContent));
    }

    [TestMethod]
    public void Build_LargeTextInjectsGuideOnly()
    {
        var big = new ProcessedAttachment
        {
            Info = new AttachmentInfo("big.log", "text/plain", 999999, AttachmentKind.TextFile, "/tmp/big.log"),
            Content = new TextContent("请使用 ReadFileAsync 读取 /tmp/big.log"),
            IsLargeText = true
        };

        var msg = AttachmentMessageBuilder.Build("摘要一下", new[] { big });

        Assert.AreEqual(1, msg.Contents.Count);
        var head = (TextContent)msg.Contents[0];
        Assert.IsTrue(head.Text!.Contains("ReadFileAsync"));
        Assert.IsFalse(msg.Contents.OfType<DataContent>().Any());
    }
}
