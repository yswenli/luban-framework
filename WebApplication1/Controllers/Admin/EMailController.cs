/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：WebApplication1.Controllers.Admin
*文件名： EMailController.cs
*版本号： V1.0.0.0
*唯一标识：75323e20-31a6-42e7-a2b7-bef6bca04791
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:30
*描述：EMailController 控制器
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：EMailController 控制器
*
*****************************************************************************/

using LuBan.EMailKit;
using LuBan.EMailKit.Models;

using WebApplication1.Models.Vos;

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
