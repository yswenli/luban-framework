/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.AspNetCore
*文件名： ApiResultConvertion
*版本号： V1.0.0.0
*唯一标识：261f9445-8d20-4bf0-b701-3782ef66a8fb
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/8/1 17:33:17
*描述：接口返回值统一处理转换
*
*=================================================
*修改标记
*修改时间：2024/8/1 17:33:17
*修改人： yswenli
*版本号： V1.0.0.0
*描述：接口返回值统一处理转换
*
*****************************************************************************/

namespace LuBan.Web.Core.Attributes;
/// <summary>
/// 接口返回值统一处理转换
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class ApiResultConvertionAttribute : BaseFilterAttribute, IAsyncActionFilter, IAsyncResultFilter, IOrderedFilter
{
    /// <summary>
    /// 执行顺序
    /// </summary>
    public new int Order => 1;

    /// <summary>
    /// 接口返回值统一处理转换
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        //处理接口业务流程
        var actionContext = await next.Invoke();

        //如果当前接口已经报错了，则不转换结果
        if (actionContext.Exception == null)
        {
            //对默认接口返回值进行全局统一封装
            actionContext.Result = await actionContext.ConvertToUnifiedResult();
        }
    }

    /// <summary>
    /// 接口返回值统一处理转换
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        await next.Invoke();
    }
}
