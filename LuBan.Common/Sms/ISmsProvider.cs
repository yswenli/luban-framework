/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common.Sms
*文件名： ISmsProvider.cs
*版本号： V1.0.0.0
*唯一标识：104ad9c0-d39f-4802-b2b4-c45c787ef740
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/3 11:48:27
*描述：ISmsProvider 类
*
*=================================================
*修改标记
*修改时间：2026/9/3 11:48:27
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ISmsProvider 类
*
*****************************************************************************/

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
