/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.EMailKit
*文件名： EnumClientType
*版本号： V1.0.0.0
*唯一标识：cc8ffccb-851b-4f16-9369-43e3ad8b74a1
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/8/14 16:28:16
*描述：邮件客户端类型
*
*=================================================
*修改标记
*修改时间：2024/8/14 16:28:16
*修改人： yswenli
*版本号： V1.0.0.0
*描述：邮件客户端类型
*
*****************************************************************************/

namespace LuBan.EMailKit;

/// <summary>
/// 邮件客户端类型
/// </summary>
public enum EnumClientType
{
    /// <summary>
    /// 无
    /// </summary>
    [Description("无")]
    None = 0,
    /// <summary>
    /// SMTP服务器
    /// </summary>
    [Description("SMTP服务器")]
    SMTP = 1,
    /// <summary>
    /// POP3服务器
    /// </summary>
    [Description("POP3服务器")]
    POP3 = 2,
    /// <summary>
    /// IMAP服务器
    /// </summary>
    [Description("IMAP服务器")]
    IMAP = 3
}
