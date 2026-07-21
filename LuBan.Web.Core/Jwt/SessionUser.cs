/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：
*文件名： 
*版本号： V1.0.0.0
*唯一标识：a5bb6173-b22d-4edd-852f-9b02bb075167
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/11/03 14:00:15
*描述：当前jwt中解析的用户
*
*=================================================
*修改标记
*修改时间：2023/11/03 14:00:15
*修改人： yswenli
*版本号： V1.0.0.0
*描述：当前jwt中解析的用户
*
*****************************************************************************/

namespace LuBan.Web.Core.Jwt;

/// <summary>
/// 当前jwt中解析的用户
/// </summary>
public static class SessionUser
{
    static readonly long _tenantId = LuBanOrmConst.DefaultTenantId;

    /// <summary>
    /// 当前登录用户id
    /// </summary>
    public static long UserId
    {
        get
        {
            if (WebApp.HttpContext == null) return 0;
            var value = WebApp.User.FindFirst(ClaimConst.UserId)?.Value;
            return value.IsNullOrEmpty() ? 0 : long.Parse(value);
        }
    }

    /// <summary>
    /// 当前jwt登录的用户信息
    /// </summary>
    public static DbUser? CurrentUser
    {
        get
        {
            if (WebApp.HttpContext == null) return null;
            var uid = UserId;
            if (uid < 1) return null;
            return WebApp.ServiceCache.GetOrSet($"{CacheConst.KeyUser}{uid}", (k) =>
            {
                return new DbRepository<DbUser>()
                    .AsQueryable()
                    .ClearFilter()
                    .Includes(q => q.Position)
                    .Includes(q => q.Organization)
                    .Includes(q => q.UserRoles)
                    .Includes(q => q.ManagerUser)
                    .Where(q => q.IsDelete == false && q.Id == uid).First();
            }, TimeSpan.FromSeconds(WebApp.HostingOptions.AppOptions.JwtAuthConfig.AccessExpiration));
        }
    }

    /// <summary>
    /// 租户Id，
    /// 无特别的的租户，则返回默认库连接的租户Id
    /// </summary>
    public static long TenantId
    {
        get
        {
            if (WebApp.HttpContext == null) return _tenantId;
            var tId = WebApp.User.FindFirst(ClaimConst.TenantId)?.Value;
            return tId.IsNullOrWhiteSpace() ? _tenantId : long.Parse(tId);
        }
    }

    /// <summary>
    /// 当前登录用户账号
    /// </summary>
    public static string Account
    {
        get => CurrentUser?.Account ?? "";
    }

    /// <summary>
    /// 当前登录用户姓名
    /// </summary>
    public static string RealName
    {
        get => CurrentUser?.RealName ?? "";
    }


    /// <summary>
    /// 当前登录用户是否是超级管理员
    /// </summary>
    public static bool IsSuperAdmin
    {
        get => UserId == LuBanOrmConst.SuperAdminId;
    }

    /// <summary>
    /// 当前登录用户角色
    /// </summary>
    public static List<long> RoleIds
    {
        get
        {
            return CurrentUser?.UserRoles?.Select(q => q.RoleId).ToList() ?? [];
        }
    }

    /// <summary>
    /// 机构Id
    /// </summary>
    public static long OrgId
    {
        get
        {
            return CurrentUser?.OrgId ?? 0;
        }
    }
    /// <summary>
    /// 机构名称
    /// </summary>
    public static string OrgName
    {
        get
        {
            return CurrentUser?.Organization?.Name ?? "";
        }
    }
    /// <summary>
    /// 机构类型
    /// </summary>
    public static string OrgType
    {
        get
        {
            return CurrentUser?.Organization?.Type ?? "";
        }
    }
    /// <summary>
    /// openid
    /// </summary>
    public static string OpenId
    {
        get
        {
            if (WebApp.HttpContext == null) return "";
            return WebApp.User.FindFirst(ClaimConst.OpenId)?.Value ?? "";
        }
    }
}