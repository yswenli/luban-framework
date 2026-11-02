/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：河之洲
*命名空间：LuBan.Web.Core.Attributes
*文件名： AllowAccessAttribute
*版本号： V1.0.0.0
*唯一标识：09d9d48a-f262-4997-9a25-481f0aa97502
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/11/12 11:40:22
*描述：允许访问，与ForbiddenAccessAtrribute相反
*
*=================================================
*修改标记
*修改时间：2024/11/12 11:40:22
*修改人： yswenli
*版本号： V1.0.0.0
*描述：允许访问，与ForbiddenAccessAtrribute相反
*
*****************************************************************************/
namespace LuBan.Web.Core.Attributes;

/// <summary>
/// 用户访问接口角色允许过滤器，
/// 允许访问，与ForbiddenAccessAtrribute相反
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class AllowAccessAttribute : BaseFilterAttribute, IOrderedFilter
{
    /// <summary>
    /// OnActionExecutionAsync
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await next.Invoke();
    }

    /// <summary>
    /// OnResultExecutionAsync
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        await next.Invoke();
    }
}
