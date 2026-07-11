/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Web.Core.Auth
*文件名： BaseFilterAttribute
*版本号： V1.0.0.0
*唯一标识：a1bf06a5-5c65-4dc1-ad84-285681b4796d
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/12/13 17:26:44
*描述：基础过滤器
*
*=================================================
*修改标记
*修改时间：2022/12/13 17:26:44
*修改人： yswen
*版本号： V1.0.0.0
*描述：基础过滤器
*
*****************************************************************************/

namespace LuBan.Web.Core.AspNetCore;

/// <summary>
/// 基础过滤器
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public abstract class BaseFilterAttribute : Attribute, IAsyncActionFilter, IAsyncResultFilter, IOrderedFilter
{
    /// <summary>
    /// 顺序
    /// </summary>
    public int Order { get; set; } = 2;

    /// <summary>
    /// 方法执行前
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public abstract Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next);

    /// <summary>
    /// 方法执行后
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public abstract Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next);
}
