/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Controllers
*文件名： BaseAdminController
*版本号： V1.0.0.0
*唯一标识：e4de6ae0-0cfd-4fcb-b628-ac88eb8b5b1d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/8/7 16:28:31
*描述：框架中管理端基础控制器类
*
*=================================================
*修改标记
*修改时间：2025/8/7 16:28:31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：框架中管理端基础控制器类
*
*****************************************************************************/
namespace LuBan.Web.Core;


/// <summary>
/// 框架中管理端基础控制器类
/// </summary>
[Route(RouteConst.ROUTE_TEMPLATE_ADMIN)]
[ApiExplorerSettings(GroupName = "admin")]
[ForbiddenAccess]
public abstract class BaseAdminController : BaseApiController
{

}
