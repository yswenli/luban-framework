/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Web.Core.Auth
*文件名： AraParameterFilterAttribute
*版本号： V1.0.0.0
*唯一标识：34ecb79d-7243-4786-ba9e-8ccf2c328fc9
*当前的用户域：WALLE
*创建人： WALLEli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/9/1 13:53:50
*描述：安全参数较验过滤器
*
*=================================================
*修改标记
*修改时间：2022/9/1 13:53:50
*修改人： yswenli
*版本号： V1.0.0.0
*描述：安全参数较验过滤器
*
*****************************************************************************/

namespace LuBan.Web.Core.Attributes;

/// <summary>
/// 安全参数较验过滤器,
/// NoAraParameterFilterAttribute
/// 安全规则：
/// 参数nonce为随机数，用于防重。
/// 参数timeStamp为unix时间戳
/// 参数signature为md5签名，将除signature的所有参数按key名称排序，再按key=value&方式连接后取hash值
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class AraParameterFilterAttribute : BaseFilterAttribute
{
    /// <summary>
    /// 安全参数较验过滤器,
    /// 安全规则：
    /// 参数nonce为随机数，用于防重。
    /// 参数timeStamp为unix时间戳
    /// 参数signature为md5签名，将除signature的所有参数按key名称排序，再按key=value&方式连接后取hash值
    /// </summary>
    /// <param name="enabled"></param>
    /// <param name="order"></param>
    public AraParameterFilterAttribute(int order = 2)
    {
        Order = order;
    }

    /// <summary>
    /// 方法执行校验
    /// </summary>
    /// <param name="actionContext"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public override async Task OnActionExecutionAsync(ActionExecutingContext actionContext, ActionExecutionDelegate next)
    {
        var config = HostingOptions.Default.AppOptions;

        //禁用了安全参数检查
        if (config == null || config.DisableSafeComparisonFilter)
        {
            await next.Invoke();
            return;
        }
        //检查无需校验标签
        if (actionContext.HasAttribute<NoAraParameterFilterAttribute>())
        {
            await next.Invoke();
            return;
        }

        var context = actionContext.HttpContext;

        var request = context.Request;

        var method = request.Method;

        var headers = request.Headers;

        Result? result = null;

        switch (method)
        {
            //get的取header + query
            case "GET":
                result = ValideForGet(context, headers, config.SafeComparisonExpired);
                break;
            // post 的取header + body
            case "POST":
                result = await ValideForPost(context, headers, config.SafeComparisonExpired);
                break;

            default:

                break;
        }

        if (result == null)
        {
            await next.Invoke();
        }
        else
        {
            actionContext.Result = new JsonResult(result);
        }
    }


    /// <summary>
    /// 执行后
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        await next.Invoke();
    }

    /// <summary>
    /// 读取header中的校验参数
    /// </summary>
    /// <param name="headers"></param>
    /// <returns></returns>
    SortedDictionary<string, string> GetSortedDic(IHeaderDictionary headers)
    {
        if (headers == null || headers.Count < 1) return [];

        if (!headers.ContainsKey("timestamp"))
        {
            throw new Exception("timestamp不能为空");
        }
        if (!headers.ContainsKey("nonce"))
        {
            throw new Exception("nonce不能为空");
        }
        if (!headers.ContainsKey("signature"))
        {
            throw new Exception("signature不能为空");
        }

        var timestamp = headers["timestamp"];
        var nonce = headers["nonce"];
        var signature = headers["signature"];

        var sortedDic = new SortedDictionary<string, string>();
        sortedDic.TryAdd("timestamp", timestamp.ToString());
        sortedDic.TryAdd("nonce", nonce.ToString());
        sortedDic.TryAdd("signature", signature.ToString());

        return sortedDic;
    }

    /// <summary>
    /// 验证get方式的数据,header + query
    /// </summary>
    /// <param name="context"></param>
    /// <param name="headers"></param>
    /// <param name="safeComparisonExpired"></param>
    /// <returns></returns>
    Result? ValideForGet(HttpContext context, IHeaderDictionary headers, int safeComparisonExpired)
    {
        var sortedDic = GetSortedDic(headers);

        var query = context.Request.Query;

        if (query != null && query.Count > 0)
        {
            foreach (var item in query)
            {
                if (item.Key.IsNotNullOrEmpty())
                    sortedDic.TryAdd(item.Key.ToLower(), item.Value.ToString());
            }
        }

        if (AraReplayAttacksUtil.Valide(sortedDic, safeComparisonExpired, out Result result))
        {
            return null;
        }
        else
        {
            return result;
        }
    }

    /// <summary>
    /// 验证post方式的数据,header + body，若是json，则为json:body；否则为：md5:body的md5
    /// </summary>
    /// <param name="context"></param>
    /// <param name="headers"></param>
    /// <param name="safeComparisonExpired"></param>
    /// <returns></returns>
    async Task<Result?> ValideForPost(HttpContext context, IHeaderDictionary headers, int safeComparisonExpired)
    {
        var sortedDic = GetSortedDic(headers);

        var query = context.Request.Query;

        if (query != null && query.Count > 0)
        {
            foreach (var item in query)
            {
                if (item.Key.IsNotNullOrEmpty())
                    sortedDic.TryAdd(item.Key.ToLower(), item.Value.ToString());
            }
        }

        try
        {
            var json = await context.GetRequestBodyTextAsync(encoding: Encoding.UTF8);
            if (json.IsNotNullOrEmpty())
            {
                sortedDic.TryAdd("md5", json.GetMD5Str());
            }

            sortedDic.TryAdd("md5", json.GetMD5Str());
        }
        catch (Exception ex)
        {
            Logger.Error("不支持的数据格式", ex);
        }

        if (AraReplayAttacksUtil.Valide(sortedDic, safeComparisonExpired, out Result result))
        {
            return null;
        }
        else
        {
            return result;
        }
    }
}
