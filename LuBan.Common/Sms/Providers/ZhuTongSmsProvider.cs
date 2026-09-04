/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common.Sms.Providers
*文件名： ZhuTongSmsProvider.cs
*版本号： V1.0.0.0
*唯一标识：67f2fdcc-f7b9-4192-b300-077001436a7b
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/3 11:50:00
*描述：ZhuTongSmsProvider 类
*
*=================================================
*修改标记
*修改时间：2026/9/3 11:50:00
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ZhuTongSmsProvider 类
*
*****************************************************************************/

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
