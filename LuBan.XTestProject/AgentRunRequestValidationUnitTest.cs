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