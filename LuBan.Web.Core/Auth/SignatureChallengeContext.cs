/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Web.Core.Auth
*文件名： SignatureChallengeContext
*版本号： V1.0.0.0
*唯一标识：b9d9a72d-4063-4da2-a1b6-c82d985ae132
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/5 10:27:17
*描述：Signature 身份验证质询上下文
*
*=================================================
*修改标记
*修改时间：2023/12/5 10:27:17
*修改人： yswenli
*版本号： V1.0.0.0
*描述：Signature 身份验证质询上下文
*
*****************************************************************************/
namespace LuBan.Web.Core.Auth
{

    /// <summary>
    /// Signature 身份验证质询上下文
    /// </summary>
    public class SignatureChallengeContext : PropertiesContext<SignatureAuthenticationOptions>
    {
        /// <summary>
        /// Signature 身份验证质询上下文
        /// </summary>
        /// <param name="context"></param>
        /// <param name="scheme"></param>
        /// <param name="options"></param>
        /// <param name="properties"></param>
        public SignatureChallengeContext(HttpContext context,
            AuthenticationScheme scheme,
            SignatureAuthenticationOptions options,
            AuthenticationProperties properties)
            : base(context, scheme, options, properties)
        {
        }

        /// <summary>
        /// 在认证期间出现的异常
        /// </summary>
        public Exception AuthenticateFailure { get; set; }

        /// <summary>
        /// 指定是否已被处理，如果已处理，则跳过默认认证逻辑
        /// </summary>
        public bool Handled { get; private set; }
    }
}
