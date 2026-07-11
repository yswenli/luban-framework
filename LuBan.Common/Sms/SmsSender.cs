/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Common.Sms
*文件名： SmsSender
*版本号： V1.0.0.0
*唯一标识：a2bf5189-f1eb-440d-b878-770892306f83
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/5 16:15:11
*描述：
*
*=================================================
*修改标记
*修改时间：2023/12/5 16:15:11
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
using LuBan.Common.Sms.Models;

namespace LuBan.Common.Sms;

/// <summary>
/// 发送短信
/// </summary>
public class SmsSender : BaseSingleInstance<SmsSender>
{

    string _userName, _password, _sign;
    long _tpId = 0;

    static HttpClientProxy _httpClientUtil;

    SmsOption _smsOption;

    /// <summary>
    /// 配置
    /// </summary>
    public SmsOption Option
    {
        get
        {
            return _smsOption;
        }
    }

    /// <summary>
    /// 发送短信
    /// </summary>
    /// <param name="smsOption"></param>
    public SmsSender(SmsOption smsOption)
    {
        _userName = smsOption.ZhuTong.UserName;
        _password = smsOption.ZhuTong.Password;
        _sign = smsOption.ZhuTong.Signature;
        _tpId = smsOption.ZhuTong.TemplateId;
        _smsOption = smsOption;
        _httpClientUtil = HttpClientProxy.Create("https://api.mix2.zthysms.com", useLog: true);
    }



    /// <summary>
    /// 发送短信
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="pwd"></param>
    /// <param name="sign"></param>
    /// <param name="tpId"></param>
    public SmsSender(string userName, string pwd, string sign, long tpId) : this(new SmsOption()
    {
        ZhuTong = new ZhuTongSmsSetting()
        {
            UserName = userName,
            Password = pwd,
            Signature = sign,
            TemplateId = tpId
        }
    })
    {

    }

    /// <summary>
    /// 发送短信
    /// </summary>
    public SmsSender() : this(NacosConfigUtil.Read<SmsOption>() ?? throw new Exception("读取短信配置失败"))
    {

    }


    string Encrypt(string tKey)
    {
        string password = MD5Util.GetMD5Str(_password).ToLower();

        password = MD5Util.GetMD5Str(password + tKey);

        return password.ToLower();
    }

    /// <summary>
    /// 发送模板消息，不带参数
    /// </summary>
    /// <param name="tpId"></param>
    /// <param name="mobiles"></param>
    /// <returns></returns>
    public async Task<SmsRequestResult> SendTemplaMsgsAsync(long tpId, List<string> mobiles)
    {
        var tKey = DateTimeUtil.UtcNow.ToUnixTimeStamp(false);

        var data = new
        {
            username = _userName,
            password = Encrypt(tKey.ToString()),
            tKey = tKey,
            tpId = tpId,
            signature = _sign,
            ext = string.Empty,
            extend = string.Empty,
            records = mobiles
        };
        return await _httpClientUtil.PostJsonAsync<SmsRequestResult>("/v2/sendSmsTp", data.ToJson());
    }

    /// <summary>
    /// 发送模板消息，带参数
    /// </summary>
    /// <param name="tpId"></param>
    /// <param name="mobileAndMsgs"></param>
    public async Task<SmsRequestResult> SendTemplaMsgsAsync(long tpId, List<TemplateMsgInfo> mobileAndMsgs)
    {
        var tKey = DateTimeUtil.UtcNow.ToUnixTimeStamp(false);
        var data = new
        {
            username = _userName,
            password = Encrypt(tKey.ToString()),
            tKey = tKey,
            tpId = tpId,
            signature = _sign,
            ext = string.Empty,
            extend = string.Empty,
            records = mobileAndMsgs
        };
        return await _httpClientUtil.PostJsonAsync<SmsRequestResult>("/v2/sendSmsTp", data.ToJson());
    }

    /// <summary>
    /// 发送手机短信验证码
    /// </summary>
    /// <param name="tpId"></param>
    /// <param name="phoneNumber"></param>
    /// <param name="verifyCode"></param>
    /// <returns></returns>
    public async Task<SmsRequestResult> SendValideCodeAsync(string phoneNumber, string verifyCode)
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
        return await SendTemplaMsgsAsync(_tpId, data);
    }
}
