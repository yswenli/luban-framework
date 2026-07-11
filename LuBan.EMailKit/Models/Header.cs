/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.EMailKit.Models
*文件名： EMailHeader
*版本号： V1.0.0.0
*唯一标识：fb3d3ecc-dbdf-4d27-943c-c39d029241cb
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/8/15 10:23:11
*描述：邮件头
*
*=================================================
*修改标记
*修改时间：2024/8/15 10:23:11
*修改人： yswenli
*版本号： V1.0.0.0
*描述：邮件头
*
*****************************************************************************/
namespace LuBan.EMailKit.Models;
/// <summary>
/// 邮件头
/// </summary>
public class Header
{
    /// <summary>
    /// 发件人
    /// </summary>
    public UserAddress From { get; set; }
    /// <summary>
    /// 收件人列表
    /// </summary>
    [Required(ErrorMessage = "请输入收件人")]
    public List<UserAddress> To { get; set; }
    /// <summary>
    /// 抄送人列表
    /// </summary>
    public List<UserAddress> Cc { get; set; }
    /// <summary>
    /// 密送人列表
    /// </summary>
    public List<UserAddress> Bcc { get; set; }
    /// <summary>
    /// 邮件主题
    /// </summary>
    [Required(ErrorMessage = "请输入邮件主题")]
    public string Subject { get; set; }
}
