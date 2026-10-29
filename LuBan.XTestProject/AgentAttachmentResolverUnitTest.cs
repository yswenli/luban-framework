using LuBan.AIAgent.Attachments;
using LuBan.AIAgent.Attachments.Ingestion;
using LuBan.AIAgent.Hosting;
using Microsoft.Extensions.AI;

namespace LuBan.XTestProject;

/// <summary>
/// 附件路径归一（<see cref="AgentAttachmentResolver"/>）与宿主接线单测：
/// 锁死「正文 @引用 未解析、只当普通文本发给模型」的缺陷（真机：用户写
/// 「D:\...\Marcus.png 看看这个图里面有什么」，附件为空，模型改用 ReadFile 读二进制）。
/// </summary>
[TestClass]
public class AgentAttachmentResolverUnitTest
{
    [TestMethod]
    public void Resolve_DefaultOrchestrator_KeepsUiPathsAndInput()
    {
        var orchestrator = new DefaultAgentAttachmentOrchestrator(
            new FakeProcessor(), new AllowAllModelCapabilityGate(),
            Microsoft.Extensions.Options.Options.Create(new LuBan.AIAgent.Configuration.LuBanAgentOptions()));
        var request = new AgentRunRequest
        {
            Input = "@a.png 你好",
            AttachmentPaths = [@"C:\tmp\ui.png"]
        };

        var (text, paths) = AgentAttachmentResolver.Resolve(request, orchestrator);

        Assert.AreEqual("@a.png 你好", text, "默认实现不解析 @引用，正文必须原样保留");
        Assert.AreEqual(1, paths.Count);
        Assert.AreEqual(@"C:\tmp\ui.png", paths[0]);
    }

    [TestMethod]
    public void Resolve_MergesUiAndReferenced_UiFirst_DedupesCaseInsensitively()
    {
        var orchestrator = new StubOrchestrator(
            [@"C:\tmp\A.PNG", @"C:\tmp\c.png", @"C:\tmp/ui.png "]);
        var request = new AgentRunRequest
        {
            Input = "@A.PNG @c.png",
            AttachmentPaths = [@"C:\tmp\ui.png", @"c:\tmp\a.png"]
        };

        var (text, paths) = AgentAttachmentResolver.Resolve(request, orchestrator);

        Assert.AreEqual("看图", text, "正文应使用剥离 @引用 后的文本");
        CollectionAssert.AreEqual(
            new[] { @"C:\tmp\ui.png", @"c:\tmp\a.png", @"C:\tmp\c.png" },
            paths.ToArray(),
            "UI 路径必须优先，且大小写不同的重复路径只保留一次");
    }

    [TestMethod]
    public void Resolve_NoPathsAnywhere_ReturnsEmpty()
    {
        var orchestrator = new StubOrchestrator([]);
        var request = new AgentRunRequest { Input = "你好" };

        var (text, paths) = AgentAttachmentResolver.Resolve(request, orchestrator);

        Assert.AreEqual("看图", text);
        Assert.AreEqual(0, paths.Count);
    }

    [TestMethod]
    public void Resolve_OnlyUiPaths_StillNormalizesAndDedupes()
    {
        var orchestrator = new StubOrchestrator([]);
        var request = new AgentRunRequest
        {
            Input = "看图",
            AttachmentPaths = [@"C:\tmp\a.png", @"  c:\tmp\A.png  ", @"   ", @"C:\tmp\b.png"]
        };

        var (_, paths) = AgentAttachmentResolver.Resolve(request, orchestrator);

        CollectionAssert.AreEqual(
            new[] { @"C:\tmp\a.png", @"C:\tmp\b.png" },
            paths.ToArray(),
            "没有 @引用 时也必须归一空白、去重并丢弃空项，不能把宿主原始列表直接透传");
    }

    [TestMethod]
    public async Task AgentHost_PassesExtractedAndUiPathsToOrchestrator()
    {
        var orchestrator = new StubOrchestrator([@"C:\tmp\ref.png"]);
        var host = new AgentHost(
            null!, null!, new StubWorkspaceResolver(), null!, null!,
            new StubModelSelection("p:m"),
            orchestrator,
            new StubSourceResolver(),
            null!,
            Microsoft.Extensions.Options.Options.Create(
                new LuBan.AIAgent.Configuration.LuBanAgentOptions()));

        var events = new List<AgentStreamEvent>();
        await foreach (var evt in host.RunStreamingAsync(new AgentRunRequest
        {
            Input = "@ref.png 看图",
            AttachmentPaths = [@"C:\tmp\ui.png"],
            SessionId = "s1"
        }))
        {
            events.Add(evt);
        }

        Assert.AreEqual("@ref.png 看图", orchestrator.ExtractedInput, "宿主必须把原始正文交给编排器解析");
        CollectionAssert.AreEqual(
            new[] { @"C:\tmp\ui.png", @"C:\tmp\ref.png" },
            orchestrator.ProcessedPaths.ToArray(),
            "宿主必须把合并后的来源交给处理管线");
        Assert.AreEqual(1, events.Count);
        Assert.IsInstanceOfType<ErrorEvent>(events[0]);
        StringAssert.Contains(((ErrorEvent)events[0]).Message, "stop");
    }

    private sealed class StubWorkspaceResolver : IAgentWorkspaceResolver
    {
        public Task<WorkspaceDescriptor> ResolveAsync(string sessionId, CancellationToken ct = default)
            => Task.FromResult(new WorkspaceDescriptor("w", "W", Path.GetTempPath(), "normal"));
    }

    /// <summary>把来源原样当成本地路径返回，便于断言宿主传给来源解析器的列表。</summary>
    private sealed class StubSourceResolver : IAgentAttachmentSourceResolver
    {
        public Task<AttachmentSourceResult> ResolveAsync(
            IReadOnlyList<string> sources, AttachmentSourceContext context, CancellationToken ct = default)
        {
            var resolved = sources
                .Select(s => new ResolvedAttachment(s, s, Path.GetFileName(s), "image/png", 10))
                .ToList();
            return Task.FromResult(new AttachmentSourceResult(
                resolved, Array.Empty<AttachmentSourceFailure>()));
        }
    }

    private sealed class StubModelSelection(string qualifiedModelName) : IAgentModelSelection
    {
        public string? ProviderId => "p";

        public string? ModelId => "m";

        public string QualifiedModelName { get; } = qualifiedModelName;
    }

    /// <summary>解析固定引用并记录宿主传入内容；处理阶段主动失败以阻断后续运行时依赖。</summary>
    private sealed class StubOrchestrator(IReadOnlyList<string> referenced) : IAgentAttachmentOrchestrator
    {
        public string? ExtractedInput { get; private set; }

        public IReadOnlyList<string> ProcessedPaths { get; private set; } = Array.Empty<string>();

        public (string Text, IReadOnlyList<string> Paths) ExtractReferences(string input)
        {
            ExtractedInput = input;
            return ("看图", referenced);
        }

        public Task<AttachmentProcessingResult> ProcessAsync(
            IReadOnlyList<string> paths, CancellationToken ct = default)
        {
            ProcessedPaths = paths;
            // 用哨兵异常终止：确保断言只覆盖「路径解析与传递」，不依赖会话运行时
            throw new InvalidOperationException("stop");
        }

        public string? Validate(string modelId, IReadOnlyList<ProcessedAttachment> attachments) => null;

        public IModelCapabilityProbe? CreateProbe(string modelId, bool hasImage) => null;
    }

    private sealed class FakeProcessor : IAttachmentProcessor
    {
        public Task<ProcessedAttachment> ProcessAsync(
            string filePath, bool generateThumbnail = false, CancellationToken ct = default)
            => throw new NotSupportedException("单测不应真正处理文件");

        public bool IsSupported(string filePath) => true;

        public string ResolveMediaType(string filePath) => "application/octet-stream";

        public IReadOnlyList<string> SupportedExtensions => [".png"];
    }
}