/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Web.Core.Auth
*文件名： GetAccessSecretContext
*版本号： V1.0.0.0
*唯一标识：89e3881f-d2ef-4652-bfdb-a0acde46f3c5
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/5 10:26:11
*描述：
*
*=================================================
*修改标记
*修改时间：2023/12/5 10:26:11
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Web.Core.Auth;

/// <summary>
/// 获取 AccessKey 关联 AccessSecret 方法的上下文
/// </summary>
public class AccessSecretContext : BaseContext<SignatureAuthenticationOptions>
{
    /// <summary>
    /// 获取 AccessKey 关联 AccessSecret 方法的上下文
    /// </summary>
    /// <param name="context"></param>
    /// <param name="scheme"></param>
    /// <param name="options"></param>
    public AccessSecretContext(HttpContext context,
        AuthenticationScheme scheme,
        SignatureAuthenticationOptions options)
        : base(context, scheme, options)
    {
    }

    /// <summary>
    /// 身份标识
    /// </summary>
    public string AccessKey { get; set; }
}
