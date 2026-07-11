/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.EMailKit
*文件名： EMailSmtp
*版本号： V1.0.0.0
*唯一标识：e81daacb-c7f7-414e-a1b5-c01a37e3d811
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/8/14 18:24:17
*描述：邮件客户端
*
*=================================================
*修改标记
*修改时间：2024/8/14 18:24:17
*修改人： yswenli
*版本号： V1.0.0.0
*描述：邮件客户端
*
*****************************************************************************/

namespace LuBan.EMailKit;

/// <summary>
/// 邮件客户端
/// </summary>
public interface IEMailClient : IDisposable
{
    /// <summary>
    /// 邮件选项
    /// </summary>
    EMailClientConfig EMailClientConfig { get; }

    /// <summary>
    /// 接收邮件
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<Message>> RecieveAsync(CancellationToken cancellationToken = default);


    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task<string> SendAsync(Message message);

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="to"></param>
    /// <param name="cc"></param>
    /// <param name="bcc"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="isHtml"></param>
    /// <param name="attachments"></param>
    /// <returns></returns>
    Task<string> SendAsync(List<(string, string)> to,
        List<(string, string)>? cc,
        List<(string, string)>? bcc,
        string subject, string body, bool isHtml = false, List<Attachment>? attachments = null);

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="toName"></param>
    /// <param name="to"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="isHtml"></param>
    /// <param name="attachments"></param>
    /// <returns></returns>
    Task<string> SendAsync(string toName, string to, string subject, string body, bool isHtml = false, List<Attachment>? attachments = null);

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="to"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="isHtml"></param>
    /// <param name="attachments"></param>
    /// <returns></returns>
    Task<string> SendAsync(string to, string subject, string body, bool isHtml = false, List<Attachment>? attachments = null);

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="to"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="isHtml"></param>
    /// <param name="attachmentUrls"></param>
    /// <returns></returns>
    Task<string> SendAsync(string to, string subject, string body, bool isHtml = false, Dictionary<string, string>? attachmentUrls = null);
}