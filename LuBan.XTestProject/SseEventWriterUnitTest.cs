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
*文件名： SseEventWriterUnitTest
*版本号： V1.0.0.0
*唯一标识：e6a1c4f9-3d72-4b58-9c06-5f2b8d7a0e13
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/29
*描述：SSE 事件写入与心跳泵单元测试
*
*=================================================
*修改标记
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：SSE 事件写入与心跳泵单元测试
*
*****************************************************************************/

using System.Text;

using LuBan.AIAgent.Protocol;
using LuBan.Web.Core.AspNetCore;

using Microsoft.AspNetCore.Http;

namespace LuBan.XTestProject;

[TestClass]
public class SseEventWriterUnitTest
{
    private static (SseEventWriter Writer, DefaultHttpContext Context, MemoryStream Body) Build()
    {
        var context = new DefaultHttpContext();
        var body = new MemoryStream();
        context.Response.Body = body;
        return (new SseEventWriter(context.Response, AgentEventProtocol.PayloadJsonOptions), context, body);
    }

    [TestMethod]
    public async Task StartAsync_SetsHeaders()
    {
        var (writer, context, _) = Build();

        await writer.StartAsync(CancellationToken.None);

        StringAssert.StartsWith(context.Response.ContentType!, "text/event-stream");
        Assert.AreEqual("no-cache", context.Response.Headers.CacheControl.ToString());
    }

    [TestMethod]
    public async Task WriteEventAsync_WritesNameAndCamelCaseSingleLineJson()
    {
        var (writer, _, body) = Build();
        await writer.StartAsync(CancellationToken.None);

        await writer.WriteEventAsync("text.delta", new { delta = "你好", llmRound = 1 }, CancellationToken.None);

        var text = Encoding.UTF8.GetString(body.ToArray());
        StringAssert.Contains(text, "event: text.delta\n");
        StringAssert.Contains(text, "\"delta\":\"你好\"");
        StringAssert.Contains(text, "\"llmRound\":1");
        Assert.IsFalse(text.Contains("\r"));
        Assert.AreEqual(1, CountOccurrences(text, "data: "));
    }

    [TestMethod]
    public async Task WriteCommentAsync_WritesSseComment()
    {
        var (writer, _, body) = Build();
        await writer.StartAsync(CancellationToken.None);

        await writer.WriteCommentAsync("ping", CancellationToken.None);

        StringAssert.Contains(Encoding.UTF8.GetString(body.ToArray()), ": ping");
    }

    [TestMethod]
    public async Task Pump_WritesHeartbeatWhileWaitingForNextFrame()
    {
        var (writer, _, body) = Build();
        var gate = new TaskCompletionSource();

        async IAsyncEnumerable<AgentEventFrame> Frames(
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
        {
            yield return new AgentEventFrame("session", new { sessionId = "s1", created = false });
            await gate.Task.WaitAsync(ct);
            yield return new AgentEventFrame("done", new { sessionId = "s1", elapsedMs = 1 });
        }

        var pump = SseEventPump.PumpAsync(
            Frames(), writer, TimeSpan.FromMilliseconds(50), CancellationToken.None);

        await Task.Delay(300);
        var partial = Encoding.UTF8.GetString(body.ToArray());
        StringAssert.Contains(partial, "event: session");
        StringAssert.Contains(partial, ": ping");
        Assert.IsFalse(partial.Contains("event: done"));

        gate.SetResult();
        await pump;

        var final = Encoding.UTF8.GetString(body.ToArray());
        StringAssert.Contains(final, "event: done");
    }

    [TestMethod]
    public async Task Pump_DoesNotLoseFrames_WhenFramesArriveFasterThanHeartbeat()
    {
        var (writer, _, body) = Build();

        async IAsyncEnumerable<AgentEventFrame> Frames(
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
        {
            for (var i = 0; i < 5; i++)
                yield return new AgentEventFrame("text.delta", new { delta = i.ToString(), llmRound = 1 });
            await Task.Yield();
        }

        await SseEventPump.PumpAsync(Frames(), writer, TimeSpan.FromSeconds(30), CancellationToken.None);

        var text = Encoding.UTF8.GetString(body.ToArray());
        Assert.AreEqual(5, CountOccurrences(text, "event: text.delta"));
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }
        return count;
    }
}