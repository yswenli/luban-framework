namespace LuBan.Lives;


/// <summary>
/// 配置,
/// https://wiki.meeting.talkmed.com/
/// 测试秘钥：     
/// 正式秘钥： tk62a982fd03ebf    00e4037ccf189c4bbb16b22426359ee7
/// </summary>
public class LiveOption
{
    /// <summary>
    /// appid
    /// </summary>
    public string AppId { get; set; }
    /// <summary>
    /// appsecret
    /// </summary>
    public string AppSecret { get; set; }
    /// <summary>
    /// 地址,
    /// https://devapimeeting.talkmed.com
    /// https://apimeeting.talkmed.com
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// https://wiki.meeting.talkmed.com/#/?id=%e5%8f%82%e4%bc%9a%e6%97%b6%e5%ba%8f%e5%9b%be
    /// 正式地址：https://meeting.talkmed.com/oauth/authorize
    /// 测试地址：https://devmeeting.talkmed.com/oauth/authorize
    /// </summary>
    public string? AuthorizeUrl { get; set; } = "";
    /// <summary>
    /// 签权密码
    /// </summary>
    public string? AuthSecret { get; set; } = "";

    /// <summary>
    /// 用户名
    /// </summary>
    public string? UserName { get; set; } = "";
    /// <summary>
    /// 密码
    /// </summary>
    public string? Password { get; set; } = "";
    /// <summary>
    /// 盐
    /// </summary>
    public string? Salt { get; set; } = "";


    /// <summary>
    /// 配置
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="appSecret"></param>
    /// <param name="url"></param>
    /// <param name="authorizeUrl"></param>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    /// <param name="salt"></param>
    public LiveOption(string appId, string appSecret, string url, string authorizeUrl, string userName, string password, string salt)
    {
        AppId = appId;
        AppSecret = appSecret;
        Url = url;
        AuthorizeUrl = authorizeUrl;
        UserName = userName;
        Password = password;
        Salt = salt;
    }
    /// <summary>
    /// 配置
    /// </summary>
    public LiveOption()
    {

    }
}