/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.EMailKit.Core
*文件名： EMailImap
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
    /// <param name="maxCount">最大接收邮件数量，默认100，防止大邮箱内存溢出</param>
    /// <returns></returns>
    public async Task<List<Message>> ReceiveAsync(CancellationToken cancellationToken = default, int maxCount = 100)
    {
        var result = new List<Message>();
        var inBox = _imapClient.Inbox;
        inBox.Open(FolderAccess.ReadOnly);
        var count = inBox.Count;
        if (count < 1) return result;
        var limit = Math.Min(count, maxCount);
        for (int i = 0; i < limit; i++)
        {
            var message = await inBox.GetMessageAsync(i, cancellationToken);
            if (message != null)
            {
                var from = (MailboxAddress)message.From.First();
                var to = new List<UserAddress>();
                foreach (MailboxAddress item in message.To)
                {
                    to.Add(new UserAddress(item.Name ?? string.Empty, item.Address));
                }
                var cc = new List<UserAddress>();
                if (message.Cc != null && message.Cc.Count > 0)
                {
                    foreach (MailboxAddress item in message.Cc)
                    {
                        cc.Add(new UserAddress(item.Name ?? string.Empty, item.Address));
                    }
                }
                var bcc = new List<UserAddress>();
                if (message.Bcc != null && message.Bcc.Count > 0)
                {
                    foreach (MailboxAddress item in message.Bcc)
                    {
                        bcc.Add(new UserAddress(item.Name ?? string.Empty, item.Address));
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
                                var attachment = new Attachment(mp.FileName ?? string.Empty, mc.Stream);
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
                var msg = new Message(to, cc, bcc, message.Subject ?? string.Empty, body, isHtml, attachments);
                msg.Header.From = new UserAddress(from.Name ?? string.Empty, from.Address);
                result.Add(msg);
            }
        }
        return result;
    }

    /// <summary>
    /// 发送由 <c>Message</c> 对象描述的邮件；IMAP 客户端当前未实现发送能力。
    /// </summary>
    /// <param name="message">包含收件人、主题、正文与附件的邮件对象。</param>
    /// <returns>表示发送结果文本的任务，当前实现直接抛出未实现异常。</returns>
    public Task<string> SendAsync(Message message)
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// 以显式发件人与单个收件人地址字符串的形态发送邮件；IMAP 客户端当前未实现发送能力。
    /// </summary>
    /// <param name="from">发件人邮箱地址。</param>
    /// <param name="to">收件人邮箱地址字符串。</param>
    /// <param name="subject">邮件主题。</param>
    /// <param name="body">邮件正文内容。</param>
    /// <param name="isHtml">正文是否为 HTML 格式。</param>
    /// <param name="attachments">附件列表，为空表示无附件。</param>
    /// <returns>表示发送结果文本的任务，当前实现直接抛出未实现异常。</returns>
    public Task<string> SendAsync(string from, string to, string subject, string body, bool isHtml = false, List<Attachment>? attachments = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 以“姓名与地址”元组列表分别指定收件人、抄送与密送的形态发送邮件；IMAP 客户端当前未实现发送能力。
    /// </summary>
    /// <param name="to">收件人列表，每项为姓名与邮箱地址的元组。</param>
    /// <param name="cc">抄送列表，为空表示不抄送。</param>
    /// <param name="bcc">密送列表，为空表示不密送。</param>
    /// <param name="subject">邮件主题。</param>
    /// <param name="body">邮件正文内容。</param>
    /// <param name="isHtml">正文是否为 HTML 格式。</param>
    /// <param name="attachments">附件列表，为空表示无附件。</param>
    /// <returns>表示发送结果文本的任务，当前实现直接抛出未实现异常。</returns>
    public Task<string> SendAsync(List<(string, string)> to, List<(string, string)>? cc, List<(string, string)>? bcc, string subject, string body, bool isHtml = false, List<Attachment>? attachments = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 向单个收件人地址发送邮件并附带本地附件列表；IMAP 客户端当前未实现发送能力。
    /// </summary>
    /// <param name="to">收件人邮箱地址字符串。</param>
    /// <param name="subject">邮件主题。</param>
    /// <param name="body">邮件正文内容。</param>
    /// <param name="isHtml">正文是否为 HTML 格式。</param>
    /// <param name="attachments">附件列表，为空表示无附件。</param>
    /// <returns>表示发送结果文本的任务，当前实现直接抛出未实现异常。</returns>
    public Task<string> SendAsync(string to, string subject, string body, bool isHtml = false, List<Attachment>? attachments = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 向单个收件人地址发送邮件，附件以“文件名到下载地址”的字典给出；IMAP 客户端当前未实现发送能力。
    /// </summary>
    /// <param name="to">收件人邮箱地址字符串。</param>
    /// <param name="subject">邮件主题。</param>
    /// <param name="body">邮件正文内容。</param>
    /// <param name="isHtml">正文是否为 HTML 格式。</param>
    /// <param name="attachmentUrls">附件字典，键为附件文件名、值为可下载的 URL。</param>
    /// <returns>表示发送结果文本的任务，当前实现直接抛出未实现异常。</returns>
    public Task<string> SendAsync(string to, string subject, string body, bool isHtml = false, Dictionary<string, string>? attachmentUrls = null)
    {
        throw new NotImplementedException();
    }
}
