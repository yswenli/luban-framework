/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.Http
*文件名： HttpExtension
*版本号： V1.0.0.0
*唯一标识：c343572c-4a8e-45e4-991c-0fb5587611d0
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/9/1 13:43:34
*描述：http快捷请求类
*
*=================================================
*修改标记
*修改时间：2025/9/1 13:43:34
*修改人： yswenli
*版本号： V1.0.0.0
*描述：http快捷请求类
*
*****************************************************************************/

namespace System;

/// <summary>
/// http快捷请求类
/// </summary>
public static class HttpExtension
{
    /// <summary>
    /// 获取请求的域名和路径
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    static (string, string) GetDomainAndPath(string url)
    {
        var uri = new Uri(url);
        return (uri.Scheme + "://" + uri.Authority, uri.PathAndQuery);
    }

    /// <summary>
    /// get 请求
    /// </summary>
    /// <param name="url"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public static async Task<byte[]> HttpGetAsync(this string url, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        var domainAndPath = GetDomainAndPath(url);
        var client = HttpClientProxy.Create(domainAndPath.Item1);
        return await client.GetBytesAsync(domainAndPath.Item2, headers, timeout);
    }

    /// <summary>
    /// get 请求
    /// </summary>
    /// <param name="url"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public static async Task<string> HttpGetJsonAsync(this string url, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        var domainAndPath = GetDomainAndPath(url);
        var client = HttpClientProxy.Create(domainAndPath.Item1);
        return await client.GetAsync(domainAndPath.Item2, headers, timeout);
    }
    /// <summary>
    /// get 请求
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="url"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public static async Task<T> HttpGetAsync<T>(this string url, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        var domainAndPath = GetDomainAndPath(url);
        var client = HttpClientProxy.Create(domainAndPath.Item1);
        return await client.GetAsync<T>(domainAndPath.Item2, headers, timeout);
    }

    /// <summary>
    /// post 请求
    /// </summary>
    /// <param name="url"></param>
    /// <param name="postData"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public static async Task<byte[]> HttpPostAsync(this string url, byte[] postData, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        var domainAndPath = GetDomainAndPath(url);
        var client = HttpClientProxy.Create(domainAndPath.Item1);
        return await client.PostBytesAsync(domainAndPath.Item2, postData, headers, timeout);
    }

    /// <summary>
    /// post 请求
    /// </summary>
    /// <param name="url"></param>
    /// <param name="json"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public static async Task<string> HttpPostJsonAsync(this string url, string json, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        var domainAndPath = GetDomainAndPath(url);
        var client = HttpClientProxy.Create(domainAndPath.Item1);
        return await client.PostAsync(domainAndPath.Item2, json, headers, timeout);
    }

    /// <summary>
    /// post 请求
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="url"></param>
    /// <param name="postModel"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public static async Task<T> HttpPostAsync<T>(this string url, object postModel, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        var domainAndPath = GetDomainAndPath(url);
        var client = HttpClientProxy.Create(domainAndPath.Item1);
        return await client.PostAsync<T>(domainAndPath.Item2, postModel, headers, timeout);
    }

    /// <summary>
    /// 删除 请求
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="url"></param>
    /// <param name="postModel"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public static async Task<T?> HttpDeleteAsync<T>(this string url, object postModel, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        var domainAndPath = GetDomainAndPath(url);
        var client = HttpClientProxy.Create(domainAndPath.Item1);
        return await client.DeleteAsync<T>(domainAndPath.Item2, postModel, headers, timeout);
    }

    /// <summary>
    /// Delete
    /// </summary>
    /// <param name="url"></param>
    /// <param name="json"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public static async Task<string> HttpDeleteJsonAsync(this string url, string json, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        var domainAndPath = GetDomainAndPath(url);
        var client = HttpClientProxy.Create(domainAndPath.Item1);
        return await client.DeleteJsonAsync(domainAndPath.Item2, json, headers, timeout);
    }

    /// <summary>
    /// Delete
    /// </summary>
    /// <param name="url"></param>
    /// <param name="postData"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public static async Task<byte[]> HttpDeleteAsync(this string url, byte[] postData, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        var domainAndPath = GetDomainAndPath(url);
        var client = HttpClientProxy.Create(domainAndPath.Item1);
        return await client.DeleteAsync(domainAndPath.Item2, postData, headers, timeout);
    }

    /// <summary>
    /// Put请求
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="url"></param>
    /// <param name="postModel"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public static async Task<T?> HttpPutAsync<T>(this string url, object postModel, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        var domainAndPath = GetDomainAndPath(url);
        var client = HttpClientProxy.Create(domainAndPath.Item1);
        return await client.PutAsync<T>(domainAndPath.Item2, postModel, headers, timeout);
    }

    /// <summary>
    /// Put请求
    /// </summary>
    /// <param name="url"></param>
    /// <param name="json"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public static async Task<string> HttpPutJsonAsync(this string url, string json, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        var domainAndPath = GetDomainAndPath(url);
        var client = HttpClientProxy.Create(domainAndPath.Item1);
        return await client.PutJsonAsync(domainAndPath.Item2, json, headers, timeout);
    }

    /// <summary>
    /// Put请求
    /// </summary>
    /// <param name="url"></param>
    /// <param name="postData"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public static async Task<byte[]> HttpPutAsync(this string url, byte[] postData, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        var domainAndPath = GetDomainAndPath(url);
        var client = HttpClientProxy.Create(domainAndPath.Item1);
        return await client.PutAsync(domainAndPath.Item2, postData, headers, timeout);
    }

    /// <summary>
    /// Patch请求
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="url"></param>
    /// <param name="postModel"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public static async Task<T?> HttpPatchAsync<T>(this string url, object postModel, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        var domainAndPath = GetDomainAndPath(url);
        var client = HttpClientProxy.Create(domainAndPath.Item1);
        return await client.PatchAsync<T>(domainAndPath.Item2, postModel, headers, timeout);
    }

    /// <summary>
    /// Patch请求
    /// </summary>
    /// <param name="url"></param>
    /// <param name="json"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public static async Task<string> HttpPatchJsonAsync(this string url, string json, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        var domainAndPath = GetDomainAndPath(url);
        var client = HttpClientProxy.Create(domainAndPath.Item1);
        return await client.PatchJsonAsync(domainAndPath.Item2, json, headers, timeout);
    }

    /// <summary>
    /// Patch请求
    /// </summary>
    /// <param name="url"></param>
    /// <param name="postData"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public static async Task<byte[]> HttpPatchAsync(this string url, byte[] postData, Dictionary<string, string>? headers = null, int timeout = 180)
    {
        var domainAndPath = GetDomainAndPath(url);
        var client = HttpClientProxy.Create(domainAndPath.Item1);
        return await client.PatchAsync(domainAndPath.Item2, postData, headers, timeout);
    }
}
