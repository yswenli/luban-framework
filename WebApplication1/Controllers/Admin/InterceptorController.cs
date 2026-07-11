namespace WebApplication1.Controllers.Admin;

/// <summary>
/// 测试自定义拦截功能
/// </summary>
[AllowAnonymous, NoAraParameterFilter, AllowAccess]
public class InterceptorController : BaseAdminController
{
    /// <summary>
    /// 自定义拦截业务测试1
    /// </summary>
    /// <returns></returns>
    [HttpGet, Interceptor(typeof(InterceptorService))]
    public Result Test1()
    {
        return SuccessResult();
    }

    /// <summary>
    /// 自定义拦截业务测试2
    /// </summary>
    /// <returns></returns>
    [HttpGet, Interceptor<InterceptorService>("b")]
    public Result Test2(string a, int b)
    {
        return SuccessResult($"{a}{b}");
    }

    /// <summary>
    /// 自定义拦截业务测试3
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    [HttpPost]
    public Result Test3(int a)
    {
        return SuccessResult();
    }
}


/// <summary>
/// 自定义拦截业务
/// </summary>
public class InterceptorService : IInterceptorService
{
    /// <summary>
    /// jwttoken
    /// </summary>
    public string JwtTokenString { get; set; }

    /// <summary>
    /// 拦截代码
    /// </summary>
    /// <param name="interceptorInfo"></param>
    /// <returns></returns>
    public Result? Valide(InterceptorInfo interceptorInfo)
    {
        if (interceptorInfo.Url == "https://127.0.0.1:15010/api/Interceptor/test1") return null;

        var b = interceptorInfo.Parameters.FirstOrDefault("b");

        var bVal = b != null ? (int)b : 0;

        if (bVal > 0)
        {
            return new Success(1);
        }
        else if (bVal == 0)
        {
            return null;
        }
        else
        {
            return new Fail("失败拦截示例");
        }
    }
}
