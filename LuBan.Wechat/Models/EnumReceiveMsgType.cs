/****************************************************************************
*Copyright @ 2023-2024 Oceania All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Wechat.Models
*文件名： EnumReceiveMsgType
*版本号： V1.0.0.0
*唯一标识：9fb5f914-f0ca-4eb5-a283-e9014072be67
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/5 14:42:54
*描述：EnumReceiveMsgType
*
*=================================================
*修改标记
*修改时间：2022/7/5 14:42:54
*修改人： yswenli
*版本号： V1.0.0.0
*描述：EnumReceiveMsgType
*
*****************************************************************************/

namespace LuBan.Wechat.Models;

/// <summary>
/// EnumReceiveMsgType
/// </summary>
public enum EnumReceiveMsgType
{
    None,
    text,
    image,
    voice,
    video,
    location,
    link,
    @event,
    shortvideo
}
