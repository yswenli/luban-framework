/****************************************************************************
*Copyright @ YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.AspNetCore.Extentions
*文件名： ActionExecutingContextExtention
*版本号： V1.0.0.0
*唯一标识：faebcf49-7a27-4b6f-903f-b9ea90fd2c17
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/12/9 18:55:29
*描述：上下文扩展
*
*=================================================
*修改标记
*修改时间：2024/12/9 18:55:29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：上下文扩展
*
*****************************************************************************/
namespace LuBan.Web.Core.AspNetCore.Extentions;

/// <summary>
/// 上下文扩展
/// </summary>
public static class ActionExecutingContextExtention
{
    /// <summary>
    /// 获取当前请求url
    /// </summary>
    /// <param name="request"></param>
    /// <param name="withQuery"></param>
    /// <returns></returns>
    public static string GetRequestUrl(this HttpRequest request, bool withQuery = true)
    {
        var host = request.Host.Value;

        if (host.EndsWith(":80"))
        {
            host = host.Replace(":80", "");
        }
        if (host.EndsWith(":443"))
        {
            host = host.Replace(":443", "");
        }
        if (request.Headers.TryGetValue("X-Forwarded-Prefix", out StringValues values) && values.Count > 0)
        {
            var prefix = values.FirstOrDefault();
            if (prefix.IsNotNullOrEmpty())
            {
                host = host + "/" + prefix;
            }
        }
        if (withQuery)
            return $"https://{host}{request.Path}{(request.QueryString.HasValue ? request.QueryString.Value : "")}";
        else
            return $"https://{host}{request.Path}";
    }

    /// <summary>
    /// 获取当前请求url
    /// </summary>
    /// <param name="context"></param>
    /// <param name="withQuery"></param>
    /// <returns></returns>
    public static string GetRequestUrl(this ActionExecutingContext context, bool withQuery = true)
    {
        return context.HttpContext.Request.GetRequestUrl(withQuery);
    }


    #region 缓存处理


    /// <summary>
    /// 根据当前请求上下文生成缓存key
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cacheKey"></param>
    /// <param name="varyByArgument"></param>
    /// <param name="varyByHeader"></param>
    /// <returns></returns>
    public static string GetCacheKey(this ActionExecutingContext context,
        string? cacheKey = "",
        string? varyByArgument = "",
        string? varyByHeader = "")
    {
        void AppendByArguments(string[] varyByArguments, StringPlus stringBuilder)
        {
            if (varyByArguments == null || varyByArguments.Length == 0) return;

            stringBuilder.Append(":");
            var sp = new StringPlus();
            foreach (var argumentName in varyByArguments)
            {
                var value = "";
                if (context.ActionArguments.ContainsKey(argumentName))
                {
                    value = JsonSerializer.Serialize(context.ActionArguments[argumentName]);
                }
                sp.AppendFormat("{0}={1}&", argumentName, value);
            }

            sp.Remove(stringBuilder.Length - 1, 1);
            stringBuilder.Append(sp.ToString().GetMD5Str());
        }

        void AppendByHeaders(string[] varyByHeaders, StringPlus stringBuilder)
        {
            if (varyByHeaders == null || varyByHeaders.Length == 0) return;

            stringBuilder.Append(":");
            var sp = new StringPlus();
            foreach (var headerName in varyByHeaders)
            {
                string? value = context.HttpContext.Request.Headers[headerName ?? ""];
                if (string.IsNullOrEmpty(value)) value = "";
                sp.AppendFormat("{0}: {1}\r\n", headerName ?? "", value);
            }
            sp.Remove(stringBuilder.Length - 2, 2);
            stringBuilder.Append(sp.ToString().GetMD5Str());
        }

        var keyBuilder = new StringPlus(CacheConst.KeyApiCache);
        var groupName = "default";
        var apiExplorerSettings = context.GetAttribute<ApiExplorerSettingsAttribute>();
        if (apiExplorerSettings != null && apiExplorerSettings.GroupName.IsNotNullOrEmpty())
        {
            groupName = apiExplorerSettings.GroupName;
        }
        if (cacheKey.IsNullOrEmpty())
        {
            keyBuilder.Append(context.RouteData.Values["area"]?.ToString()?.ToLower() ?? groupName);
            keyBuilder.Append(":");
            keyBuilder.Append(context.RouteData.Values["controller"]?.ToString()?.ToLower() ?? "");
            keyBuilder.Append(":");
            keyBuilder.Append(context.RouteData.Values["action"]?.ToString()?.ToLower() ?? "");
            keyBuilder.Append(":");
            keyBuilder.Append(context.HttpContext.Request.Method.ToLower());
        }
        else
        {
            keyBuilder.Append(cacheKey);
        }

        AppendByArguments(varyByArgument?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? [], keyBuilder);

        AppendByHeaders(varyByHeader?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? [], keyBuilder);

        return keyBuilder.ToString();
    }

    /// <summary>
    /// 获取缓存值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <param name="cacheKey"></param>
    /// <param name="varyByArgument"></param>
    /// <param name="varyByHeader"></param>
    /// <returns></returns>
    public static T? GetCacheValue<T>(this ActionExecutingContext context,
        string? cacheKey = "",
        string? varyByArgument = "",
        string? varyByHeader = "")
    {
        var cache = context.HttpContext.RequestServices.GetService<IServiceCache>();
        if (cache == null)
        {
            return default(T);
        }
        var ck = context.GetCacheKey(cacheKey, varyByArgument, varyByHeader);
        return cache.Get<T>(ck);
    }

    /// <summary>
    /// 设置缓存值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <param name="value"></param>
    /// <param name="timeout"></param>
    /// <param name="cacheKey"></param>
    /// <param name="varyByArgument"></param>
    /// <param name="varyByHeader"></param>
    public static void SetCacheValue<T>(this ActionExecutingContext context,
        T value,
        TimeSpan timeout,
        string? cacheKey = "",
        string? varyByArgument = "",
        string? varyByHeader = "")
    {
        var cache = context.HttpContext.RequestServices.GetService<IServiceCache>();
        if (cache == null)
        {
            return;
        }
        var ck = context.GetCacheKey(cacheKey, varyByArgument, varyByHeader);
        cache.Set(ck, value, timeout);
    }

    /// <summary>
    /// 删除缓存值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <param name="cacheKey"></param>
    /// <param name="varyByArgument"></param>
    /// <param name="varyByHeader"></param>
    public static void DeleteCacheValue(this ActionExecutingContext context,
        string? cacheKey = "",
        string? varyByArgument = "",
        string? varyByHeader = "")
    {
        var cache = context.HttpContext.RequestServices.GetService<IServiceCache>();
        if (cache == null)
        {
            return;
        }
        var ck = context.GetCacheKey(cacheKey, varyByArgument, varyByHeader);
        cache.Delete(ck);
    }


    #endregion


    /// <summary>
    /// 检查是否标记无效
    /// </summary>
    /// <param name="context"></param>
    /// <param name="filterName"></param>
    /// <returns></returns>
    public static bool HasAttribute(this ActionExecutingContext context, string filterName)
    {
        return context.ActionDescriptor.EndpointMetadata.Any(q => q.ToString()?.EndsWith(filterName) ?? false);
    }

    /// <summary>
    /// 检查是否标记无效
    /// </summary>
    /// <typeparam name="Attr"></typeparam>
    /// <param name="context"></param>
    /// <returns></returns>
    public static bool HasAttribute<Attr>(this ActionExecutingContext context) where Attr : Attribute
    {
        return context.ActionDescriptor.EndpointMetadata.Any(q => q is Attr);
    }

    /// <summary>
    /// 获取属性
    /// </summary>
    /// <typeparam name="Attr"></typeparam>
    /// <param name="context"></param>
    /// <returns></returns>
    public static Attr? GetAttribute<Attr>(this ActionExecutingContext context) where Attr : Attribute
    {
        return context.ActionDescriptor.EndpointMetadata.FirstOrDefault(q => q is Attr) as Attr;
    }

}
