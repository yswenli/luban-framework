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
*文件名： AgentAttachmentSourceResolverUnitTest
*版本号： V1.0.0.0
*唯一标识：f2b6c1a4-8f0e-4a7d-9b3c-5d1e2a7c4b90
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/29
*描述：附件来源解析（URL / 绝对路径 / 相对路径）单元测试
*
*=================================================
*修改标记
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：附件来源解析（URL / 绝对路径 / 相对路径）单元测试
*
*****************************************************************************/

using LuBan.AIAgent.Attachments.Ingestion;
using LuBan.AIAgent.Configuration;

using Microsoft.Extensions.Options;

using System.Net;

namespace LuBan.XTestProject;

[TestClass]
public class AgentAttachmentSourceResolverUnitTest
{
    private string _ws = "";

    [TestInitialize]
    public void Setup()
    {
        _ws = Path.Combine(Path.GetTempPath(), "luban-att-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_ws);
    }

    [TestCleanup]
    public void Cleanup()
    {
        try { Directory.Delete(_ws, recursive: true); } catch { }
    }

    private DefaultAttachmentSourceResolver Build(HttpMessageHandler? handler = null)
        => new(
            Options.Create(new LuBanAgentOptions
            {
                WorkspaceRoot = _ws,
                Attachments = new AttachmentsOptions { MaxFileBytes = 1024 * 1024, TimeoutSeconds = 5 }
            }),
            handler is null ? null : new HttpClient(handler));

    private AttachmentSourceContext Ctx(string? contentRoot = null)
        => new(_ws, contentRoot, "sid-1");

    [TestMethod]
    public async Task WorkspaceRelative_ResolvesToExistingFile()
    {
        File.WriteAllText(Path.Combine(_ws, "notes.txt"), "hi");

        var result = await Build().ResolveAsync(new[] { "notes.txt" }, Ctx());

        Assert.AreEqual(1, result.Resolved.Count);
        Assert.AreEqual("notes.txt", result.Resolved[0].Source);
        Assert.AreEqual("text/plain", result.Resolved[0].MediaType);
        Assert.AreEqual(0, result.Failed.Count);
    }

    [TestMethod]
    public async Task ContentRootRelative_IsSecondChance()
    {
        var web = Path.Combine(_ws, "wwwroot");
        Directory.CreateDirectory(web);
        File.WriteAllText(Path.Combine(web, "a.txt"), "hi");

        var result = await Build().ResolveAsync(new[] { "a.txt" }, Ctx(web));

        Assert.AreEqual(1, result.Resolved.Count);
        Assert.AreEqual(Path.Combine(web, "a.txt"), result.Resolved[0].LocalPath);
    }

    [TestMethod]
    public async Task MissingPath_FailsWithReason()
    {
        var result = await Build().ResolveAsync(new[] { "nope.txt" }, Ctx());

        Assert.AreEqual(0, result.Resolved.Count);
        Assert.AreEqual("路径不存在或不可读", result.Failed[0].Reason);
    }

    [TestMethod]
    public async Task UnsupportedExtension_FailsWithReason()
    {
        File.WriteAllText(Path.Combine(_ws, "a.pdf"), "hi");

        var result = await Build().ResolveAsync(new[] { "a.pdf" }, Ctx());

        Assert.AreEqual("不支持的附件类型", result.Failed[0].Reason);
    }

    [TestMethod]
    public async Task OversizedLocalFile_FailsWithReason()
    {
        File.WriteAllBytes(Path.Combine(_ws, "big.txt"), new byte[2 * 1024 * 1024]);

        var result = await Build().ResolveAsync(new[] { "big.txt" }, Ctx());

        Assert.AreEqual("文件过大", result.Failed[0].Reason);
    }

    [TestMethod]
    public async Task FileUri_ResolvesToLocalPath()
    {
        var path = Path.Combine(_ws, "x.txt");
        File.WriteAllText(path, "hi");

        var result = await Build().ResolveAsync(new[] { new Uri(path).AbsoluteUri }, Ctx());

        Assert.AreEqual(1, result.Resolved.Count);
        Assert.AreEqual(path, result.Resolved[0].LocalPath);
    }

    [TestMethod]
    public async Task Url_DownloadsIntoWorkspaceWithGuidName()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(new byte[] { 1, 2, 3 })
        });

        var result = await Build(handler).ResolveAsync(new[] { "https://cdn.example.com/pic.png" }, Ctx());

        Assert.AreEqual(1, result.Resolved.Count, string.Join(";", result.Failed.Select(f => f.Reason)));
        var local = result.Resolved[0].LocalPath;
        Assert.IsTrue(File.Exists(local));
        Assert.AreEqual(".png", Path.GetExtension(local));
        StringAssert.Contains(local, Path.Combine(_ws, ".luban-agent", "attachments", "sid-1"));
        Assert.AreEqual("https://cdn.example.com/pic.png", result.Resolved[0].Source);

        // 磁盘名是 guid，响应/事件里回的是原 URL 文件名
        Assert.AreEqual("pic.png", result.Resolved[0].FileName);
        Assert.AreNotEqual("pic.png", Path.GetFileName(local));
    }

    [TestMethod]
    public async Task Url404_FailsWithDownloadReason()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));

        var result = await Build(handler).ResolveAsync(new[] { "https://cdn.example.com/a.png" }, Ctx());

        Assert.AreEqual(0, result.Resolved.Count);
        StringAssert.Contains(result.Failed[0].Reason, "下载失败");
    }

    [TestMethod]
    public async Task UrlTooLarge_DeletesPartialAndFails()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(new byte[2 * 1024 * 1024])
        });

        var result = await Build(handler).ResolveAsync(new[] { "https://cdn.example.com/a.png" }, Ctx());

        Assert.AreEqual("文件过大", result.Failed[0].Reason);
        var dir = Path.Combine(_ws, ".luban-agent", "attachments");
        Assert.IsFalse(Directory.Exists(dir) && Directory.GetFiles(dir, "*", SearchOption.AllDirectories).Length > 0);
    }

    [TestMethod]
    public void Classifier_WindowsSlashPath_IsRelative_NotDriveRoot()
    {
        if (!OperatingSystem.IsWindows()) Assert.Inconclusive("Windows 专用用例");

        var kind = AttachmentPathClassifier.Classify("/fonts/a.png");

        Assert.AreEqual(AttachmentSourceKind.Relative, kind);
    }

    [TestMethod]
    public void Classifier_DriveAndUnc_AreLocalAbsolute()
    {
        if (!OperatingSystem.IsWindows()) Assert.Inconclusive("Windows 专用用例");

        Assert.AreEqual(AttachmentSourceKind.LocalAbsolute, AttachmentPathClassifier.Classify(@"D:\a\b.txt"));
        Assert.AreEqual(AttachmentSourceKind.LocalAbsolute, AttachmentPathClassifier.Classify(@"\\server\share\b.txt"));
    }

    [TestMethod]
    public void Classifier_HttpAndFileUri()
    {
        Assert.AreEqual(AttachmentSourceKind.Http, AttachmentPathClassifier.Classify("https://x/a.png"));
        Assert.AreEqual(AttachmentSourceKind.FileUri, AttachmentPathClassifier.Classify("file:///C:/a.png"));
    }

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
            => Task.FromResult(respond(request));
    }
}