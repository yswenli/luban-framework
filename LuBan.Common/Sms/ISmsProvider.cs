using LuBan.Common.Sms.Models;

namespace LuBan.Common.Sms;

/// <summary>
/// 短信运营商抽象
/// </summary>
public interface ISmsProvider
{
    /// <summary>
    /// 运营商名称（ZhuTong / Aliyun）
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// 发送模板短信，不带参数
    /// </summary>
    /// <param name="templateCode">模板标识（助通为 tpId 数字串，阿里云为 SMS_xxx）</param>
    /// <param name="mobiles">手机号列表</param>
    Task<SmsRequestResult> SendTemplateAsync(string templateCode, List<string> mobiles);

    /// <summary>
    /// 发送模板短信，带参数
    /// </summary>
    /// <param name="templateCode">模板标识</param>
    /// <param name="mobileAndMsgs">手机号与模板变量列表</param>
    Task<SmsRequestResult> SendTemplateAsync(string templateCode, List<TemplateMsgInfo> mobileAndMsgs);

    /// <summary>
    /// 发送验证码（模板与变量名由各 Provider 配置决定）
    /// </summary>
    /// <param name="phoneNumber">手机号</param>
    /// <param name="verifyCode">验证码</param>
    Task<SmsRequestResult> SendVerifyCodeAsync(string phoneNumber, string verifyCode);
}
