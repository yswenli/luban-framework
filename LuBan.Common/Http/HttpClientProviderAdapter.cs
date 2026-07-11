using System.Net;

namespace LuBan.Common.Http;

/// <summary>
/// HttpClientProvider 适配器，包装 HttpClientProxy 静态方法
/// </summary>
public class HttpClientProviderAdapter : IHttpClientProvider
{
    /// <summary>
    /// 创建 HttpClientProxy 实例
    /// </summary>
    public HttpClientProxy Create(
        string baseUrl,
        int timeout = 180,
        string version = "1.1",
        CookieContainer? cookiescontainer = null,
        WebProxy? webProxy = null,
        bool useLog = false)
    {
        return HttpClientProxy.Create(baseUrl, timeout, version, cookiescontainer, webProxy, useLog);
    }

    /// <summary>
    /// 创建 HttpClientProxy 实例
    /// </summary>
    public HttpClientProxy Create(
        Uri baseUri,
        int timeout = 180,
        string version = "1.1",
        CookieContainer? cookiescontainer = null,
        WebProxy? webProxy = null,
        bool useLog = false)
    {
        return HttpClientProxy.Create(baseUri, timeout, version, cookiescontainer, webProxy, useLog);
    }
}
