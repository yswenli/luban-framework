/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Lives.HuiChang
*文件名： HCMeetingClient
*版本号： V1.0.0.0
*唯一标识：ff7de66d-3ce6-483b-9946-37e2eeca9d64
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/9/18 18:02:06
*描述：会畅客户端
*
*=================================================
*修改标记
*修改时间：2024/9/18 18:02:06
*修改人： yswenli
*版本号： V1.0.0.0
*描述：会畅客户端
*
*****************************************************************************/
namespace LuBan.Lives.HuiChang;

/// <summary>
/// 会畅客户端
/// </summary>
public class HCLiveClient : BaseLiveClient, ILiveClient
{
    /// <summary>
    /// 会畅客户端
    /// </summary>
    /// <param name="liveConfig"></param>
    public HCLiveClient(LiveOption? liveConfig) : base(liveConfig)
    {

    }

    /// <summary>
    /// 会议客户端
    /// </summary>
    public HCLiveClient() : this(NacosConfigUtil.Read<LiveOption>("LiveOption"))
    {

    }

    /// <summary>
    /// 获取直播地址
    /// </summary>
    /// <param name="channelId"></param>
    /// <param name="secret"></param>
    /// <param name="userId"></param>
    /// <param name="name"></param>
    /// <param name="avatar"></param>
    /// <returns></returns>
    public string GetLiveUrl(string channelId, string secret, string userId, string name, string avatar)
    {
        try
        {
            name = string.IsNullOrWhiteSpace(name) ? "员工" : name;
            avatar = avatar.UrlEncode();
            string key = userId + secret;
            var md5 = key.GetMD5Str().ToLower();
            var url = string.Format("{0}/activity.php?a=userAssign&id={1}&userid={2}&name={3}&avatar={4}&key={5}", _liveOption.Url, channelId, userId, name, avatar, md5);
            return url;
        }
        catch
        {
            return "1";
        }
    }

    protected override Dictionary<string, string> GetBaseHeaders()
    {
        throw new NotImplementedException();
    }
}
