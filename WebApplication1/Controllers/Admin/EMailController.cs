using LuBan.EMailKit;
using LuBan.EMailKit.Models;

namespace WebApplication1.Controllers.Admin;

/// <summary>
/// 邮件接口
/// </summary>
[AllowAnonymous, AllowAccess]
public class EMailController : BaseAdminController
{
    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public async Task<string> SendEMailAsync([FromBody] Message message)
    {
        try
        {
            var eMailClient = EMailFactory.Create();
            return await eMailClient.SendAsync(message);
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    /// <summary>
    /// 发送带附件的邮件
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [DisableRequestSizeLimit]
    public async Task<string> SendEMailWithAttachmentsAsync([FromForm] EMailInput input)
    {
        try
        {
            var attachements = new List<Attachment>();
            if (input.Files != null && input.Files.Count > 0)
            {
                attachements = input.Files.Select(q => new Attachment(q.FileName, q.OpenReadStream())).ToList();
            }
            var eMailClient = EMailFactory.Create();
            return await eMailClient.SendAsync(Message.FromInput(input, attachements));
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}
