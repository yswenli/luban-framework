/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Controllers
*文件名： BaseOpenController
*版本号： V1.0.0.0
*唯一标识：9c531e90-48ce-4d47-a058-b1af81f310e2
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/8/7 16:30:28
*描述：框架中开放接口基础控制器类
*
*=================================================
*修改标记
*修改时间：2025/8/7 16:30:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：框架中开放接口基础控制器类
*
*****************************************************************************/
namespace LuBan.Web.Core;


/// <summary>
/// 框架中开放接口基础控制器类
/// </summary>
[Route(RouteConst.ROUTE_TEMPLATE_OPEN)]
[ApiExplorerSettings(GroupName = "open")]
[AllowAccess]
[AllowAnonymous]
[OpenApiAccess]
[AraParameterFilter]
public abstract class BaseOpenController : BaseApiController
{

}
