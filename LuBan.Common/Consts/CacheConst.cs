/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Common.Consts
*文件名： CacheConst
*版本号： V1.0.0.0
*唯一标识：efa1ac45-4030-42cd-9b49-1b5f84afb05f
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 17:53:18
*描述：
*
*=================================================
*修改标记
*修改时间：2023/12/4 17:53:18
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Common.Consts;


/// <summary>
/// 缓存相关常量
/// </summary>
[Const("缓存相关常量")]
public class CacheConst
{
    /// <summary>
    /// 系统缓存
    /// </summary>
    public const string KeySystem = "sys-luban-framework:";

    /// <summary>
    /// 分布式锁缓存
    /// </summary>
    public const string KeyDistributeLock = "sys-luban-framework:distribute-lock:";

    /// <summary>
    /// 用户缓存
    /// </summary>
    public const string KeyUser = "sys-luban-framework:user:";

    /// <summary>
    /// 用户菜单缓存
    /// </summary>
    public const string KeyUserMenu = "sys-luban-framework:user-menu:";

    /// <summary>
    /// 用户权限缓存（按钮集合）
    /// </summary>
    public const string KeyUserButton = "sys-luban-framework:user-button:";

    /// <summary>
    /// 用户机构缓存
    /// </summary>
    public const string KeyUserOrg = "sys-luban-framework:user-org:";

    /// <summary>
    /// 角色最大数据范围缓存
    /// </summary>
    public const string KeyRoleMaxDataScope = "sys-luban-framework:role-max-data-scope:";

    /// <summary>
    /// 在线用户缓存
    /// </summary>
    public const string KeyUserOnline = "sys-luban-framework:user-online:";

    /// <summary>
    /// 图形验证码缓存
    /// </summary>
    public const string KeyVerCode = "sys-luban-framework:ver-code:";

    /// <summary>
    /// 手机验证码缓存
    /// </summary>
    public const string KeyPhoneVerCode = "sys-luban-framework:phone-ver-code:";

    /// <summary>
    /// 租户缓存
    /// </summary>
    public const string KeyTenant = "sys-luban-framework:tenant";

    /// <summary>
    /// 常量下拉框
    /// </summary>
    public const string KeyConst = "sys-luban-framework:const:";

    /// <summary>
    /// 所有缓存关键字集合
    /// </summary>
    public const string KeyAll = "sys-luban-framework:keys";

    /// <summary>
    /// SqlSugar二级缓存
    /// </summary>
    public const string SqlSugar = "sys-luban-framework:sqlsugar:";

    /// <summary>
    /// 开放接口身份缓存
    /// </summary>
    public const string KeyOpenAccess = "sys-luban-framework:open-access:";

    /// <summary>
    /// 开放接口身份随机数缓存
    /// </summary>
    public const string KeyOpenAccessNonce = "sys-luban-framework:open-access-nonce:";

    /// <summary>
    /// 缓存禁止访问admin api 角色
    /// </summary>
    public const string KeyForbiddenAccessRoles = "sys-luban-framework:forbidden-access-roles";

    /// <summary>
    /// api缓存
    /// </summary>
    public const string KeyApiCache = "sys-luban-framework:api-cache:";
}
