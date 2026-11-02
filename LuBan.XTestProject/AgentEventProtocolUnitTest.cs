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
*文件名： AgentEventProtocolUnitTest
*版本号： V1.0.0.0
*唯一标识：c5d2f8a1-6e04-4b79-a3c8-7f1e9d0b2a56
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/29
*描述：事件帧映射单元测试
*
*=================================================
*修改标记
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：事件帧映射单元测试
*
*****************************************************************************/

using LuBan.AIAgent.Hosting;
using LuBan.AIAgent.Orchestration.Models;
using LuBan.AIAgent.Protocol;

namespace LuBan.XTestProject;

[TestClass]
public class AgentEventProtocolUnitTest
{
    [TestMethod]
    public void TextDelta_MapsToCamelCaseName_WithDeltaAndRound()
    {
        var frame = AgentEventProtocol.ToFrame(new TextDeltaEvent("hi") { LlmRound = 2 });

        Assert.IsNotNull(frame);
        Assert.AreEqual("text.delta", frame!.Name);
        var payload = AgentEventProtocol.SerializePayload(frame.Payload);
        StringAssert.Contains(payload, "\"delta\":\"hi\"");
        StringAssert.Contains(payload, "\"llmRound\":2");
    }

    [TestMethod]
    public void ThinkingDelta_MapsToThinkDelta()
    {
        var frame = AgentEventProtocol.ToFrame(new ThinkingDeltaEvent("t"));

        Assert.AreEqual("think.delta", frame!.Name);
    }

    [TestMethod]
    public void ToolEvents_MapToDottedNames()
    {
        Assert.AreEqual("tool.started", AgentEventProtocol.ToFrame(
            new ToolCallStartedEvent("DeleteFileAsync", "call-1", new Dictionary<string, object?>()))!.Name);
        Assert.AreEqual("tool.completed", AgentEventProtocol.ToFrame(
            new ToolCallCompletedEvent("call-1", "ok"))!.Name);
        Assert.AreEqual("tool.failed", AgentEventProtocol.ToFrame(
            new ToolCallFailedEvent("call-1", "boom"))!.Name);
    }

    [TestMethod]
    public void ConfirmEvent_MapsNameAndPayload()
    {
        var frame = AgentEventProtocol.ToFrame(new ToolConfirmationRequestedEvent(
            "cid", "DeleteFileAsync", new Dictionary<string, object?> { ["path"] = "a" }, 60));

        Assert.AreEqual("tool.confirm", frame!.Name);
        var payload = AgentEventProtocol.SerializePayload(frame.Payload);
        StringAssert.Contains(payload, "\"confirmationId\":\"cid\"");
        StringAssert.Contains(payload, "\"timeoutSeconds\":60");
    }

    [TestMethod]
    public void AttachmentEvents_MapAddedAndSkipped()
    {
        Assert.AreEqual("attachment.added", AgentEventProtocol.ToFrame(
            new AttachmentAddedEvent("a.png", "image/png", 1, "https://x/a.png"))!.Name);
        Assert.AreEqual("attachment.skipped", AgentEventProtocol.ToFrame(
            new AttachmentSkippedEvent("https://x/b.pdf", "不支持的附件类型"))!.Name);
    }

    [TestMethod]
    public void ErrorEvent_MapsToError()
    {
        Assert.AreEqual("error", AgentEventProtocol.ToFrame(new ErrorEvent("bad"))!.Name);
    }

    [TestMethod]
    public void OrchestrationProgress_IsIgnored()
    {
        var evt = new OrchestrationProgressEvent(ProgressEventType.PlanningStarted, null, "x", null, null);

        Assert.IsNull(AgentEventProtocol.ToFrame(evt));
    }
}