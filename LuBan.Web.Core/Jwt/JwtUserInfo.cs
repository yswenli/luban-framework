/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.JWT
*文件名： JwtUserInfo
*版本号： V1.0.0.0
*唯一标识：545a86da-e760-4461-8fef-eb96325fde2e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/5/7 17:19:29
*描述：jwt用户信息
*
*=================================================
*修改标记
*修改时间：2024/5/7 17:19:29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：jwt用户信息
*
*****************************************************************************/
namespace LuBan.Web.Core.Jwt;

/// <summary>
/// jwt用户信息
/// </summary>
public class JwtUserInfo
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 租户ID
    /// </summary>
    public long TenantId { get; set; } = LuBanOrmConst.DefaultTenantId;

    /// <summary>
    /// OpenID
    /// </summary>
    public string OpenId { get; set; }

    /// <summary>
    /// 隐式转换
    /// </summary>
    /// <param name="userDic">用户字典</param>
    public static implicit operator JwtUserInfo(Dictionary<string, object> userDic)
    {
        if (userDic == null || userDic.Count == 0) throw new ArgumentNullException("传入的数据不能为空");
        return new JwtUserInfo()
        {
            UserId = userDic.TryGetValue(ClaimConst.UserId, out object? userid) ? Convert.ToInt64(userid) : 0,
            TenantId = userDic.TryGetValue(ClaimConst.TenantId, out object? tenanid) ? Convert.ToInt64(tenanid) : 0,
            OpenId = userDic.TryGetValue(ClaimConst.OpenId, out object? openid) ? openid?.ToString() ?? "" : "",
        };
    }
}
