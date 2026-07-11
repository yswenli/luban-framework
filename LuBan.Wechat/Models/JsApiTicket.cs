/****************************************************************************
*Copyright @ 2023-2024 Oceania All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Wechat.Models
*文件名： JsApiTicket
*版本号： V1.0.0.0
*唯一标识：46602dad-7e3c-4fcf-bb2e-99a9d38b5bf0
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/5 14:54:02
*描述：jsapi_ticket
*
*=================================================
*修改标记
*修改时间：2022/7/5 14:54:02
*修改人： yswenli
*版本号： V1.0.0.0
*描述：jsapi_ticket
*
*****************************************************************************/
namespace LuBan.Wechat.Models;

/// <summary>
/// jsapi_ticket
/// </summary>
public class JsApiTicket
{
    /// <summary>
    /// jsapi_ticket
    /// </summary>
    public string Ticket { get; set; }
    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime ExpireTime { get; set; }
}
