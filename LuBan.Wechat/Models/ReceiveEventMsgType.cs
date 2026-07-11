/****************************************************************************
*Copyright @ 2023-2024 Walle All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Wechat.Models
*文件名： ReceiveEventMsgType
*版本号： V1.0.0.0
*唯一标识：4fc5d566-92af-412c-805d-b302a7ba3ee9
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/26 10:23:29
*描述：ReceiveEventMsgType
*
*=================================================
*修改标记
*修改时间：2022/7/26 10:23:29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ReceiveEventMsgType
*
*****************************************************************************/
namespace LuBan.Wechat.Models
{
    /// <summary>
    /// ReceiveEventMsgType
    /// </summary>
    public class ReceiveEventMsgType
    {
        public const string subscribe = "subscribe";

        public const string scan = "scan";

        public const string unsubscribe = "unsubscribe";

        public const string location = "location";

        public const string click = "click";

        public const string view = "view";

        public const string scancode_push = "scancode_push";

        public const string scancode_waitmsg = "scancode_waitmsg";

        public const string pic_sysphoto = "pic_sysphoto";

        public const string pic_photo_or_album = "pic_photo_or_album";

        public const string pic_weixin = "pic_weixin";

        public const string location_select = "location_select";

        public const string masssendjobfinish = "masssendjobfinish";

        public const string templatesendjobfinish = "templatesendjobfinish";
    }
}
