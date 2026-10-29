/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.XTestProject
*文件名： AgentAttachmentOrchestratorUnitTest
*版本号： V1.0.0.0
*唯一标识：9c1e5b7a-4d82-4f63-a159-6e3d0c8b2f47
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/29
*描述：附件编排逐项容错与上限读配置单元测试
*
*=================================================
*修改标记
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：附件编排逐项容错与上限读配置单元测试
*
*****************************************************************************/

using LuBan.AIAgent.Attachments;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Hosting;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace LuBan.XTestProject;

[TestClass]
public class AgentAttachmentOrchestratorUnitTest
{
    private static DefaultAgentAttachmentOrchestrator Build(int maxCount = 5)
        => new(new FakeProcessor(), new AllowAllModelCapabilityGate(),
               Options.Create(new LuBanAgentOptions
               {
                   Attachments = new AttachmentsOptions { MaxCount = maxCount }
               }));

    [TestMethod]
    public async Task OneBadFile_DoesNotThrow_AndReportsFailure()
    {
        var sut = Build();

        var result = await sut.ProcessAsync(new[] { "good.txt", "bad.txt" });

        Assert.AreEqual(1, result.Processed.Count);
        Assert.AreEqual("good.txt", result.Processed[0].Info.SourcePath);
        Assert.AreEqual(1, result.Failed.Count);
        Assert.AreEqual("bad.txt", result.Failed[0].Source);
        StringAssert.Contains(result.Failed[0].Reason, "处理失败");
    }

    [TestMethod]
    public async Task AllBad_ReturnsEmptyProcessed_WithAllFailures()
    {
        var sut = Build();

        var result = await sut.ProcessAsync(new[] { "bad.txt", "bad.txt" });

        Assert.AreEqual(0, result.Processed.Count);
        Assert.AreEqual(2, result.Failed.Count);
    }

    [TestMethod]
    public async Task OverMaxCount_Throws()
    {
        var sut = Build(maxCount: 1);

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => sut.ProcessAsync(new[] { "a.txt", "b.txt" }));
    }

    private sealed class FakeProcessor : IAttachmentProcessor
    {
        public IReadOnlyList<string> SupportedExtensions => new[] { ".txt" };

        public bool IsSupported(string filePath) => filePath.EndsWith(".txt");

        public string ResolveMediaType(string filePath) => "text/plain";

        public Task<ProcessedAttachment> ProcessAsync(string filePath, bool generateThumbnail = false, CancellationToken ct = default)
        {
            if (Path.GetFileNameWithoutExtension(filePath) == "bad")
                throw new InvalidOperationException("坏文件");

            return Task.FromResult(new ProcessedAttachment
            {
                Info = new AttachmentInfo(Path.GetFileName(filePath), "text/plain", 1,
                                          AttachmentKind.TextFile, filePath),
                Content = new TextContent("hi")
            });
        }
    }
}