/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common
*文件名： UrlUtil
*版本号： V1.0.0.0
*唯一标识：722ac77b-df18-463c-afb2-c79f1b44cdd2
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/6/23 18:18:16
*描述：url工具类
*
*=================================================
*修改标记
*修改时间：2025/6/23 18:18:16
*修改人： yswenli
*版本号： V1.0.0.0
*描述：url工具类
*
*****************************************************************************/
namespace System;

/// <summary>
/// url工具类
/// </summary>
public static class UrlUtil
{
    /// <summary>
    /// 将url转换为基本url和查询字符串
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static ValueTuple<string, string> GetBaseUrlAndQuery(this string url)
    {
        var uri = new Uri(url);

        var baseUrl = uri.IsDefaultPort
            ? $"{uri.Scheme}://{uri.Host}"
            : $"{uri.Scheme}://{uri.Host}:{uri.Port}";

        var resource = uri.PathAndQuery;

        return (baseUrl, resource);
    }

    /// <summary>
    /// url encode
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string UrlEncode(this string str)
    {
        if (!string.IsNullOrEmpty(str))
            return HttpUtility.UrlEncode(str);
        return string.Empty;
    }

    /// <summary>
    /// url decode
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string UrlDecode(this string str)
    {
        return HttpUtility.UrlDecode(str);
    }



    /// <summary>
    /// 将键值对转换成url query
    /// </summary>
    /// <param name="nvc"></param>
    /// <returns></returns>
    public static string ToQueryString(this NameValueCollection nvc)
    {
        if (nvc != null && nvc.AllKeys.Length > 0)
        {
            var array = (from key in nvc.AllKeys
                         from value in nvc.GetValues(key) ?? []
                         select string.Format("{0}={1}", key.UrlEncode(), value.UrlEncode()))
                .ToArray();
            return "?" + string.Join("&", array);
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// 将string转换成nv
    /// </summary>
    /// <param name="queryString"></param>
    /// <returns></returns>
    public static NameValueCollection ParseQueryString(this string queryString)
    {
        var nvc = new NameValueCollection();
        if (!string.IsNullOrWhiteSpace(queryString))
            nvc = HttpUtility.ParseQueryString(queryString);
        return nvc;
    }
    /// <summary>
    /// 将nv转换成字典
    /// </summary>
    /// <param name="nvc"></param>
    /// <returns></returns>
    public static IDictionary<string, string> ToDictionary(this NameValueCollection nvc)
    {
        var dic = new Dictionary<string, string>();
        foreach (var k in nvc.AllKeys)
        {
            if (string.IsNullOrEmpty(k)) continue;
            var val = nvc[k];
            if (string.IsNullOrEmpty(val)) continue;
            dic.Add(k, val);
        }
        return dic;
    }

    /// <summary>
    /// 将string转换成nv
    /// </summary>
    /// <param name="queryStr"></param>
    /// <returns></returns>
    public static SortedDictionary<string, string> ToQueryDic(this string queryStr)
    {
        var nvc = queryStr.ParseQueryString();
        var dic = new SortedDictionary<string, string>();
        foreach (var k in nvc.AllKeys)
        {
            if (string.IsNullOrEmpty(k)) continue;
            var val = nvc[k];
            if (string.IsNullOrEmpty(val)) continue;
            dic.Add(k, val);
        }
        return dic;
    }


    /// <summary>
    /// 获取完整url中Resource部分地址
    /// </summary>
    /// <param name="url"></param>
    /// <param name="indexStr"></param>
    /// <returns></returns>
    public static string? GetUrlResource(this string url, string indexStr = "/api/")
    {
        if (string.IsNullOrEmpty(url)) return null;

        var index = url.IndexOf(indexStr);

        if (index < 0) return null;

        return url.Substring(index);
    }
}
