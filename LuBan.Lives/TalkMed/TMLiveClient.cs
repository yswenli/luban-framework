namespace LuBan.Lives.TalkMed;


/// <summary>
/// 拓麦客户端
/// </summary>
public class TMLiveClient : BaseLiveClient, ILiveClient
{

    /// <summary>
    /// 拓麦客户端
    /// </summary>
    /// <param name="config"></param>
    public TMLiveClient(LiveOption? config) : base(config)
    {

    }

    /// <summary>
    /// 会议客户端
    /// </summary>
    public TMLiveClient() : this(NacosConfigUtil.Read<LiveOption>("LiveOption"))
    {

    }

    /// <summary>
    /// 签名
    /// </summary>
    /// <returns></returns>
    protected override Dictionary<string, string> GetBaseHeaders()
    {
        var timestamp = DateTimeUtil.UtcNow.ToUnixTimeStamp(false).ToString();
        Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "timestamp", timestamp },
                { "appid", _liveOption.AppId },
                { "signature", SignatureHelper.GetOpenApiSignature(_liveOption.AppId, _liveOption.AppSecret, timestamp) }
            };
        return headers;
    }

    /// <summary>
    /// 签名 加密鉴权字段 sha256(appid-appsecret-authToken-timestamp) 注意中横线 token 为用户登录token
    /// </summary>
    /// <returns></returns>
    Dictionary<string, string> GetSignatureForAuthorize(string authToken)
    {
        var timestamp = DateTimeUtil.UtcNow.ToUnixTimeStamp(false).ToString();
        Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "timestamp", timestamp },
                { "appid", _liveOption.AppId },
                { "signature", SignatureHelper.GetSignatureForAuthorize(_liveOption.AppId, _liveOption.AppSecret,authToken, timestamp) }
            };
        return headers;
    }


    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="data"></param>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public async Task<T?> UploadFileAsync<T>(string resource, Dictionary<string, string> data, string filePath)
    {
        return await _httpClient.UploadFileAsync<T>(resource, GetBaseHeaders(), [], filePath);
    }

    /// <summary>
    /// 注册用户信息
    /// </summary>
    /// <param name="nickName"></param>
    /// <param name="mobile"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public async Task<Result<AuthTokenInfo>> Regist(string nickName, string mobile, string type = "mobile")
    {
        return await Post<Result<AuthTokenInfo>>("/v1/open/register", new
        {
            type = type,
            mobile = mobile,
            nickname = nickName,
            login = 1
        });
    }

    /// <summary>
    /// 获取登录凭据，创建直播用户
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<Result<AuthTokenInfo>> AuthToken(string userId)
    {
        return await Post<Result<AuthTokenInfo>>("/v1/open/auth_token", new
        {
            type = "unionid",
            unionid = userId
        });
    }


    /// <summary>
    /// 开始会议
    /// </summary>
    public async Task<Result<OpenRoomData>> OpenRoom(int userId, string title)
    {
        return await Post<Result<OpenRoomData>>("/v2/open/room", new
        {
            title = title,
            start_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            end_at = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss"),
            live_type = 1,
            user_id = userId,
            is_speaker_one_password = 1,
            access_type = 0
        });
    }

    /// <summary>
    /// 获取
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="roomId"></param>
    /// <returns></returns>
    public async Task<Result<OpenRoomData>> GetRoom(string roomId)
    {
        return await Get<Result<OpenRoomData>>($"/v2/open/room/{roomId}");
    }

    /// <summary>
    /// 重开会议
    /// </summary>
    /// <param name="roomId"></param>
    /// <returns></returns>
    //public Result<int> ReOpenRoom(long roomId)
    //{
    //    return Get<Result<int>>($"/v1/open/room/{roomId}/0");
    //}

    /// <summary>
    /// 关闭会议
    /// </summary>
    /// <param name="roomId"></param>
    /// <returns></returns>
    //public Result<int> CloseRoom(long roomId)
    //{
    //    return Get<Result<int>>($"/v1/open/room/{roomId}/1");
    //}

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="roomId"></param>
    /// <param name="filename"></param>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public async Task<Result<UploadFileInfo>?> UploadFileAsync(int userId, long roomId, string filename, string filePath)
    {
        var data = new Dictionary<string, string>
        {
            { "user_id", $"{userId}" },
            { "room_id", $"{roomId}" }
        };
        if (!string.IsNullOrEmpty(filename))
            data.Add("filename", filename);
        return await UploadFileAsync<Result<UploadFileInfo>>("/v1/open/file", data, filePath);
    }

    #region 授权并返回直播间观看地址

    /// <summary>
    /// 授权，
    /// 获取直播地址
    /// </summary>
    /// <param name="authToken"></param>
    /// <param name="roomId"></param>
    /// <param name="role"></param>
    /// <param name="channel"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public string Authorize(string authToken, string roomId, int role = 3, string channel = "", string password = "")
    {
        var headers = GetSignatureForAuthorize(authToken);
        return $"{_liveOption.AuthorizeUrl}/oauth/authorize?app_id={_liveOption.AppId}&auth_token={authToken}&timestamp={headers["timestamp"]}&signature={headers["signature"]}&platform=web&room_id={roomId}&role={role}&channel={channel}&password={password}";
    }

    #endregion

    #region 白名单

    /// <summary>
    /// 添加白名单
    /// </summary>
    /// <param name="roomId"></param>
    /// <param name="tuserId"></param>
    public async Task<string> AddWhiteList(string roomId, string tuserId)
    {
        return await Post($"/v1/open/room/{roomId}/white_list", new
        {
            type = 3,
            content = tuserId
        });
    }

    /// <summary>
    /// 删除白名单
    /// </summary>
    /// <param name="roomId"></param>
    /// <param name="tuserId"></param>
    public async Task<Result<bool>> RemoveWhiteList(string roomId, string tuserId)
    {
        return await Delete<Result<bool>>($"/v1/open/room/{roomId}/white_list", new
        {
            type = 3,
            content = tuserId
        });
    }

    /// <summary>
    /// 获取直播间地址
    /// </summary>
    /// <param name="channelId"></param>
    /// <param name="secret"></param>
    /// <param name="userId"></param>
    /// <param name="name"></param>
    /// <param name="avatar"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public string GetLiveUrl(string channelId, string secret, string userId, string name, string avatar)
    {
        throw new NotImplementedException();
    }

    #endregion
}
