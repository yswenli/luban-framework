/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Lives.VZan
*文件名： VZLiveClient
*版本号： V1.0.0.0
*唯一标识：19f69602-4998-4349-b019-d27c582b2b84
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/9/19 17:01:42
*描述：微赞直播
*
*=================================================
*修改标记
*修改时间：2024/9/19 17:01:42
*修改人： yswenli
*版本号： V1.0.0.0
*描述：微赞直播
*
*****************************************************************************/
using LuBan.Lives.VZan.Models;

namespace LuBan.Lives.VZan;

/// <summary>
/// 微赞直播
/// </summary>
public class VZLiveClient : BaseLiveClient, ILiveClient
{

    static readonly VZTokenInfo _tokenInfo = new()
    {
        Date = DateTime.Now.AddYears(-1)
    };

    static readonly object _locker = new();

    /// <summary>
    /// 会议客户端
    /// </summary>
    /// <param name="config"></param>
    public VZLiveClient(LiveOption? config) : base(config)
    {
        if (config == null) throw new Exception("LiveOption不能为空");
        if (config.AppId.IsNullOrEmpty()) throw new Exception("AppId不能为空");
        if (config.AppSecret.IsNullOrEmpty()) throw new Exception("AppSecret不能为空");
        if (config.AuthSecret.IsNullOrEmpty()) throw new Exception("AuthSecret不能为空");
        if (config.Url.IsNullOrEmpty()) throw new Exception("Url不能为空");

        _httpClient = HttpClientProxy.Create("https://openapi.vzan.com", timeout: 180, useLog: true);
    }

    /// <summary>
    /// 会议客户端
    /// </summary>
    public VZLiveClient() : this(NacosConfigUtil.Read<LiveOption>("LiveOption"))
    {
    }

    /// <summary>
    /// 获取基础请求头
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    protected override Dictionary<string, string> GetBaseHeaders()
    {
        Dictionary<string, string> headers = [];
        lock (_locker)
        {
            if (_tokenInfo.Date > DateTime.Now)
            {
                headers.Add("Authorization", $"Bearer {_tokenInfo.Token}");
                return headers;
            }
            var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
            var ms = (long)timeSpan.TotalMilliseconds;
            var signkey = $"{_liveOption.AppSecret}{ms}".GetMD5Str();
            var body = $"{{\"account\":\"{_liveOption.AppId}\",\"signkey\":\"{signkey}\",\"timestamp\":{ms}}}";
            var response = _httpClient.Post("/api/v2/token/get_token", body);

            var data = SerializeUtil.Deserialize<VZResult<VZToken>>(response);
            if (data != null && data.code == 0)
            {
                _tokenInfo.Token = data.data.token;
                _tokenInfo.Date = DateTime.Now.AddHours(1);
                headers.Add("Authorization", $"Bearer {_tokenInfo.Token}");
                return headers;
            }
            else throw new Exception($"获取vz的token异常，code:{data?.code ?? 999}, msg:{data?.msg ?? ""}");
        }
    }

    /// <summary>
    /// 获取直播间地址，
    /// 密钥验证，
    /// https://paas.vzan.com/pages/document.html#/1124
    /// </summary>
    /// <param name="topicId"></param>
    /// <param name="secret"></param>
    /// <param name="userId"></param>
    /// <param name="nickName"></param>
    /// <param name="avatar"></param>
    /// <returns></returns>
    [Obsolete("微赞2025-5-15官方作废此方法")]
    public string GetOldLiveUrl(string topicId, string secret, string userId, string nickName, string avatar = "")
    {
        try
        {
            //此处不加时，几秒内就会过期
            var third_ts = DateTimeUtil.Now.AddSeconds(20).ToUnixTimeStamp(true);
            if (avatar.IsNullOrEmpty())
                avatar = "https%3A%2F%2Fi.weizan.cn%2Fzhibo%2Fimages%2Fdefaultuser.jpg";
            var time_sign = $"{userId}{third_ts}{_liveOption.AuthSecret}".GetMD5Str().ToLower();
            return $"{_liveOption.Url}/live/page/{topicId}?tuid={userId}&third_ts={third_ts}&nickname={nickName}&avatar={avatar}&time_sign={time_sign}";
        }
        catch (Exception e)
        {
            Logger.Error(e);
            return "1";
        }
    }

    /// <summary>
    /// 获取直播间地址，
    /// 加密传参，
    /// https://paas.vzan.com/pages/document.html#/1124
    /// </summary>
    /// <param name="topicId"></param>
    /// <param name="secret"></param>
    /// <param name="userId"></param>
    /// <param name="nickName"></param>
    /// <param name="avatar"></param>
    /// <returns></returns>
    public string GetLiveUrl(string topicId, string secret, string userId, string nickName, string avatar = "")
    {
        try
        {
            //此处不加时，几秒内就会过期
            var third_ts = DateTimeUtil.Now.AddSeconds(20).ToUnixTimeStamp(true);
            if (avatar.IsNullOrEmpty())
                avatar = "https%3A%2F%2Fi.weizan.cn%2Fzhibo%2Fimages%2Fdefaultuser.jpg";
            var data = new { tuid = userId, third_ts = third_ts, nickName = nickName, avatar = avatar }.ToJson(true, false, false);
            var sm4 = SM4Util.Encrypt(secret, data);
            return $"{_liveOption.Url}/live/page/{topicId}?data={sm4.UrlEncode()}";
        }
        catch (Exception e)
        {
            Logger.Error(e);
            return "1";
        }
    }

    /// <summary>
    /// 获取直播数据
    /// </summary>
    /// <param name="topicId"></param>
    /// <param name="pageSize"></param>
    /// <param name="pageIndex"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public VZLiveUserList GetLiveUserPagedList(long topicId, int pageSize, int pageIndex)
    {
        var body = $"{{\"id\":{topicId},\"page_size\":{pageSize},\"page\":{pageIndex}}}";
        var data = _httpClient.PostJson<VZResult<VZLiveUserList>>("api/v2/topic/get_live_user_list_with_total", body, GetBaseHeaders());
        return data.data;
    }

    /// <summary>
    /// 获取直播间用户数据
    /// </summary>
    /// <param name="topicId"></param>
    /// <returns></returns>
    public List<VZLiveUserInfo> GetLiveUserList(long topicId)
    {
        var result = new List<VZLiveUserInfo>();
        var pageIndex = 1;
        var pageSize = 100;
        var data = GetLiveUserPagedList(topicId, pageSize, pageIndex);
        if (data.List == null || data.List.Count == 0) return result;
        while (result.Count < data.Total)
        {
            result.AddRange(data.List);
            var p = data.Total - result.Count;
            if (p <= 0) break;
            if (p < 100) pageSize = p;
            pageIndex++;
            data = GetLiveUserPagedList(topicId, pageSize, pageIndex);
        }
        return result;
    }



    /// <summary>
    /// 解绑第三方账号关联，2个参数至少传一个，需在管理后台申请开通
    /// </summary>
    /// <param name="uidList">项目中用户id</param>
    /// <param name="vzUidList">微赞的用户id</param>
    /// <returns></returns>
    public bool UnbindUserid(List<string>? uidList, List<long>? vzUidList)
    {
        try
        {
            var inputOjb = new
            {
                tuid_list = uidList,
                uid_list = vzUidList
            };
            var input = SerializeUtil.Serialize(inputOjb);
            var data = _httpClient.PostJson<VZResult<bool>>("/api/v2/live/third_relation_unbind", input, GetBaseHeaders());
            return data.data;
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
        return false;
    }
    /// <summary>
    /// 获取直播间用户详情，需在管理后台申请开通
    /// </summary>
    /// <param name="vzUserId">微赞UserId</param>
    /// <param name="uid">项目中uid</param>
    /// <param name="openId">项目中openId</param>
    /// <returns></returns>
    public VZUserInfo GetUserDetail(long? vzUserId, string? uid, string? openId)
    {
        var inputOjb = new
        {
            userId = vzUserId,
            tuid = uid,
            third_openid = openId
        };
        var input = SerializeUtil.Serialize(inputOjb);
        var data = _httpClient.PostJson<VZResult<VZUserInfo>>("/api/v2/live/get_live_user_detail", input, GetBaseHeaders());
        return data.data;
    }
}
