/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Web.Core
*文件名： BaseWebController
*版本号： V1.0.0.0
*唯一标识：647416bb-c6b2-4821-b0bf-c9dd09999052
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/9/6 11:15:10
*描述：管理后端web控制器基类
*
*=================================================
*修改标记
*修改时间：2022/9/6 11:15:10
*修改人： yswenli
*版本号： V1.0.0.0
*描述：管理后端web控制器基类
*
*****************************************************************************/

namespace LuBan.Web.Core;

/// <summary>
/// 管理后端web控制器基类，
/// LoginAuth(false)取消登录较验
/// </summary>
[WebLoginAuth]
public abstract class BaseWebController : BaseController
{
    protected readonly string _userIdKey;
    protected readonly string _jwtKey;

    /// <summary>
    /// 管理后端web控制器基类
    /// </summary>
    public BaseWebController() : base()
    {
        _userIdKey = "UserID".GetMD5Str();
        _jwtKey = "JWT".GetMD5Str();
    }

    /// <summary>
    /// aspnetcore环境参数
    /// </summary>
    protected new IWebHostEnvironment _env;

    /// <summary>
    /// 框架中基础控制器类
    /// </summary>
    /// <param name="env"></param>
    public BaseWebController(IWebHostEnvironment env) : this()
    {
        _env = env;
    }

    /// <summary>
    /// jwt token string
    /// </summary>
    public string JwtTokenString => $"Bearer {HttpContext.Request.Cookies[_jwtKey]}";



    /// <summary>
    /// 用户id
    /// </summary>
    public long? UserID
    {
        get
        {
            var cuid = HttpContext.Request.Cookies[_userIdKey];
            if (cuid.IsNullOrEmpty()) return null;
            if (long.TryParse(AESUtil.Decrypt(cuid.UrlDecode(), KeyIvExtensions.DEFAULTKEY), out long uid))
            {
                return uid;
            }
            else
            {
                return null;
            }
        }
        set
        {
            if (value == null) { DelSession(); return; }
            var userId = value;
            var option = new CookieOptions();
            option.Expires = DateTime.Now.AddSeconds(HostingOptions.Default.AppOptions.JwtAuthConfig.AccessExpiration);
            HttpContext.Response.Cookies.Append(_userIdKey, AESUtil.Encrypt(userId.ToString(), KeyIvExtensions.DEFAULTKEY).UrlEncode(), option);
        }
    }


    /// <summary>
    /// 删除登录状态
    /// </summary>
    protected void DelSession()
    {
        HttpContext.Response.Cookies.Delete(_userIdKey);
        HttpContext.Response.Cookies.Delete(_jwtKey);
    }

    /// <summary>
    /// 返回结果
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public IActionResult Result(Result result)
    {
        return Json(result);
    }

    /// <summary>
    /// 返回成功结果
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public IActionResult SuccessResult(dynamic? data = null)
    {
        var result = new Success();
        if (data != null)
        {
            result.Result = data;
        }
        return Result(result);
    }
    /// <summary>
    /// 返回失败结果
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    public IActionResult ErrorResult(string? msg = null)
    {
        var result = new Fail();
        if (msg != null)
        {
            result.Message = msg ?? "操作失败";
        }
        return Result(result);
    }
}
