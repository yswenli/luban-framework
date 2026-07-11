/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.EMailKit.Core
*文件名： EMailPop3
*版本号： V1.0.0.0
*唯一标识：cb4e757c-d97b-4519-ba3e-1a9f4d725a26
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/8/15 10:14:44
*描述：POP3
*
*=================================================
*修改标记
*修改时间：2024/8/15 10:14:44
*修改人： yswenli
*版本号： V1.0.0.0
*描述：POP3
*
*****************************************************************************/


namespace LuBan.EMailKit.Core;

/// <summary>
/// POP3
/// </summary>
public class EMailPop3 : IDisposable, IEMailClient
{
    Pop3Client _pop3Client;

    /// <summary>
    /// POP3
    /// </summary>
    internal EMailPop3(EMailClientConfig eMailClientConfig)
    {
        EMailClientConfig = eMailClientConfig;
        _pop3Client = new Pop3Client();
        _pop3Client.Connect(EMailClientConfig.Host, EMailClientConfig.Port, EMailClientConfig.UseSsl);
        _pop3Client.Authenticate(EMailClientConfig.UserName, EMailClientConfig.Password);
    }

    /// <summary>
    /// 配置
    /// </summary>
    public EMailClientConfig EMailClientConfig { get; private set; }

    /// <summary>
    /// Dispose
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public void Dispose()
    {
        _pop3Client.Disconnect(true);
        _pop3Client.Dispose();
    }

    /// <summary>
    /// 接收邮件
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<List<Message>> RecieveAsync(CancellationToken cancellationToken = default)
    {
        var result = new List<Message>();
        var count = _pop3Client.Count;
        if (count < 1) return result;
        for (int i = 0; i < count; i++)
        {
            var message = await _pop3Client.GetMessageAsync(i, cancellationToken);
            if (message != null)
            {
                var from = (MailboxAddress)message.From.First();
                var to = new List<UserAddress>();
                foreach (MailboxAddress item in message.To)
                {
                    to.Add(new UserAddress(item.Name, item.Address));
                }
                var cc = new List<UserAddress>();
                if (message.Cc != null && message.Cc.Count > 0)
                {
                    foreach (MailboxAddress item in message.Cc)
                    {
                        cc.Add(new UserAddress(item.Name, item.Address));
                    }
                }
                var bcc = new List<UserAddress>();
                if (message.Bcc != null && message.Bcc.Count > 0)
                {
                    foreach (MailboxAddress item in message.Bcc)
                    {
                        bcc.Add(new UserAddress(item.Name, item.Address));
                    }
                }

                var isHtml = false;
                var body = "";
                if (message.HtmlBody.IsNotNullOrEmpty())
                {
                    isHtml = true;
                    body = message.HtmlBody;
                }
                if (message.TextBody.IsNotNullOrEmpty())
                {
                    body = message.TextBody;
                }
                var attachments = new List<Attachment>();
                if (body.IsNullOrEmpty() && message.BodyParts != null)
                {
                    foreach (var item in message.BodyParts)
                    {
                        if (item.IsAttachment && item is MimePart mp)
                        {
                            if (mp.Content is MimeContent mc && mc.Stream != null)
                            {
                                var attachment = new Attachment(mp.FileName, mc.Stream);
                                attachments.Add(attachment);
                            }
                        }
                        else if (item is TextPart tp)
                        {
                            if (tp.IsHtml)
                            {
                                isHtml = true;
                                body += tp.Text;
                            }
                            else
                            {
                                body += tp.Text;
                            }
                        }

                    }
                }
                var msg = new Message(to, cc, bcc, message.Subject, body, isHtml, attachments);
                msg.Header.From = new UserAddress(from.Name, from.Address);
                result.Add(msg);
            }
        }
        return result;
    }

    public Task<string> SendAsync(string from, string to, string subject, string body, bool isHtml = false, List<Attachment>? attachments = null)
    {
        throw new NotImplementedException();
    }

    public Task<string> SendAsync(Message message)
    {
        throw new NotImplementedException();
    }

    public Task<string> SendAsync(List<(string, string)> to, List<(string, string)>? cc, List<(string, string)>? bcc, string subject, string body, bool isHtml = false, List<Attachment>? attachments = null)
    {
        throw new NotImplementedException();
    }

    public Task<string> SendAsync(string to, string subject, string body, bool isHtml = false, List<Attachment>? attachments = null)
    {
        throw new NotImplementedException();
    }

    public Task<string> SendAsync(string to, string subject, string body, bool isHtml = false, Dictionary<string, string>? attachmentUrls = null)
    {
        throw new NotImplementedException();
    }
}
