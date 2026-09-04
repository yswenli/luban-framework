/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common.Http
*文件名： IHttpClientProvider.cs
*版本号： V1.0.0.0
*唯一标识：69b66eb8-549e-4758-8044-6f9a64243484
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：IHttpClientProvider 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：IHttpClientProvider 类
*
*****************************************************************************/

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
