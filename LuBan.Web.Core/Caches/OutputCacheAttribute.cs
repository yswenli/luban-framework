/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Web.Core.Caches
*文件名： OutputCache
*版本号： V1.0.0.0
*唯一标识：b0a11e77-1e2d-4114-aeb0-1523665bdde8
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/12 10:55:30
*描述：aspnet api缓存
*
*=====================================================================
*修改标记
*修改时间：2022/7/12 10:55:30
*修改人： walle.wen
*版本号： V1.0.0.0
*描述：aspnet api缓存
*
*****************************************************************************/
namespace LuBan.Web.Core.Caches;

/// <summary>
/// aspnet api缓存
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class OutputCacheAttribute : Attribute, IAsyncActionFilter, IOrderedFilter
{
    /// <summary>
    /// 排序
    /// </summary>
    public int Order
    {
        get;
    } = 999;


    /// <summary>
    /// 缓存名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public int Duration
    {
        get; set;
    } = 600;

    /// <summary>
    /// 根据参数变化缓存
    /// </summary>
    public string VaryByArgument { get; set; }

    /// <summary>
    /// 根据头部变化缓存
    /// </summary>
    public string VaryByHeader { get; set; }


    /// <summary>
    /// 在执行过程使用缓存
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        IActionResult? result = context.GetCacheValue<IActionResult>(Name, VaryByArgument, VaryByHeader);

        if (result != null)
        {
            context.Result = result;
        }
        else
        {
            var executedContext = await next();

            if (executedContext.Exception == null && executedContext.Result != null && !executedContext.Canceled)
            {
                if (Duration < 1)
                {
                    context.SetCacheValue(executedContext.Result, TimeSpan.FromDays(9999), Name, VaryByArgument, VaryByHeader);
                }
                else
                {
                    context.SetCacheValue(executedContext.Result, TimeSpan.FromSeconds(Duration), Name, VaryByArgument, VaryByHeader);
                }
            }
        }
    }
}
