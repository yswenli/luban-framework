using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Attachments;
using LuBan.AIAgent.Hosting;

using Microsoft.Extensions.AI;

namespace LuBan.XTestProject;

[TestClass]
public class AgentStreamEventUnitTest
{
    [TestMethod]
    public void TextDeltaEvent_CarriesDelta_AndDefaultsRoundToZero()
    {
        var evt = new TextDeltaEvent("hi");

        Assert.AreEqual("hi", evt.Delta);
        Assert.AreEqual(0, evt.LlmRound);
    }

    [TestMethod]
    public void LlmRound_IsInitOnly()
    {
        AgentStreamEvent evt = new ThinkingDeltaEvent("t") { LlmRound = 3 };

        Assert.AreEqual(3, evt.LlmRound);
    }

    [TestMethod]
    public void AgentRunRequest_DefaultsAreSane()
    {
        var req = new AgentRunRequest { Input = "hello" };

        Assert.AreEqual("hello", req.Input);
        Assert.AreEqual(0, req.AttachmentPaths.Count);
        Assert.IsNull(req.SessionId);
        Assert.IsNull(req.ConfirmHandler);
        Assert.AreEqual(ToolPermissionMode.Default, req.PermissionMode);
    }

    [TestMethod]
    public void WorkspaceDescriptor_IsValueEqual()
    {
        var a = new WorkspaceDescriptor("w1", "demo", @"C:\demo", "normal");
        var b = new WorkspaceDescriptor("w1", "demo", @"C:\demo", "normal");

        Assert.AreEqual(a, b);
    }

    [TestMethod]
    public void ToolConfirmationRequestedEvent_CarriesArgumentsAndTimeout()
    {
        var args = new Dictionary<string, object?> { ["path"] = "a.txt" };

        var evt = new ToolConfirmationRequestedEvent("cid-1", "DeleteFileAsync", args, 60);

        Assert.AreEqual("cid-1", evt.ConfirmationId);
        Assert.AreEqual("DeleteFileAsync", evt.ToolName);
        Assert.AreEqual("a.txt", evt.Arguments["path"]);
        Assert.AreEqual(60, evt.TimeoutSeconds);
    }

    [TestMethod]
    public void AttachmentSkippedEvent_CarriesSourceAndReason()
    {
        var evt = new AttachmentSkippedEvent("https://x/a.png", "下载失败");

        Assert.AreEqual("https://x/a.png", evt.Source);
        Assert.AreEqual("下载失败", evt.Reason);
    }

    [TestMethod]
    public void AttachmentAddedEvent_SourceIsOptional_ForBackwardCompatibility()
    {
        var oldStyle = new AttachmentAddedEvent("a.png", "image/png", 10);
        var newStyle = new AttachmentAddedEvent("a.png", "image/png", 10, "https://x/a.png");

        Assert.IsNull(oldStyle.Source);
        Assert.AreEqual("https://x/a.png", newStyle.Source);
    }

    [TestMethod]
    public void ProcessedAttachment_SourceIsSettableByInit()
    {
        var attachment = new ProcessedAttachment
        {
            Info = new AttachmentInfo("a.txt", "text/plain", 1, AttachmentKind.TextFile, @"C:\ws\a.txt"),
            Content = new TextContent("hi"),
            Source = @"C:\ws\a.txt"
        };

        Assert.AreEqual(@"C:\ws\a.txt", attachment.Source);
    }
}