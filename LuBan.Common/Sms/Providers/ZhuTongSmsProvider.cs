using LuBan.Common.Sms.Models;

namespace LuBan.Common.Sms.Providers;

/// <summary>
/// 助通短信
/// </summary>
public class ZhuTongSmsProvider : ISmsProvider
{
    readonly ZhuTongSmsSetting _setting;

    readonly HttpClientProxy _httpClientUtil;

    /// <summary>
    /// 运营商名称
    /// </summary>
    public string ProviderName => "ZhuTong";

    /// <summary>
    /// 助通短信
    /// </summary>
    /// <param name="setting"></param>
    public ZhuTongSmsProvider(ZhuTongSmsSetting setting)
    {
        _setting = setting;
        _httpClientUtil = HttpClientProxy.Create("https://api.mix2.zthysms.com", useLog: true);
    }

    string Encrypt(string tKey)
    {
        return EncryptPassword(_setting.Password, tKey);
    }

    /// <summary>
    /// 助通密码双重 MD5 加密（纯函数，供单测）
    /// </summary>
    internal static string EncryptPassword(string password, string tKey)
    {
        string pwd = MD5Util.GetMD5Str(password).ToLower();

        pwd = MD5Util.GetMD5Str(pwd + tKey);

        return pwd.ToLower();
    }

    /// <summary>
    /// 发送模板消息，不带参数
    /// </summary>
    /// <param name="templateCode">助通 tpId 数字串</param>
    /// <param name="mobiles"></param>
    public async Task<SmsRequestResult> SendTemplateAsync(string templateCode, List<string> mobiles)
    {
        var tKey = DateTimeUtil.UtcNow.ToUnixTimeStamp(false);

        var data = new
        {
            username = _setting.UserName,
            password = Encrypt(tKey.ToString()),
            tKey = tKey,
            tpId = long.Parse(templateCode),
            signature = _setting.Signature,
            ext = string.Empty,
            extend = string.Empty,
            records = mobiles
        };
        return await _httpClientUtil.PostJsonAsync<SmsRequestResult>("/v2/sendSmsTp", data.ToJson());
    }

    /// <summary>
    /// 发送模板消息，带参数
    /// </summary>
    /// <param name="templateCode">助通 tpId 数字串</param>
    /// <param name="mobileAndMsgs"></param>
    public async Task<SmsRequestResult> SendTemplateAsync(string templateCode, List<TemplateMsgInfo> mobileAndMsgs)
    {
        var tKey = DateTimeUtil.UtcNow.ToUnixTimeStamp(false);
        var data = new
        {
            username = _setting.UserName,
            password = Encrypt(tKey.ToString()),
            tKey = tKey,
            tpId = long.Parse(templateCode),
            signature = _setting.Signature,
            ext = string.Empty,
            extend = string.Empty,
            records = mobileAndMsgs
        };
        return await _httpClientUtil.PostJsonAsync<SmsRequestResult>("/v2/sendSmsTp", data.ToJson());
    }

    /// <summary>
    /// 发送手机短信验证码
    /// </summary>
    /// <param name="phoneNumber"></param>
    /// <param name="verifyCode"></param>
    public async Task<SmsRequestResult> SendVerifyCodeAsync(string phoneNumber, string verifyCode)
    {
        var data = new List<TemplateMsgInfo>()
        {
            new TemplateMsgInfo()
            {
                Mobile = phoneNumber,
                TpContent = new Dictionary<string, string>() {
                    { "valid_code", verifyCode }
                }
            }
        };
        return await SendTemplateAsync(_setting.TemplateId.ToString(), data);
    }
}
