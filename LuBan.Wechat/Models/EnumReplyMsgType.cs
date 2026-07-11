/****************************************************************************
*Copyright @ 2023-2024 Oceania All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Wechat.Models
*文件名： EnumReplyMsgType
*版本号： V1.0.0.0
*唯一标识：eefc8bb8-fd36-4023-a7fa-731f87ef067e
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/5 14:56:50
*描述：EnumReplyMsgType
*
*=================================================
*修改标记
*修改时间：2022/7/5 14:56:50
*修改人： yswenli
*版本号： V1.0.0.0
*描述：EnumReplyMsgType
*
*****************************************************************************/
namespace LuBan.Wechat.Models;

/// <summary>
/// ReplyMsgType
/// </summary>
public enum EnumReplyMsgType
{
    text = 1,
    image,
    voice,
    video,
    music,
    news
}
