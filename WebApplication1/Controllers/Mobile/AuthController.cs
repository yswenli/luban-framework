/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：WebApplication1.Controllers.Mobile
*文件名： AuthController.cs
*版本号： V1.0.0.0
*唯一标识：637639f9-695e-4f33-aa7d-2f0867d3b963
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:30
*描述：AuthController 控制器
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：AuthController 控制器
*
*****************************************************************************/

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
    public async Task<Result> Login([Required(ErrorMessage = "请输入用户名或密码"), FromBody] UserLoginInput input)
    {
        var user = await new DbRepository<DbUser>()
            .Includes(q => q.UserRoles, w => w.SysRole)
            .FirstAsync(q => q.Id == LuBanOrmConst.SuperAdminId);
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
