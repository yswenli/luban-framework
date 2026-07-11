/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.AspNetCore
*文件名： RouteConst
*版本号： V1.0.0.0
*唯一标识：5ac1c593-c13b-49fb-b954-dfa023d59cfe
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/9/23 14:46:44
*描述：路由常量
*
*=================================================
*修改标记
*修改时间：2024/9/23 14:46:44
*修改人： yswenli
*版本号： V1.0.0.0
*描述：路由常量
*
*****************************************************************************/
namespace LuBan.Web.Core.AspNetCore;

/// <summary>
/// 路由常量
/// </summary>
public static class RouteConst
{
    /// <summary>
    /// api默认路由模板
    /// </summary>
    public const string ROUTE_TEMPLATE_API = "api/[controller]/[action]";
    /// <summary>
    /// api管理端路由模板
    /// </summary>
    public const string ROUTE_TEMPLATE_ADMIN = "api/admin/[controller]/[action]";
    /// <summary>
    /// api业务端路由模板
    /// </summary>
    public const string ROUTE_TEMPLATE_MOBILE = "api/mobile/[controller]/[action]";
    /// <summary>
    /// api内部服务路由模板
    /// </summary>
    public const string ROUTE_TEMPLATE_INTERNAL = "api/internal/[controller]/[action]";
    /// <summary>
    /// api开放服务端路由模板
    /// </summary>
    public const string ROUTE_TEMPLATE_OPEN = "api/open/[controller]/[action]";
}
