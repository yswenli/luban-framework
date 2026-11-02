/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core
*文件名： BaseInternalController
*版本号： V1.0.0.0
*唯一标识：1991c9eb-d392-4387-8ab7-75b6892fe642
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/8/7 16:29:57
*描述：框架中内部接口基础控制器类
*
*=================================================
*修改标记
*修改时间：2025/8/7 16:29:57
*修改人： yswenli
*版本号： V1.0.0.0
*描述：框架中内部接口基础控制器类
*
*****************************************************************************/
namespace LuBan.Web.Core;

/// <summary>
/// 框架中内部接口基础控制器类
/// </summary>
[Route(RouteConst.ROUTE_TEMPLATE_INTERNAL)]
[ApiExplorerSettings(GroupName = "internal")]
[AllowAccess]
public abstract class BaseInternalController : BaseApiController
{

}
