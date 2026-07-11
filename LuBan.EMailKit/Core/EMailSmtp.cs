/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.EMailKit.Core
*文件名： EMailSmtp
*版本号： V1.0.0.0
*唯一标识：e81daacb-c7f7-414e-a1b5-c01a37e3d811
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/8/14 18:24:17
*描述：SMTP
*
*=================================================
*修改标记
*修改时间：2024/8/14 18:24:17
*修改人： yswenli
*版本号： V1.0.0.0
*描述：SMTP
*
*****************************************************************************/


namespace LuBan.EMailKit.Core;

/// <summary>
/// SMTP
/// </summary>
public class EMailSmtp : IDisposable, IEMailClient
{
    SmtpClient _smtpClient;

    /// <summary>
    /// 配置
    /// </summary>
    public EMailClientConfig EMailClientConfig { get; private set; }

    /// <summary>
    /// SMTP
    /// </summary>
    /// <param name="eMailClientConfig"></param>
    internal EMailSmtp(EMailClientConfig eMailClientConfig)
    {
        EMailClientConfig = eMailClientConfig;
        _smtpClient = new SmtpClient();
        _smtpClient.Connect(EMailClientConfig.Host, EMailClientConfig.Port, EMailClientConfig.UseSsl);
        _smtpClient.Authenticate(EMailClientConfig.UserName, EMailClientConfig.Password);
    }

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="fromName"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="cc"></param>
    /// <param name="bcc"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="isHtml"></param>
    /// <param name="attachments"></param>
    /// <returns></returns>
    public async Task<string> SendAsync(Message message)
    {
        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(EMailClientConfig.UserName, EMailClientConfig.UserName));
        foreach (var t in message.Header.To)
        {
            msg.To.Add(new MailboxAddress(t.Name, t.Address));
        }
        var cc = message.Header.Cc;
        if (cc != null && cc.Count > 0)
        {
            foreach (var c in cc)
            {
                msg.Cc.Add(new MailboxAddress(c.Name, c.Address));
            }
        }
        var bcc = message.Header.Bcc;
        if (bcc != null && bcc.Count > 0)
        {
            foreach (var b in bcc)
            {
                msg.Bcc.Add(new MailboxAddress(b.Name, b.Address));
            }
        }
        msg.Subject = message.Header.Subject;
        var multipart = new Multipart("multipart/alternative");
        if (message.Attachments != null && message.Attachments.Count > 0)
        {
            multipart = new Multipart("multipart/mixed");
        }
        if (message.IsHtml)
        {
            if (message.Body.IsNotNullOrEmpty())
            {
                var tp = new TextPart(TextFormat.Html)
                {
                    Text = message.Body
                };
                multipart.Add(tp);
            }
        }
        else
        {
            if (message.Body.IsNotNullOrEmpty())
            {
                var tp = new TextPart(TextFormat.Plain)
                {
                    Text = message.Body
                };
                multipart.Add(tp);
            }
        }
        if (message.Attachments != null && message.Attachments.Count > 0)
        {
            foreach (var attachment in message.Attachments)
            {
                var part = new MimePart()
                {
                    Content = new MimeContent(attachment.Content),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = attachment.Name
                };
                multipart.Add(part);
            }
        }
        msg.Body = multipart;
        return await _smtpClient.SendAsync(msg);
    }

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="fromName"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="cc"></param>
    /// <param name="bcc"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="isHtml"></param>
    /// <param name="attachments"></param>
    /// <returns></returns>
    public async Task<string> SendAsync(List<(string, string)> to,
        List<(string, string)>? cc,
        List<(string, string)>? bcc,
        string subject, string body, bool isHtml = false, List<Attachment>? attachments = null)
    {
        var message = new Message(to, cc, bcc, subject, body, isHtml, attachments);
        return await SendAsync(message);
    }

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="fromName"></param>
    /// <param name="from"></param>
    /// <param name="toName"></param>
    /// <param name="to"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="isHtml"></param>
    /// <param name="attachments"></param>
    /// <returns></returns>
    public async Task<string> SendAsync(string toName, string to, string subject, string body, bool isHtml = false, List<Attachment>? attachments = null)
    {
        List<(string, string)> tos = new List<(string, string)>();
        tos.Add((toName, to));
        return await SendAsync(tos, null, null, subject, body, isHtml, attachments);
    }

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="isHtml"></param>
    /// <param name="attachments"></param>
    /// <returns></returns>
    public async Task<string> SendAsync(string to, string subject, string body, bool isHtml = false, List<Attachment>? attachments = null)
    {
        List<(string, string)> tos = new List<(string, string)>();
        tos.Add((to, to));
        return await SendAsync(tos, null, null, subject, body, isHtml, attachments);
    }
    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="to"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="isHtml"></param>
    /// <param name="attachmentUrls"></param>
    /// <returns></returns>
    public async Task<string> SendAsync(string to, string subject, string body, bool isHtml = false, Dictionary<string, string>? attachmentUrls = null)
    {
        var attachements = new List<Attachment>();
        if (attachmentUrls != null && attachmentUrls.Count > 0)
        {
            foreach (var url in attachmentUrls)
            {
                if (url.Value.IsNullOrEmpty()) continue;
                var ms = HttpClientProxy.DownloadStream(url.Value);
                if (ms == null) continue;
                attachements.Add(new Attachment(url.Key, ms));
            }
        }
        return await SendAsync(to, subject, body, isHtml, attachements);
    }


    /// <summary>
    /// 接收邮件
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<List<Message>> RecieveAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        _smtpClient.Disconnect(true);
        _smtpClient.Dispose();
    }
}
