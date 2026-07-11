/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Web.Core.Auth
*文件名： SignatureAuthenticationOptions
*版本号： V1.0.0.0
*唯一标识：3df7c3c3-3e5d-4018-86d8-8747ad5dc735
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/5 10:26:37
*描述：
*
*=================================================
*修改标记
*修改时间：2023/12/5 10:26:37
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Web.Core.Auth
{
    /// <summary>
    /// Signature 身份验证选项
    /// </summary>
    public class SignatureAuthenticationOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// 请求时间允许的偏差范围
        /// </summary>
        public TimeSpan AllowedDateDrift { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Signature 身份验证事件
        /// </summary>
        public new SignatureAuthenticationEvent? Events
        {
            get
            {
                if (base.Events != null)
                    return (SignatureAuthenticationEvent)base.Events;
                return null;
            }
            set => base.Events = value;
        }
    }
}
