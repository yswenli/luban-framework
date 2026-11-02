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
*公司名称：yswenli
*命名空间：LuBan.Web.Core.Attributes
*文件名： InterceptorAttribute
*版本号： V1.0.0.0
*唯一标识：72161c1a-6478-4b26-8c70-f8de3062f71b
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/6/30 15:16:07
*描述：业务拦截过滤器
*
*=================================================
*修改标记
*修改时间：2023/6/30 15:16:07
*修改人： yswenli
*版本号： V1.0.0.0
*描述：业务拦截过滤器
*
*****************************************************************************/
namespace LuBan.Web.Core.Attributes;

/// <summary>
/// 业务拦截过滤器，可使用NoInterceptorAttribute标记无需校验
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class InterceptorAttribute : BaseFilterAttribute
{
    Type _serviceType;
    string[] _parameters;

    /// <summary>
    /// 用拦截服务类型与其需要读取的参数名构造业务拦截过滤器，并校验该类型可实现拦截服务接口。
    /// </summary>
    /// <param name="type">实现拦截服务接口的类型。</param>
    /// <param name="parameters">拦截时需要从请求中提取的参数名列表。</param>
    public InterceptorAttribute(Type type, params string[] parameters)
    {
        _serviceType = type;
        _parameters = parameters;
        var intService = type.Create() as IInterceptorService;
        if (intService == null) throw new Exception("InterceptorService is null");
    }

    /// <summary>
    /// 拦截处理
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.HasAttribute<NoInterceptorAttribute>())
        {
            await next.Invoke();
            return;
        }

        var interceptorService = _serviceType.Create() as IInterceptorService;
        if (interceptorService == null) throw new Exception("InterceptorService is null");

        var request = context.HttpContext.Request;
        var url = context.GetRequestUrl(false);

        var headers = new Dictionary<string, string>();
        if (request.Headers != null && request.Headers.Count > 0)
        {
            foreach (var item in request.Headers)
            {
                headers.TryAdd(item.Key, item.Value.ToString());
            }

            if (request.Headers.ContainsKey("Authorization"))
            {
                interceptorService.JwtTokenString = request.Headers["Authorization"].ToString();
            }
        }

        var parameters = new Dictionary<string, object>();
        if (_parameters != null && _parameters.Length > 0 && _parameters[0].IsNotNullOrEmpty())
        {
            var ps = context.ActionArguments;
            if (ps != null && ps.Count > 0)
            {
                foreach (var item in _parameters)
                {
                    if (ps.TryGetValue(item, out object? val) && val != null)
                    {
                        parameters.TryAdd(item, val);
                    }
                }
            }
        }

        var controllerName = "";
        var actionName = "";
        if (context.RouteData != null && context.RouteData.Values != null && context.RouteData.Values.Count > 0)
        {
            if (context.RouteData.Values.ContainsKey("controller"))
            {
                controllerName = context.RouteData.Values["controller"]?.ToString() ?? "";
            }
            if (context.RouteData.Values.ContainsKey("action"))
            {
                actionName = context.RouteData.Values["action"]?.ToString() ?? "";
            }
        }

        var result = interceptorService.Valide(new InterceptorInfo()
        {
            ControllerName = controllerName,
            ActionName = actionName,
            Url = url,
            Query = request.QueryString.ToString(),
            Method = request.Method,
            Headers = headers,
            Parameters = parameters
        });

        if (result == null)
        {
            await next.Invoke();
        }
        else
        {
            context.Result = new JsonResult(result);
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
}


/// <summary>
/// 拦截业务实现
/// </summary>
public interface IInterceptorService
{
    /// <summary>
    /// jwt token string
    /// </summary>
    public string JwtTokenString { set; get; }


    /// <summary>
    /// 业务处理,空结果表示需要执行业务,否则直接返回Result
    /// </summary>
    /// <param name="interceptorInfo"></param>
    /// <returns></returns>
    public Result? Valide(InterceptorInfo interceptorInfo);
}

/// <summary>
/// 标记无需业务拦截过滤器
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class NoInterceptorAttribute : Attribute
{

}

/// <summary>
/// 业务拦截过滤器
/// </summary>
/// <typeparam name="T"></typeparam>
public class InterceptorAttribute<T> : InterceptorAttribute where T : IInterceptorService
{
    /// <summary>
    /// 业务拦截过滤器
    /// </summary>
    /// <param name="serviceFilterType"></param>
    /// <param name="parameters"></param>
    public InterceptorAttribute(params string[] parameters) : base(typeof(T), parameters)
    {

    }
}