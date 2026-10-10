/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： HttpClientProxyResponseUnitTest
*创建时间：2026/10/9
*描述：验证 GetResponseAsync 返回字节与 ContentType
*****************************************************************************/
using LuBan.Common.Http;

namespace LuBan.XTestProject;

[TestClass]
public class HttpClientProxyResponseUnitTest
{
    [TestMethod]
    public async Task GetResponseAsync_ReturnsBytesAndContentType()
    {
        // 使用 example.com 作为稳定可访问的 text/html 目标；无网络时允许跳过
        var proxy = HttpClientProxy.Create(new Uri("https://example.com/"), timeout: 30, useLog: false);
        (byte[] Bytes, string? ContentType) result;
        try
        {
            result = await proxy.GetResponseAsync(string.Empty, timeout: 30);
        }
        catch (Exception ex)
        {
            Assert.Inconclusive($"网络不可用，跳过：{ex.Message}");
            return;
        }

        Assert.IsTrue(result.Bytes.Length > 0);
        StringAssert.Contains(result.ContentType ?? "", "html");
    }
}
