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
*文件名： AgentRunRequestValidationUnitTest
*版本号： V1.0.0.0
*唯一标识：7c235fa1-2fd2-47c1-a95b-16829c629cc8
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/26 15:05:23
*描述：AgentRunRequest 参数校验与默认编排器行为单元测试
*
*=================================================
*修改标记
*修改时间：2026/10/26 15:05:23
*修改人： yswenli
*版本号： V1.0.0.0
*描述：AgentRunRequest 参数校验与默认编排器行为单元测试
*
*****************************************************************************/

using LuBan.AIAgent.Attachments;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Hosting;

using Microsoft.Extensions.Options;

namespace LuBan.XTestProject;

[TestClass]
public class AgentRunRequestValidationUnitTest
{
    [TestMethod]
    public void AllowAllModelCapabilityGate_AlwaysAllows()
    {
        IModelCapabilityGate gate = new AllowAllModelCapabilityGate();

        Assert.IsTrue(gate.CanAttach("any", Array.Empty<ProcessedAttachment>()));
        Assert.AreEqual(string.Empty, gate.GetUnsupportedMessage("any", Array.Empty<ProcessedAttachment>()));
    }

    [TestMethod]
    public void AllowAllModelCapabilityGate_CreateProbe_ReturnsNull()
    {
        IModelCapabilityGate gate = new AllowAllModelCapabilityGate();

        Assert.IsNull(gate.CreateProbe("any", hasImage: true));
    }

    [TestMethod]
    public void DefaultOrchestrator_ExtractReferences_ReturnsInputUnchanged()
    {
        var orchestrator = new DefaultAgentAttachmentOrchestrator(new FakeProcessor(), new AllowAllModelCapabilityGate(), Options.Create(new LuBanAgentOptions()));

        var (text, paths) = orchestrator.ExtractReferences("@a.txt 你好");

        Assert.AreEqual("@a.txt 你好", text);
        Assert.AreEqual(0, paths.Count);
    }

    [TestMethod]
    public async Task DefaultOrchestrator_ProcessAsync_EmptyPaths_ReturnsEmpty()
    {
        var processor = new FakeProcessor();
        var orchestrator = new DefaultAgentAttachmentOrchestrator(processor, new AllowAllModelCapabilityGate(), Options.Create(new LuBanAgentOptions()));

        var result = await orchestrator.ProcessAsync(Array.Empty<string>());

        Assert.AreEqual(0, result.Processed.Count);
        Assert.AreEqual(0, processor.Calls);
    }

    [TestMethod]
    public async Task DefaultOrchestrator_ProcessAsync_MoreThanFive_TruncatesAndSkips()
    {
        var processor = new FakeProcessor();
        var orchestrator = new DefaultAgentAttachmentOrchestrator(processor, new AllowAllModelCapabilityGate(), Options.Create(new LuBanAgentOptions()));
        var paths = Enumerable.Range(0, 6).Select(i => $@"C:\tmp\f{i}.txt").ToArray();

        // 超限不再抛异常（会让整轮对话失败），改为保留前 5 个 + 其余记为跳过
        var result = await orchestrator.ProcessAsync(paths);

        Assert.AreEqual(5, processor.Calls, "只处理上限内的前 5 个");
        Assert.AreEqual(0, result.Processed.Count);
        Assert.AreEqual(6, result.Failed.Count, "5 个处理失败 + 1 个超限跳过");
        Assert.IsTrue(result.Failed.Any(f => f.Reason.Contains("超出附件上限")), "超限项必须带可读原因");
    }

    [TestMethod]
    public void AgentHost_RunStreamingAsync_NullRequest_Throws()
    {
        var host = new AgentHost(null!, null!, null!, null!, null!, null!, null!, null!, null!, null!);

        Assert.ThrowsExactly<ArgumentNullException>(() => host.RunStreamingAsync(null!));
    }

    private sealed class FakeProcessor : IAttachmentProcessor
    {
        public int Calls { get; private set; }

        public Task<ProcessedAttachment> ProcessAsync(string filePath, bool generateThumbnail = false, CancellationToken ct = default)
        {
            Calls++;
            throw new NotSupportedException("单测不应真正处理文件");
        }

        public bool IsSupported(string filePath) => true;

        public string ResolveMediaType(string filePath) => "application/octet-stream";

        public IReadOnlyList<string> SupportedExtensions => [".txt"];
    }
}