using System.Net;

namespace LuBan.Common.Http;

/// <summary>
/// HTTP 客户端提供者接口
/// </summary>
public interface IHttpClientProvider
{
    /// <summary>
    /// 创建 HttpClientProxy 实例
    /// </summary>
    HttpClientProxy Create(
        string baseUrl,
        int timeout = 180,
        string version = "1.1",
        CookieContainer? cookiescontainer = null,
        WebProxy? webProxy = null,
        bool useLog = false);

    /// <summary>
    /// 创建 HttpClientProxy 实例
    /// </summary>
    HttpClientProxy Create(
        Uri baseUri,
        int timeout = 180,
        string version = "1.1",
        CookieContainer? cookiescontainer = null,
        WebProxy? webProxy = null,
        bool useLog = false);
}
