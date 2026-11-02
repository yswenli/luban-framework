/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： MarkdownUtilUnitTest
*创建时间：2026/10/9
*描述：验证 HTML→Markdown 基础转换
*****************************************************************************/
using LuBan.Common;

namespace LuBan.XTestProject;

[TestClass]
public class MarkdownUtilUnitTest
{
    [TestMethod]
    public void HtmlToMarkdown_ConvertsHeadingsAndLinks()
    {
        const string html = "<h1>标题</h1><p>正文 <a href=\"https://example.com\">链接</a></p>";

        var md = html.HtmlToMarkdown();

        StringAssert.Contains(md, "# 标题");
        StringAssert.Contains(md, "正文");
        StringAssert.Contains(md, "[链接](https://example.com)");
    }

    [TestMethod]
    public void HtmlToMarkdown_EmptyInput_ReturnsEmpty()
    {
        var md = "".HtmlToMarkdown();
        Assert.AreEqual(string.Empty, md);
    }
}
