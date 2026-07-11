/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.EMailKit.Models
*文件名： EMailInfo
*版本号： V1.0.0.0
*唯一标识：bd1027ec-1130-4408-8a32-40c9d4d9bea3
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/8/15 10:21:17
*描述：邮件信息
*
*=================================================
*修改标记
*修改时间：2024/8/15 10:21:17
*修改人： yswenli
*版本号： V1.0.0.0
*描述：邮件信息
*
*****************************************************************************/


namespace LuBan.EMailKit.Models;

/// <summary>
/// 邮件信息
/// </summary>
public class Message
{
    /// <summary>
    /// 邮件头
    /// </summary>
    public Header Header { get; set; }
    /// <summary>
    /// 邮件体
    /// </summary>
    [Required(ErrorMessage = "请输入邮件内容")]
    public string Body { get; set; }

    /// <summary>
    /// 邮件体类型
    /// </summary>
    public bool IsHtml { get; set; }

    /// <summary>
    /// 邮件附件列表
    /// </summary>
    public List<Attachment>? Attachments { get; set; }

    /// <summary>
    /// 邮件信息
    /// </summary>
    public Message()
    {
        Header = new Header()
        {
            To = new List<UserAddress>(),
            Bcc = new List<UserAddress>(),
            Cc = new List<UserAddress>()
        };
    }

    /// <summary>
    /// 邮件信息
    /// </summary>
    /// <param name="to"></param>
    /// <param name="cc"></param>
    /// <param name="bcc"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="isHtml"></param>
    /// <param name="attachments"></param>
    public Message(List<(string, string)> to,
            List<(string, string)>? cc,
            List<(string, string)>? bcc,
            string subject, string body, bool isHtml = false, List<Attachment>? attachments = null)
    {
        Header = new Header()
        {
            To = new List<UserAddress>(),
            Bcc = new List<UserAddress>(),
            Cc = new List<UserAddress>(),
            Subject = subject
        };
        Header.To.AddRange(to.Select(x => new UserAddress() { Name = x.Item1, Address = x.Item2 }));
        if (cc != null && cc.Count > 0)
        {
            Header.Cc.AddRange(cc.Select(x => new UserAddress() { Name = x.Item1, Address = x.Item2 }));
        }
        if (bcc != null && bcc.Count > 0)
        {
            Header.Bcc.AddRange(bcc.Select(x => new UserAddress() { Name = x.Item1, Address = x.Item2 }));
        }
        Body = body;
        IsHtml = isHtml;
        Attachments = attachments;
    }

    /// <summary>
    /// 邮件信息
    /// </summary>
    /// <param name="to"></param>
    /// <param name="cc"></param>
    /// <param name="bcc"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="isHtml"></param>
    /// <param name="attachments"></param>
    public Message(List<UserAddress> to, List<UserAddress>? cc, List<UserAddress>? bcc, string subject, string body, bool isHtml = false, List<Attachment>? attachments = null)
    {
        Header = new Header()
        {
            To = new List<UserAddress>(),
            Bcc = new List<UserAddress>(),
            Cc = new List<UserAddress>(),
            Subject = subject
        };
        Header.To = to;
        if (cc != null && cc.Count > 0)
        {
            Header.Cc = cc;
        }
        if (bcc != null && bcc.Count > 0)
        {
            Header.Bcc = bcc;
        }
    }

    /// <summary>
    /// 邮件信息
    /// </summary>
    /// <param name="to"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="isHtml"></param>
    /// <param name="attachments"></param>
    public Message(string to,
        string subject,
        string body,
        bool isHtml = false,
        List<Attachment>? attachments = null) : this(new List<(string, string)>() { (to, to) },
            null,
            null,
            subject,
            body,
            isHtml,
            attachments)
    {

    }


    /// <summary>
    /// 邮件信息
    /// </summary>
    /// <param name="input"></param>
    /// <param name="attachments"></param>
    /// <returns></returns>
    public static Message FromInput(MsgInput input, List<Attachment>? attachments)
    {
        var msg = new Message(new List<(string, string)>() { (input.ToName, input.To) }, null, null, input.Subject, input.Body, input.IsHtml, attachments);
        return msg;
    }

    /// <summary>
    /// 邮件信息
    /// </summary>
    /// <param name="input"></param>
    /// <param name="attachmentUrls"></param>
    /// <returns></returns>
    public static Message FromInput(MsgInput input, Dictionary<string, string>? attachmentUrls)
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
        return FromInput(input, attachements);
    }
}
