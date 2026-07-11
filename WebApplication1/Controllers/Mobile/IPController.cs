

namespace WebApplication1.Controllers.Mobile;

/// <summary>
/// ip
/// </summary>
[AllowAnonymous]
public class IPController : BaseMobileController
{
    /// <summary>
    /// index
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public Result Index()
    {
        return SuccessResult(ClientIP);
    }

    /// <summary>
    /// GetClientIP
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost]
    public Result GetClientIP(int id)
    {
        return SuccessResult(ClientIP);
    }

    /// <summary>
    /// get
    /// </summary>
    /// <returns></returns>
    [HttpGet, IPWhiteListFilter]
    public Result Get()
    {
        return SuccessResult();
    }


    [HttpGet]
    public Result Get21()
    {
        var data = new Result<List<string>>();
        data.Code = 200;
        data.Result = new List<string>() { "aaa" };
        return data;
    }


    [HttpGet]
    public Result Get22()
    {
        var data = new Result<string>();
        Result result = data;
        return result;
    }

    [HttpGet]
    public Result Get23()
    {
        var data = new Success<string>();
        return data;
    }

    [HttpGet]
    public Result Get24()
    {
        var data = new Success();
        return data;
    }
}
