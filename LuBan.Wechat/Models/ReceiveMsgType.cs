/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Wechat.Models
*文件名： ReceiveMsgType.cs
*版本号： V1.0.0.0
*唯一标识：c925b40e-7d5f-4384-9ac0-960e497892f9
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:29
*描述：ReceiveMsgType 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ReceiveMsgType 类
*
*****************************************************************************/

namespace LuBan.Wechat.Models;
/// <summary>
/// ReceiveMsgType
/// </summary>
public class ReceiveMsgType
{
    public const string text = "text";

    public const string image = "image";

    public const string voice = "voice";

    public const string video = "video";

    public const string location = "location";

    public const string link = "link";

    public const string Event = "event";

    public const string shortvideo = "shortvideo";
}
