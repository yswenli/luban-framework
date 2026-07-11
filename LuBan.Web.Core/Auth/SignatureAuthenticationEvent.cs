/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Web.Core.Auth
*文件名： SignatureAuthenticationEvent
*版本号： V1.0.0.0
*唯一标识：1f8b6fbb-9d79-4fa4-9bc3-ce0ad2b2f76b
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/5 10:25:40
*描述：Signature 身份验证事件
*
*=================================================
*修改标记
*修改时间：2023/12/5 10:25:40
*修改人： yswenli
*版本号： V1.0.0.0
*描述：Signature 身份验证事件
*
*****************************************************************************/
namespace LuBan.Web.Core.Auth
{

    /// <summary>
    /// Signature 身份验证事件
    /// </summary>
    public class SignatureAuthenticationEvent
    {
        public SignatureAuthenticationEvent()
        {
        }

        /// <summary>
        /// 获取或设置获取 AccessKey 的 AccessSecret 的逻辑处理
        /// </summary>
        public Func<AccessSecretContext, Task<string>> OnGetAccessSecret { get; set; }

        /// <summary>
        /// 获取或设置质询的逻辑处理
        /// </summary>
        public Func<SignatureChallengeContext, Task> OnChallenge { get; set; } = _ => Task.CompletedTask;

        /// <summary>
        /// 获取或设置已验证的逻辑处理
        /// </summary>
        public Func<SignatureValidatedContext, Task> OnValidated { get; set; } = _ => Task.CompletedTask;

        /// <summary>
        /// 获取 AccessKey 的 AccessSecret
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual Task<string> GetAccessSecret(AccessSecretContext context) => OnGetAccessSecret?.Invoke(context) ?? throw new NotImplementedException($"需要提供 {nameof(OnGetAccessSecret)} 实现");

        /// <summary>
        /// 质询
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual Task Challenge(SignatureChallengeContext context) => OnChallenge?.Invoke(context) ?? Task.CompletedTask;

        /// <summary>
        /// 已验证成功
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual Task Validated(SignatureValidatedContext context) => OnValidated?.Invoke(context) ?? Task.CompletedTask;
    }
}
