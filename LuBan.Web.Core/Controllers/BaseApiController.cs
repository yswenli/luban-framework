/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Web.Core
*文件名： BaseController
*版本号： V1.0.0.0
*唯一标识：739879d4-eade-4a64-8c29-2923cd188768
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/8 9:44:32
*描述：框架中基础控制器类
*
*=====================================================================
*修改标记
*修改时间：2022/7/8 9:44:32
*修改人： walle.wen
*版本号： V1.0.0.0
*描述：框架中基础控制器类
*
*****************************************************************************/

namespace LuBan.Web.Core;

/// <summary>
/// 框架中基础控制器类,
/// AllowAnonymous取消jwt较验
/// </summary>
[ApiController]
[Route(RouteConst.ROUTE_TEMPLATE_API)]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
//[PraParameterFilter] [NoPraParameterFilter]
[ApiExplorerSettings(GroupName = "default")]
public abstract class BaseApiController : BaseController
{
    /// <summary>
    /// 框架中基础控制器类
    /// </summary>
    public BaseApiController() : base()
    {
    }

    /// <summary>
    /// jwt token string
    /// </summary>
    public string JwtTokenString => HttpContext.Request.GetJwtTokenString() ?? "";
}