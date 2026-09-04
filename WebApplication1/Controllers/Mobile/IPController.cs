/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：WebApplication1.Controllers.Mobile
*文件名： IPController.cs
*版本号： V1.0.0.0
*唯一标识：f2a5ae13-994f-4547-99b8-9f697691d1cd
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:30
*描述：IPController 控制器
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：IPController 控制器
*
*****************************************************************************/



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
