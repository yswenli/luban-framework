/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： ServiceClientUtil
*版本号： V1.0.0.0
*唯一标识：6ed81854-6b63-4a15-9e3f-8f809d081da9
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/6/8 11:33:12
*描述：业务客户端工具类
*
*=================================================
*修改标记
*修改时间：2023/6/8 11:33:12
*修改人： yswenli
*版本号： V1.0.0.0
*描述：业务客户端工具类
*
*****************************************************************************/


namespace LuBan.Common;

/// <summary>
/// 业务客户端工具类
/// </summary>
public class ApiClientUtil
{
    HttpClientProxy _client;

    /// <summary>
    /// 业务客户端工具类
    /// </summary>
    /// <param name="baseUrl"></param>
    /// <param name="timeOut"></param>
    /// <param name="version"></param>
    /// <param name="cookiescontainer"></param>
    /// <param name="webProxy"></param>
    /// <param name="useLog"></param>
    public ApiClientUtil(string baseUrl, int timeOut = 30, string version = "1.1", CookieContainer? cookiescontainer = null, WebProxy? webProxy = null, bool useLog = false)
    {
        _client = HttpClientProxy.Create(baseUrl, timeOut, version, cookiescontainer, webProxy, useLog);
    }

    /// <summary>
    /// 校验jwt
    /// </summary>
    /// <param name="jwt"></param>
    /// <returns></returns>
    string CheckJwt(string jwt)
    {
        if (jwt.IsNullOrEmpty())
        {
            return "";
        }
        if (jwt.StartsWith("Bearer "))
        {
            return jwt;
        }
        return $"Bearer {jwt}";
    }

    /// <summary>
    /// 构建头部
    /// </summary>
    /// <param name="jwt"></param>
    /// <param name="nonce"></param>
    /// <param name="timestamp"></param>
    /// <param name="signature"></param>
    /// <returns></returns>
    Dictionary<string, string> BuildHeader(string jwt, string nonce, string timestamp, string signature)
    {
        var header = new Dictionary<string, string>();
        var checkedJwt = CheckJwt(jwt);
        if (checkedJwt.IsNotNullOrEmpty())
        {
            header.Add("Authorization", checkedJwt);
        }
        if (signature.IsNotNullOrEmpty())
        {
            header.Add("nonce", nonce);
            header.Add("timestamp", timestamp);
            header.Add("signature", signature);
        }
        return header;
    }

    /// <summary>
    /// 构建头部
    /// </summary>
    /// <param name="jwt"></param>
    /// <param name="safeComparisonInfo"></param>
    /// <returns></returns>
    Dictionary<string, string> BuildHeader(string jwt, AraInfo safeComparisonInfo)
    {
        return BuildHeader(jwt, safeComparisonInfo.Nonce, safeComparisonInfo.TimeStamp.ToString(), safeComparisonInfo.Signature);
    }

    /// <summary>
    /// 构建头部
    /// </summary>
    /// <param name="jwt"></param>
    /// <param name="query"></param>
    /// <param name="postModel"></param>
    /// <returns></returns>
    Dictionary<string, string> BuildHeader(string jwt, string query, object postModel)
    {
        var sd = new AraData();
        if (query.IsNotNullOrEmpty() && query.IndexOf("?") > -1)
        {
            sd = query.GetQueryForSortedDic();
        }
        if (postModel != null)
        {
            if (postModel.GetType().IsClass)
            {
                if (postModel is string postModelStr)
                {
                    if (postModelStr.IsNotNullOrEmpty())
                    {
                        sd.TryAdd("md5", MD5Util.GetMD5Str(postModel.ToString()));
                    }
                }
                else
                {
                    var json = SerializeUtil.Serialize(postModel);
                    if (json.IsNotNullOrEmpty())
                    {
                        sd.TryAdd("md5", MD5Util.GetMD5Str(json));
                    }
                }
            }
            else if (postModel is byte[] postModelBytes)
            {
                if (postModelBytes != null && postModelBytes.Length > 0)
                {
                    sd.TryAdd("md5", MD5Util.GetMD5Str(postModelBytes));
                }
            }
        }

        var safeComparisonInfo = AraReplayAttacksUtil.GetSafeComparisonInfo(sd, out Data.Result _);
        if (safeComparisonInfo == null) throw new Exception("计算签名有误");
        return BuildHeader(jwt, safeComparisonInfo);
    }


    /// <summary>
    /// post
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resource"></param>
    /// <param name="postModel"></param>
    /// <param name="jwt"></param>
    /// <param name="withSafe"></param>
    /// <returns></returns>
    public async Task<T> PostAsync<T>(string resource, object postModel, string jwt, bool withSafe)
    {
        Dictionary<string, string> header;
        if (withSafe)
        {
            header = BuildHeader(jwt, resource, postModel);
        }
        else
        {
            header = BuildHeader(jwt, "", "", "");
        }
        if (!header.ContainsKey("Content-Type"))
        {
            header["Content-Type"] = "application/json; charset=utf-8";
        }
        return await _client.PostAsync<T>(resource, postModel, header);
    }

    /// <summary>
    /// post
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="data"></param>
    /// <param name="jwt"></param>
    /// <param name="withSafe"></param>
    /// <returns></returns>
    public async Task<byte[]> PostAsync(string resource, byte[] data, string jwt, bool withSafe)
    {
        Dictionary<string, string> header;
        if (withSafe)
        {
            header = BuildHeader(jwt, resource, data);
        }
        else
        {
            header = BuildHeader(jwt, "", "", "");
        }
        if (!header.ContainsKey("Content-Type"))
        {
            header["Content-Type"] = "application/json; charset=utf-8";
        }
        return await _client.PostBytesAsync(resource, data, header);
    }


    /// <summary>
    /// get
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resource"></param>
    /// <param name="jwt"></param>
    /// <param name="withSafe"></param>
    /// <returns></returns>
    public async Task<T> GetAsync<T>(string resource, string jwt, bool withSafe)
    {
        Dictionary<string, string> header;
        if (withSafe)
        {
            header = BuildHeader(jwt, resource, "");
        }
        else
        {
            header = BuildHeader(jwt, "", "", "");
        }
        return await _client.GetAsync<T>(resource, header);
    }
}
