/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Web.Core.Auth
*文件名： SignatureValidatedContext
*版本号： V1.0.0.0
*唯一标识：fd2acb8a-c6c3-41bc-93d0-9d45b87d678f
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/5 10:27:49
*描述：Signature 身份验证已验证上下文
*
*=================================================
*修改标记
*修改时间：2023/12/5 10:27:49
*修改人： yswenli
*版本号： V1.0.0.0
*描述：Signature 身份验证已验证上下文
*
*****************************************************************************/
namespace LuBan.Web.Core.Auth
{

    /// <summary>
    /// Signature 身份验证已验证上下文
    /// </summary>
    public class SignatureValidatedContext : ResultContext<SignatureAuthenticationOptions>
    {
        /// <summary>
        /// Signature 身份验证已验证上下文
        /// </summary>
        /// <param name="context"></param>
        /// <param name="scheme"></param>
        /// <param name="options"></param>
        public SignatureValidatedContext(HttpContext context,
            AuthenticationScheme scheme,
            SignatureAuthenticationOptions options)
            : base(context, scheme, options)
        {
        }

        /// <summary>
        /// 身份标识
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        /// 密钥
        /// </summary>
        public string AccessSecret { get; set; }
    }
}
