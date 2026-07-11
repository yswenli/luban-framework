/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Caches
*文件名： CacheClearAttribute
*版本号： V1.0.0.0
*唯一标识：2c1ddad6-2b5a-4d9f-a5bd-fffe72b2ee2a
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/12/5 14:32:28
*描述：清除aspnetcore api 缓存
*
*=================================================
*修改标记
*修改时间：2025/12/5 14:32:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：清除aspnetcore api 缓存
*
*****************************************************************************/
namespace LuBan.Web.Core.Caches;

/// <summary>
/// 清除aspnetcore api 缓存,清除CacheableAttribute标记的缓存
/// </summary>

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CacheClearAttribute : Attribute, IAsyncActionFilter
{
    /// <summary>
    /// 缓存名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 清除aspnetcore api 缓存,清除CacheableAttribute标记的缓存
    /// </summary>
    /// <param name="name"></param>
    public CacheClearAttribute([NotNull] string name)
    {
        Name = name;
    }

    /// <summary>
    /// 接口方法执行前后
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await next();
        context.DeleteCacheValue(Name);
    }
}
