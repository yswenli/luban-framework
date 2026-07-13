/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Common.Consts
*文件名： CommonConst
*版本号： V1.0.0.0
*唯一标识：fac97e94-c16a-4e92-870a-807a9a51a4d8
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 15:21:03
*描述：通用常量
*
*=================================================
*修改标记
*修改时间：2023/12/4 15:21:03
*修改人： yswenli
*版本号： V1.0.0.0
*描述：通用常量
*
*****************************************************************************/
namespace LuBan.Common.Consts;

/// <summary>
/// 通用常量
/// </summary>
[Const("平台配置")]
public class CommonConst
{
    /// <summary>
    /// 系统密钥加盐
    /// </summary>
    public const string SecretSalt = "eXN3ZW5saQ==";


    /// <summary>
    /// 开启全局脱敏处理（默认不开启）
    /// </summary>
    public static bool SysSensitiveDetection = false;

    /// <summary>
    /// 系统配置
    /// </summary>
    public const string SysCode = "sys-luban-framework:";

    /// <summary>
    /// 演示环境开关
    /// </summary>
    public const string SysDemoEnv = "sys-luban-framework:demo";

    /// <summary>
    /// 默认密码
    /// </summary>
    public const string SysPasswordCode = "sys-luban-framework:password";

    /// <summary>
    /// 密码盐值
    /// </summary>
    public const string SysPasswordSaltCode = "sys-luban-framework:password-salt";

    /// <summary>
    /// 登录二次验证
    /// </summary>
    public const string SysSecondVerCode = "sys-luban-framework:second-ver";

    /// <summary>
    /// 开启图形验证码
    /// </summary>
    public const string SysCaptchaCode = "sys-luban-framework:captcha";

    /// <summary>
    /// 开启滑动验证码
    /// </summary>
    public const string SysSliderCode = "sys-luban-framework:slider";

    /// <summary>
    /// 开启水印
    /// </summary>
    public const string SysWatermarkCode = "sys-luban-framework:watermark";

    /// <summary>
    /// 开启操作日志
    /// </summary>
    public const string SysOpLog = "sys-luban-framework:oplog";

    /// <summary>
    /// Token过期时间
    /// </summary>
    public const string SysTokenExpireCode = "sys-luban-framework:token-expire";

    /// <summary>
    /// RefreshToken过期时间
    /// </summary>
    public const string SysRefreshTokenExpireCode = "sys-luban-framework:refresh-token-expire";

    /// <summary>
    /// 单用户登录
    /// </summary>
    public const string SysSingleLogin = "sys-luban-framework:single-login";

    /// <summary>
    /// 系统管理员角色编码
    /// </summary>
    public const string SysAdminRole = "sys-luban-framework:user:admin";

    /// <summary>
    /// 系统项目key
    /// </summary>

    public const string SysManagementPlatformCode = "sys-luban-framework:management-platform";



    /// <summary>
    /// Token过期时间
    /// </summary>
    public const string SysForbiddenAccessRolesCode = "sys-luban-framework:rorbidden-access:roles";
}
