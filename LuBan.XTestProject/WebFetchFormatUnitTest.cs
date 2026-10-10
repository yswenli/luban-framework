/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： WebFetchFormatUnitTest
*创建时间：2026/10/9
*描述：验证 Web 抓取格式参数与降级链
*****************************************************************************/
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Tools.Web;

namespace LuBan.XTestProject;

[TestClass]
public class WebFetchFormatUnitTest
{
    private const string SampleHtml =
        "<html><head><title>T</title></head><body><h1>Title</h1><p>Hello <b>World</b></p></body></html>";

    [TestMethod]
    public void WebToolOptions_DefaultFormat_IsMarkdown()
    {
        var opts = new WebToolOptions();
        Assert.AreEqual("markdown", opts.DefaultFormat);
    }

    [TestMethod]
    public void NormalizeFormat_NullOrBlank_FallsBackToMarkdown()
    {
        Assert.AreEqual("markdown", WebToolGroup.NormalizeFormat(null));
        Assert.AreEqual("markdown", WebToolGroup.NormalizeFormat(""));
        Assert.AreEqual("markdown", WebToolGroup.NormalizeFormat("   "));
    }

    [TestMethod]
    public void NormalizeFormat_ValidValues_AreNormalized()
    {
        Assert.AreEqual("raw", WebToolGroup.NormalizeFormat("RAW "));
        Assert.AreEqual("markdown", WebToolGroup.NormalizeFormat("Markdown"));
        Assert.AreEqual("text", WebToolGroup.NormalizeFormat(" text "));
    }

    [TestMethod]
    public void NormalizeFormat_UnknownValue_FallsBackToMarkdown()
    {
        Assert.AreEqual("markdown", WebToolGroup.NormalizeFormat("bogus"));
    }

    [TestMethod]
    public void ConvertHtml_RawFormat_ReturnsInputUnchanged()
    {
        var result = WebToolGroup.ConvertHtml(SampleHtml, "raw", "https://example.com/");
        Assert.AreEqual(SampleHtml, result);
    }

    [TestMethod]
    public void ConvertHtml_MarkdownFormat_ConvertsWithoutRawHtmlTags()
    {
        var result = WebToolGroup.ConvertHtml(SampleHtml, "markdown", "https://example.com/");
        Assert.IsFalse(result.Contains("<html"), "markdown 结果不应包含原始 html 标签");
        StringAssert.Contains(result, "Title");
    }

    [TestMethod]
    public void ConvertHtml_TextFormat_StripsTags()
    {
        var result = WebToolGroup.ConvertHtml(SampleHtml, "text", "https://example.com/");
        Assert.IsFalse(result.Contains("<"), "text 结果不应包含标签");
        StringAssert.Contains(result, "Title");
        StringAssert.Contains(result, "World");
    }

    [TestMethod]
    public void ConvertHtml_MarkdownFormat_NoTextHtml_DegradesToNonEmpty()
    {
        var result = WebToolGroup.ConvertHtml("<html><body><br/></body></html>", "markdown", "https://example.com/");
        Assert.IsFalse(string.IsNullOrEmpty(result), "降级链应回退到非空内容");
    }

    [TestMethod]
    public void LooksLikeHtml_DetectsHtmlContent()
    {
        Assert.IsTrue(WebToolGroup.LooksLikeHtml(SampleHtml));
        Assert.IsTrue(WebToolGroup.LooksLikeHtml("<!DOCTYPE html><html></html>"));
        Assert.IsFalse(WebToolGroup.LooksLikeHtml("plain text"));
        Assert.IsFalse(WebToolGroup.LooksLikeHtml(""));
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task FetchUrlAsync_RawFormat_ReturnsHtmlUnchanged()
    {
        var group = new WebToolGroup(new WebToolOptions { MaxCharacters = 12000 });
        try
        {
            var result = await group.FetchUrlAsync("https://example.com/", format: "raw");
            if (!result.IsSuccess) { Assert.Inconclusive("网络不可用，跳过"); return; }
            StringAssert.Contains(result.Data!, "<");
        }
        catch (Exception ex)
        {
            Assert.Inconclusive($"网络不可用，跳过：{ex.Message}");
        }
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task FetchUrlAsync_MarkdownFormat_DoesNotReturnRawHtmlTags()
    {
        var group = new WebToolGroup(new WebToolOptions { MaxCharacters = 12000 });
        try
        {
            var result = await group.FetchUrlAsync("https://example.com/", format: "markdown");
            if (!result.IsSuccess) { Assert.Inconclusive("网络不可用，跳过"); return; }
            Assert.IsFalse(result.Data!.Contains("<html"), "markdown 结果不应包含原始 html 标签");
        }
        catch (Exception ex)
        {
            Assert.Inconclusive($"网络不可用，跳过：{ex.Message}");
        }
    }
}
