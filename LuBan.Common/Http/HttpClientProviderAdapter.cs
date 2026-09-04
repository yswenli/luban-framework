/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common.Http
*文件名： HttpClientProviderAdapter.cs
*版本号： V1.0.0.0
*唯一标识：dfde5730-204b-4cc2-aa54-10a46bcb811e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：HttpClientProviderAdapter 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：HttpClientProviderAdapter 类
*
*****************************************************************************/

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
