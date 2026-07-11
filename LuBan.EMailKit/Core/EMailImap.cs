/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.EMailKit.Core
*文件名： EmailImap
*版本号： V1.0.0.0
*唯一标识：56589c9f-184a-400e-8c45-028a58f4da2b
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/8/15 11:48:13
*描述：EmailImap
*
*=================================================
*修改标记
*修改时间：2024/8/15 11:48:13
*修改人： yswenli
*版本号： V1.0.0.0
*描述：EmailImap
*
*****************************************************************************/


namespace LuBan.EMailKit.Core;

/// <summary>
/// EmailImap
/// </summary>
public class EmailImap : IDisposable, IEMailClient
{
    ImapClient _imapClient;

    /// <summary>
    /// 配置
    /// </summary>
    public EMailClientConfig EMailClientConfig { get; private set; }

    /// <summary>
    /// EmailImap
    /// </summary>
    /// <param name="eMailClientConfig"></param>
    internal EmailImap(EMailClientConfig eMailClientConfig)
    {
        EMailClientConfig = eMailClientConfig;
        _imapClient = new ImapClient();
        _imapClient.Connect(EMailClientConfig.Host, EMailClientConfig.Port, EMailClientConfig.UseSsl);
        _imapClient.Authenticate(EMailClientConfig.UserName, EMailClientConfig.Password);
    }

    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        _imapClient.Disconnect(true);
        _imapClient.Dispose();
    }

    /// <summary>
    /// 收信
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<List<Message>> RecieveAsync(CancellationToken cancellationToken = default)
    {
        var result = new List<Message>();
        var inBox = _imapClient.Inbox;
        inBox.Open(FolderAccess.ReadOnly);
        var count = inBox.Count;
        if (count < 1) return result;
        for (int i = 0; i < count; i++)
        {
            var message = await inBox.GetMessageAsync(i, cancellationToken);
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

    public Task<string> SendAsync(Message message)
    {
        throw new NotImplementedException();
    }


    public Task<string> SendAsync(string from, string to, string subject, string body, bool isHtml = false, List<Attachment>? attachments = null)
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
