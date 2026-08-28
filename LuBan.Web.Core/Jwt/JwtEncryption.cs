/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
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
*修改时间：2026/8/12 00:00:00
*修改人： yswenli
*版本号： V2.0.0.0
*描述：迁移到 System.IdentityModel.Tokens.Jwt，移除 JWT.Standard 依赖
*
*****************************************************************************/

using System.IdentityModel.Tokens.Jwt;

namespace LuBan.Web.Core.Jwt;

/// <summary>
/// JWT 加解密
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
        var et = jwtSettings.AccessExpiration;
        if (expiredTime.HasValue)
        {
            et = Convert.ToInt32(expiredTime.Value);
        }
        var secret = jwtSettings.Secret;
        if (secret.StartsWith("base64:", StringComparison.OrdinalIgnoreCase))
            secret = secret["base64:".Length..];

        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var securityKey = new SymmetricSecurityKey(keyBytes);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Aud, jwtSettings.Audience),
            new(JwtRegisteredClaimNames.Iss, jwtSettings.Issuer)
        };

        foreach (var kv in payload)
        {
            claims.Add(new Claim(kv.Key, kv.Value?.ToString() ?? ""));
        }

        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddSeconds(et),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// 解析 Token，返回 payload 字典
    /// </summary>
    /// <param name="token"></param>
    /// <param name="secret"></param>
    /// <returns></returns>
    public static Dictionary<string, object> Parse(string token, string secret)
    {
        if (secret.StartsWith("base64:", StringComparison.OrdinalIgnoreCase))
            secret = secret["base64:".Length..];

        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var securityKey = new SymmetricSecurityKey(keyBytes);

        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = securityKey,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

        var jwtToken = (JwtSecurityToken)validatedToken;
        var result = new Dictionary<string, object>();
        foreach (var claim in jwtToken.Claims)
        {
            // 跳过标准注册声明
            if (claim.Type == JwtRegisteredClaimNames.Iss
                || claim.Type == JwtRegisteredClaimNames.Aud
                || claim.Type == JwtRegisteredClaimNames.Exp
                || claim.Type == JwtRegisteredClaimNames.Nbf
                || claim.Type == JwtRegisteredClaimNames.Iat
                || claim.Type == JwtRegisteredClaimNames.Jti)
                continue;

            result[claim.Type] = claim.Value;
        }
        return result;
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
