/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Common
*文件名： EmailSender
*版本号： V1.0.0.0
*唯一标识：9f6d8866-23ee-416a-8019-b1246abede2a
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/10/8 15:20:57
*描述：发送邮件
*
*=================================================
*修改标记
*修改时间：2023/10/8 15:20:57
*修改人： yswenli
*版本号： V1.0.0.0
*描述：发送邮件
*
*****************************************************************************/
using System.Net.Mail;
using System.Net.Mime;

namespace LuBan.Common;

/// <summary>
/// 发送邮件
/// </summary>
public static class EmailUtil
{
    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="from"></param>
    /// <param name="fromPwd"></param>
    /// <param name="to"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="attachments"></param>
    public static void Send(MailAddress from, string fromPwd, MailAddress to, string subject, string body, params Attachment[] attachments)
    {
        using (var sender = new EmailSender(from.Address, fromPwd, from.DisplayName))
        {
            sender.Send(to, subject, body, attachments);
        }
    }
    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="from"></param>
    /// <param name="fromName"></param>
    /// <param name="fromPwd"></param>
    /// <param name="to"></param>
    /// <param name="toName"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="attachements"></param>
    public static void Send(string from, string fromName, string fromPwd, string to, string toName, string subject, string body, params string[] attachements)
    {
        using (var sender = new EmailSender(from, fromPwd, fromName))
        {
            sender.Send(to, toName, subject, body, attachements);
        }
    }
}
/// <summary>
/// 发送邮件
/// </summary>
public class EmailSender : IDisposable
{
    string _userName;
    string _fromName;
    SmtpClient _smtpClient;

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="pwd"></param>
    /// <param name="fromName"></param>
    /// <param name="host"></param>
    /// <param name="port"></param>
    public EmailSender(string userName, string pwd, string fromName = "", string host = "smtp.office365.com", int port = 25)
    {
        _userName = userName;
        _fromName = fromName;
        if (port == 25)
        {
            _smtpClient = new SmtpClient
            {
                Host = host,
                Port = port, //Recommended port is 587
                EnableSsl = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_userName, pwd),
            };
        }
        else
        {
            _smtpClient = new SmtpClient
            {
                Host = host,
                Port = port, //Recommended port is 587
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_userName, pwd),
            };
        }
    }


    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="to"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="attachments"></param>
    public void Send(MailAddress to, string subject, string body, params Attachment[] attachments)
    {
        using (var message = new MailMessage(new MailAddress(_userName, _fromName), to)
        {
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        })
        {
            if (attachments != null && attachments.Length > 0 && attachments[0] != null)
            {
                foreach (var item in attachments)
                {
                    message.Attachments.Add(item);
                }
            }
            _smtpClient.Send(message);
            if (attachments != null && attachments.Length > 0 && attachments[0] != null)
            {
                foreach (var item in attachments)
                {
                    item.Dispose();
                }
            }
        }
    }
    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="to"></param>
    /// <param name="toName"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="attachements"></param>
    public void Send(string to, string toName, string subject, string body, params string[] attachements)
    {
        var toAddress = new MailAddress(to, toName);
        var list = new List<Attachment>();
        if (attachements != null && attachements.Length > 0 && attachements[0].IsNotNullOrEmpty())
        {
            foreach (var item in attachements)
            {
                list.Add(new Attachment(item, MediaTypeNames.Application.Octet));
            }
        }
        Send(toAddress, subject, body, list.ToArray());
    }

    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        _smtpClient.Dispose();
    }
}
