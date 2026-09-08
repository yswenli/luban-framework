/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Web.Core.Utils
*文件名： SmsExtention
*版本号： V1.0.0.0
*唯一标识：ba773eaf-9688-41bc-b9a6-1139cd402004
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/14 13:45:37
*描述：短信扩展
*
*=====================================================================
*修改标记
*修改时间：2022/7/14 13:45:37
*修改人： walle.wen
*版本号： V1.0.0.0
*描述：短信扩展
*
*****************************************************************************/

using LuBan.Common.Sms.Models;

namespace LuBan.Web.Core.Utils;

/// <summary>
/// 短信扩展
/// </summary>
public static class SmsExtention
{
    /// <summary>
    /// 注册短信配置 dbconfig 读取委托（优先 db_config 表，回退 appsettings.json）
    /// </summary>
    /// <param name="services"></param>
    internal static void InitSms(this IServiceCollection services)
    {
        SmsConfigResolver.Register(() =>
        {
            try
            {
                var dbConfig = new DbRepository<DbConfig>()
                    .First(q => q.Code == CommonConst.SysSmsCode && q.IsDelete == false);
                if (dbConfig == null || dbConfig.Value.IsNullOrEmpty()) return null;
                var smsConfig = SerializeUtil.Deserialize<SmsOption>(dbConfig.Value);
                if (smsConfig != null)
                {
                    ConsoleUtil.WriteLineWithCount("正在初始化Sms", color: ConsoleColor.Green);
                    return smsConfig;
                }
                return null;
            }
            catch
            {
                return null;
            }
        });
    }

    /// <summary>
    /// 校验手机验证码（万能验证码直接通过，校验成功后标记已使用）
    /// </summary>
    public static void ValidatePhoneVerifyCode(string phoneNumber, string code)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw FriendlyError.Ex(FrameworkErrors.Common.PhoneEmpty);

        var globalCode = HostingOptions.Default.AppOptions.GloabVerifyCode;
        if (!string.IsNullOrEmpty(globalCode) && code == globalCode)
            return;

        var key = CacheConst.KeyPhoneVerCode + phoneNumber;
        var cached = MemoryCache.Instance.Get<PhoneVerifyCodeInfo>(key);

        if (cached == null)
            throw FriendlyError.Ex(FrameworkErrors.Common.CaptchaError);

        if (cached.IsUsed)
            throw FriendlyError.Ex(FrameworkErrors.Common.SmsVerifyCodeUsed);

        if (cached.Code != code)
            throw FriendlyError.Ex(FrameworkErrors.Common.CaptchaError);

        cached.IsUsed = true;
        MemoryCache.Instance.Set(key, cached, TimeSpan.FromMinutes(GetSmsExpireMinutes()));
    }

    private static int GetSmsExpireMinutes()
    {
        return new SmsSender().Option.VerifyCodeExpireMinutes;
    }

    /// <summary>
    /// 发送手机验证码（未使用未过期时复用已有验证码，否则生成新码）
    /// </summary>
    public static async Task<SmsRequestResult> SendVerifyCodeAsync(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw FriendlyError.Ex(FrameworkErrors.Common.PhoneEmpty);

        var expireMinutes = GetSmsExpireMinutes();
        var key = CacheConst.KeyPhoneVerCode + phoneNumber;
        var cached = MemoryCache.Instance.Get<PhoneVerifyCodeInfo>(key);
        string code;

        if (cached != null && !cached.IsUsed && cached.CreateTime.AddMinutes(expireMinutes) > DateTime.Now)
        {
            code = cached.Code;
        }
        else
        {
            code = RandomUtil.GetRndCodeStr(4, 2);
            cached = new PhoneVerifyCodeInfo
            {
                Code = code,
                CreateTime = DateTime.Now,
                IsUsed = false
            };
        }

        MemoryCache.Instance.Set(key, cached, TimeSpan.FromMinutes(expireMinutes));

        var sender = new SmsSender();
        return await sender.SendValideCodeAsync(phoneNumber, code);
    }
}
