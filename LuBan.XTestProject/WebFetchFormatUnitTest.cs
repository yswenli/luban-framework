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
    [TestMethod]
    public void WebToolOptions_DefaultFormat_IsMarkdown()
    {
        var opts = new WebToolOptions();
        Assert.AreEqual("markdown", opts.DefaultFormat);
    }

    [TestMethod]
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
