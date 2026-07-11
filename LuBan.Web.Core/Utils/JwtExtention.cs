/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Utils
*文件名： JwtExtention
*版本号： V1.0.0.0
*唯一标识：bf675dd7-f735-4b2f-b45f-a707c6f16bd0
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/3/31 10:32:26
*描述：JwtExtention
*
*=================================================
*修改标记
*修改时间：2025/3/31 10:32:26
*修改人： yswenli
*版本号： V1.0.0.0
*描述：JwtExtention
*
*****************************************************************************/
namespace LuBan.Web.Core.Utils;

/// <summary>
/// JwtExtention
/// </summary>
public static class JwtExtention
{
    /// <summary>
    /// 获取用户信息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="request"></param>
    /// <returns></returns>
    public static T? GetJwtDataFromContext<T>(this HttpRequest request) where T : class, new()
    {
        try
        {
            if (request.HttpContext.User != null
            && request.HttpContext.User.Identity != null
            && request.HttpContext.User.Identity.IsAuthenticated)
            {
                var cliams = request.HttpContext.User.Identities.ToList().First().Claims;
                return cliams.ToModel<T>();
            }
        }
        catch (Exception ex)
        {
            Logger.Error("JWTDataExtention.GetJwtDataFromContext", ex, request);
        }
        return default;
    }

    /// <summary>
    /// 获取jwt token string
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static string? GetJwtTokenString(this HttpRequest request)
    {
        return request.GetJwtToken();
    }


    /// <summary>
    /// 创建快捷的jwt token
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="tenantId"></param>
    /// <param name="openId"></param>
    /// <param name="tokenExpire"></param>
    /// <returns></returns>
    public static string CreateJwtToken(long userId, long tenantId, string openId, int tokenExpire = 3600)
    {
        return JwtEncryption.Encrypt(new Dictionary<string, object>
        {
            { ClaimConst.UserId, userId },
            { ClaimConst.TenantId, tenantId },
            { ClaimConst.OpenId, openId }
        }, tokenExpire);
    }

    /// <summary>
    /// 创建快捷的jwt token
    /// </summary>
    /// <param name="user"></param>
    /// <param name="openId"></param>
    /// <param name="tokenExpire"></param>
    /// <returns></returns>
    public static string CreateJwtToken(DbUser user, string openId, int tokenExpire = 7200)
    {
        return CreateJwtToken(user.Id, user.TenantId ?? LuBanOrmConst.DefaultTenantId, openId, tokenExpire);
    }

    /// <summary>
    /// 创建快捷的jwt token
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="tokenExpire"></param>
    /// <returns></returns>
    public static string CreateJwtToken(long userId, int tokenExpire = 7200)
    {
        var dbUser = WebApp.ServiceCache.GetOrSet($"{CacheConst.KeyUser}{userId}", (k) =>
        {
            var user = new DbRepository<DbUser>()
                    .Includes(q => q.Position)
                    .Includes(q => q.Organization)
                    .Includes(q => q.UserRoles)
                    .Includes(q => q.ManagerUser)
                    .Where(q => q.IsDelete == false && q.Id == userId).First();
            ArgumentNullException.ThrowIfNull(user, nameof(userId));
            return user;
        }, TimeSpan.FromSeconds(tokenExpire));

        ArgumentNullException.ThrowIfNull(dbUser, nameof(userId));

        return CreateJwtToken(dbUser, string.Empty, tokenExpire);
    }
}
