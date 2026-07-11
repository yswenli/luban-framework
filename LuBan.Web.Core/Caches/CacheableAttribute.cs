/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Caches
*文件名： CacheableAttribute
*版本号： V1.0.0.0
*唯一标识：97a6bcad-e8ed-4a4c-8697-33e44b3603bd
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/12/5 11:28:51
*描述：标记aspnetcore api方法结果可以被缓存
*
*=================================================
*修改标记
*修改时间：2025/12/5 11:28:51
*修改人： yswenli
*版本号： V1.0.0.0
*描述：标记aspnetcore api方法结果可以被缓存
*
*****************************************************************************/

namespace LuBan.Web.Core.Caches;

/// <summary>
/// 标记aspnetcore api方法结果可以被缓存，下次调用时直接从缓存获取，不再执行方法体。
/// 可用CacheClearAttribute清除缓存
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CacheableAttribute : Attribute, IAsyncActionFilter, IOrderedFilter
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
    /// 缓存时间（秒），-1表示永久
    /// </summary>
    public int Duration
    {
        get; set;
    } = -1;

    /// <summary>
    /// 标记aspnetcore api方法结果可以被缓存，下次调用时直接从缓存获取，不再执行方法体。
    /// 可用CacheClearAttribute清除缓存
    /// </summary>
    /// <param name="name"></param>
    /// <param name="duration"></param>
    public CacheableAttribute([NotNull] string name, int duration = -1)
    {
        Name = name;
        Duration = duration;
    }

    /// <summary>
    /// 接口方法执行前后
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var result = context.GetCacheValue<ObjectResult>(Name);

        if (result != null)
        {
            context.Result = result;
        }
        else
        {
            var executedContext = await next();

            if (executedContext.Exception == null && !executedContext.Canceled)
            {
                var data = executedContext.Result;
                if (data != null)
                {
                    if (Duration < 1)
                    {
                        context.SetCacheValue(data, TimeSpan.FromDays(9999), Name);
                    }
                    else
                    {
                        context.SetCacheValue(data, TimeSpan.FromSeconds(Duration), Name);
                    }
                }
            }
        }
    }
}
