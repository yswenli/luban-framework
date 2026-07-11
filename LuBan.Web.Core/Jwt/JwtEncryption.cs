/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Web.Core.JWT
*文件名： JwtEncryption
*版本号： V1.0.0.0
*唯一标识：b2450e72-c354-4081-8140-2c0009a089de
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 17:05:36
*描述：JWT 加解密
*
*=================================================
*修改标记
*修改时间：2023/12/4 17:05:36
*修改人： yswenli
*版本号： V1.0.0.0
*描述：JWT 加解密
*
*****************************************************************************/

namespace LuBan.Web.Core.Jwt;

/// <summary>
/// JWT 加解密,
/// Microsoft.IdentityModel.Tokens 高版本有兼容性问题
/// </summary>
public class JwtEncryption
{
    /// <summary>
    ///  创建快捷的jwt token
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="tenantId"></param>
    /// <param name="openId"></param>
    /// <param name="tokenExpire"></param>
    /// <returns></returns>
    public static string CreateJwtToken(long userId,
        long tenantId,
        string openId,
        int tokenExpire = 20)
    {
        return JwtEncryption.Encrypt(new Dictionary<string, object>
        {
            { ClaimConst.UserId, userId },
            { ClaimConst.TenantId, tenantId },
            { ClaimConst.OpenId, openId }
        }, tokenExpire);
    }

    /// <summary>
    /// 生成 Token
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="expiredTime">过期时间（秒），最大支持 13 年</param>
    /// <returns></returns>
    public static string Encrypt(Dictionary<string, object> payload, long? expiredTime = null)
    {
        var jwtSettings = GetJwtSettings();
        JwtUserInfo jwtUserInfo = payload;
        var et = jwtSettings.AccessExpiration;
        if (expiredTime.HasValue)
        {
            et = Convert.ToInt32(expiredTime.Value);
        }
        var jwtPackage = new JWTPackage<JwtUserInfo>(jwtUserInfo, et, jwtSettings.Secret);
        jwtPackage.Payload["aud"] = jwtSettings.Audience;
        jwtPackage.Payload["iss"] = jwtSettings.Issuer;
        return jwtPackage.GetToken();
    }


    /// <summary>
    /// 获取 JWT 配置
    /// </summary>
    /// <returns></returns>
    public static JwtAuthConfig GetJwtSettings()
    {
        return HostingOptions.Default.AppOptions.JwtAuthConfig;
    }

}