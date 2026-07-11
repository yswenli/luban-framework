/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Common.Consts
*文件名： ClaimConst
*版本号： V1.0.0.0
*唯一标识：aa33a588-84e3-40ca-8d1c-689ade58c34c
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/1 19:17:22
*描述：Claim相关常量
*
*=================================================
*修改标记
*修改时间：2023/12/1 19:17:22
*修改人： yswenli
*版本号： V1.0.0.0
*描述：Claim相关常量
*
*****************************************************************************/
namespace LuBan.Common.Consts;


/// <summary>
/// Claim相关常量
/// </summary>
[Const("Claim相关常量")]
public class ClaimConst
{
    /// <summary>
    /// 用户Id
    /// </summary>
    public const string UserId = "UserId";

    /// <summary>
    /// 账号
    /// </summary>
    public const string Account = "Account";

    /// <summary>
    /// 真实姓名
    /// </summary>
    public const string RealName = "RealName";

    /// <summary>
    /// 昵称
    /// </summary>
    public const string NickName = "NickName";

    /// <summary>
    /// 租户Id
    /// </summary>
    public const string TenantId = "TenantId";

    /// <summary>
    /// RoleId
    /// </summary>
    public const string RoleId = "RoleId";

    /// <summary>
    /// 组织机构Id
    /// </summary>
    public const string OrgId = "OrgId";

    /// <summary>
    /// 组织机构名称
    /// </summary>
    public const string OrgName = "OrgName";

    /// <summary>
    /// 组织机构类型
    /// </summary>
    public const string OrgType = "OrgType";

    /// <summary>
    /// 微信OpenId
    /// </summary>
    public const string OpenId = "OpenId";

    /// <summary>
    /// 登录模式PC、APP
    /// </summary>
    public const string LoginMode = "LoginMode";
}
