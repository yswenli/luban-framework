/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.EMailKit.Models
*文件名： MsgInput
*版本号： V1.0.0.0
*唯一标识：4daf0f2d-6c06-4619-a075-07ed087ce81d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/8/15 14:23:47
*描述：邮件输入请求
*
*=================================================
*修改标记
*修改时间：2024/8/15 14:23:47
*修改人： yswenli
*版本号： V1.0.0.0
*描述：邮件输入请求
*
*****************************************************************************/
namespace LuBan.EMailKit.Models;

/// <summary>
/// 邮件输入请求
/// </summary>
public class MsgInput
{
    /// <summary>
    /// 收件人名称
    /// </summary>
    public string ToName { get; set; }

    /// <summary>
    /// 收件人邮件地址
    /// </summary>
    [Required(ErrorMessage = "请输入收件人邮件地址")]
    public string To { get; set; }
    /// <summary>
    /// 邮件主题
    /// </summary>
    [Required(ErrorMessage = "请输入邮件主题")]
    public string Subject { get; set; }
    /// <summary>
    /// 邮件内容
    /// </summary>
    [Required(ErrorMessage = "请输入邮件内容")]
    public string Body { get; set; }
    /// <summary>
    /// 邮件内容类型
    /// </summary>
    [Required(ErrorMessage = "请输入邮件内容类型")]
    public bool IsHtml { get; set; }
}
