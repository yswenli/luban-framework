/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Lives.VH
*文件名： VHLiveClient
*版本号： V1.0.0.0
*唯一标识：efecf39c-b1e4-4170-9b88-1c05e9c98ca1
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/4/29 10:05:56
*描述：微吼直播客户端
*
*=================================================
*修改标记
*修改时间：2025/4/29 10:05:56
*修改人： yswenli
*版本号： V1.0.0.0
*描述：微吼直播客户端
*
*****************************************************************************/
using LuBan.Lives.VH.Models;


namespace LuBan.Lives.VH;

/// <summary>
/// 微吼直播客户端
/// </summary>
public class VHLiveClient : BaseLiveClient, ILiveClient
{
    Dictionary<string, string> _headers;

    /// <summary>
    /// 微吼直播客户端
    /// </summary>
    public VHLiveClient(LiveOption? config) : base(config)
    {
        if (config == null) throw new Exception("LiveOption不能为空");
        if (config.AppId.IsNullOrEmpty()) throw new Exception("AppId不能为空");
        if (config.AppSecret.IsNullOrEmpty()) throw new Exception("AppSecret不能为空");
        if (config.AuthSecret.IsNullOrEmpty()) throw new Exception("AuthSecret不能为空");
        if (config.Url.IsNullOrEmpty()) throw new Exception("Url不能为空");

        _httpClient = HttpClientProxy.Create(config.Url, timeout: 180, useLog: true);

        _headers = [];
        _headers.Add("platform", "15");
        _headers.Add("request-id", GuidUtil.GuidString);
        _headers.Add("Content-Type", "application/x-www-form-urlencoded");
    }

    /// <summary>
    /// 获取设置基础header
    /// </summary>
    /// <returns></returns>
    protected override Dictionary<string, string> GetBaseHeaders()
    {
        _headers.AddOrUpdate("request-id", () => GuidUtil.GuidString);
        return _headers;
    }

    /// <summary>
    /// 获取请求体
    /// </summary>
    /// <param name="forms"></param>
    /// <returns></returns>
    protected byte[] GetRequestBody(Dictionary<string, object>? forms)
    {
        SortedDictionary<string, object> dic = [];
        if (forms != null && forms.Count > 0)
        {
            foreach (var form in forms)
            {
                dic.Add(form.Key, form.Value);
            }
        }
        dic.Add("app_key", _liveOption.AppId);
        dic.Add("sign_type", 0); //0 md5 1rsa 2sha256 3 sm3
        dic.Add("signed_at", DateTimeUtil.Now.ToUnixTimeStamp(false));
        var sign = VhallSignUtil.GetSign(_liveOption.AuthSecret ?? "", dic);
        dic.Add("sign", sign);
        return dic.ToFormData().ToBytes();
    }

    /// <summary>
    /// 创建三方用户
    /// </summary>
    /// <param name="thirdUserId"></param>
    /// <param name="pwd"></param>
    /// <param name="nickName"></param>
    /// <param name="avatar"></param>
    /// <returns></returns>
    public async Task<Result?> CreateThirdUserAsync(string thirdUserId, string pwd, string? nickName, string? avatar)
    {
        Dictionary<string, object> forms = [];
        forms.Add("third_user_id", thirdUserId);
        forms.Add("pass", pwd);
        if (nickName.IsNotNullOrEmpty())
            forms.Add("nick_name", nickName);
        if (avatar.IsNotNullOrEmpty())
            forms.Add("head", avatar);
        return await _httpClient.PostAsync<Result>("/v3/users/open-user/create", GetRequestBody(forms), GetBaseHeaders());
    }



    public string GetLiveUrl(string channelId, string secret, string userId, string name, string avatar)
    {
        throw new NotImplementedException();
    }



}
