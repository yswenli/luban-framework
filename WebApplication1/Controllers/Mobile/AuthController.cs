using Lazy.Captcha.Core;

using Yitter.IdGenerator;

namespace WebApplication1.Controllers.Mobile;

/// <summary>
/// Auth
/// </summary>
[ForbiddenAccess]
public class AuthController : BaseMobileController
{
    /// <summary>
    /// Auth
    /// </summary>
    public AuthController()
    {

    }

    /// <summary>
    /// Login
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [AllowAnonymous, HttpPost, DisplayName("Login"), AllowAccess]
    public Result Login([Required(ErrorMessage = "请输入用户名或密码"), FromBody] UserLoginInput input)
    {
        var user = new DbRepository<DbUser>()
            .Includes(q => q.UserRoles, w => w.SysRole)
            .FirstAsync(q => q.Id == LuBanOrmConst.SuperAdminId).Result;
        return SuccessResult(CreateJwtToken(user, ""));
    }

    /// <summary>
    /// Test
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public Result Test()
    {
        return SuccessResult(SessionUser.UserId);
    }

    /// <summary>
    /// 获取验证码
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [DisplayName("获取验证码"), HttpGet]
    public Result GetCaptcha()
    {
        ICaptcha captcha = ServiceProviderUtil.GetRequiredService<ICaptcha>();
        var codeId = YitIdHelper.NextId().ToString();
        var captchas = captcha.Generate(codeId, 180);
        return SuccessResult(new { Id = codeId, Img = captchas.Base64 });
    }
}
