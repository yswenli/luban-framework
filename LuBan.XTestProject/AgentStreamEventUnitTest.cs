using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Hosting;

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
}