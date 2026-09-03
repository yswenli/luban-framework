/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
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
using LuBan.Common.Sms.Providers;

namespace LuBan.Common.Sms;

/// <summary>
/// 发送短信（门面：按配置路由到具体运营商 Provider，公开 API 保持兼容）
/// </summary>
public class SmsSender : BaseSingleInstance<SmsSender>
{
    readonly ISmsProvider _provider;
    readonly SmsOption _smsOption;

    /// <summary>
    /// 配置
    /// </summary>
    public SmsOption Option => _smsOption;

    /// <summary>
    /// 当前运营商 Provider（internal，仅供测试断言路由结果）
    /// </summary>
    internal ISmsProvider Provider => _provider;

    /// <summary>
    /// 测试注入构造（internal，不进入公开 API）
    /// </summary>
    internal SmsSender(ISmsProvider provider, SmsOption option)
    {
        _provider = provider;
        _smsOption = option;
    }

    /// <summary>
    /// 发送短信（按 Option.Provider 路由；Provider 为 null/空时默认 ZhuTong）
    /// </summary>
    /// <param name="smsOption"></param>
    public SmsSender(SmsOption smsOption)
    {
        _smsOption = smsOption ?? throw new ArgumentNullException(nameof(smsOption));
        _provider = CreateProvider(smsOption);
    }

    /// <summary>
    /// 发送短信（固定助通，保持旧行为）
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
    /// 发送短信（从 Nacos 读取配置）
    /// </summary>
    public SmsSender() : this(NacosConfigUtil.Read<SmsOption>() ?? throw new Exception("读取短信配置失败"))
    {
    }

    static ISmsProvider CreateProvider(SmsOption smsOption)
    {
        var provider = (smsOption.Provider ?? "").Trim();
        if (provider.Length == 0 || provider.Equals("ZhuTong", StringComparison.OrdinalIgnoreCase))
        {
            return new ZhuTongSmsProvider(smsOption.ZhuTong ?? new ZhuTongSmsSetting());
        }
        if (provider.Equals("Aliyun", StringComparison.OrdinalIgnoreCase))
        {
            return new AliyunSmsProvider(smsOption.Aliyun);   // Aliyun 为 null 或 AK 缺失时由其构造函数抛 ArgumentException
        }
        throw new ArgumentException($"不支持的短信运营商：{smsOption.Provider}（可选 ZhuTong / Aliyun）");
    }

    /// <summary>
    /// 发送模板消息，不带参数
    /// </summary>
    /// <param name="tpId"></param>
    /// <param name="mobiles"></param>
    /// <returns></returns>
    public async Task<SmsRequestResult> SendTemplaMsgsAsync(long tpId, List<string> mobiles)
    {
        return await _provider.SendTemplateAsync(tpId.ToString(), mobiles);
    }

    /// <summary>
    /// 发送模板消息，带参数
    /// </summary>
    /// <param name="tpId"></param>
    /// <param name="mobileAndMsgs"></param>
    /// <returns></returns>
    public async Task<SmsRequestResult> SendTemplaMsgsAsync(long tpId, List<TemplateMsgInfo> mobileAndMsgs)
    {
        return await _provider.SendTemplateAsync(tpId.ToString(), mobileAndMsgs);
    }

    /// <summary>
    /// 发送手机短信验证码
    /// </summary>
    /// <param name="phoneNumber"></param>
    /// <param name="verifyCode"></param>
    /// <returns></returns>
    public async Task<SmsRequestResult> SendValideCodeAsync(string phoneNumber, string verifyCode)
    {
        return await _provider.SendVerifyCodeAsync(phoneNumber, verifyCode);
    }
}
