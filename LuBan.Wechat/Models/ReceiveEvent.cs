/****************************************************************************
*Copyright @ 2023-2024 Oceania All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Wechat.Models
*文件名： ReceiveEvent
*版本号： V1.0.0.0
*唯一标识：46602dad-7e3c-4fcf-bb2e-99a9d38b5bf0
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/5 14:54:02
*描述：ReceiveEvent 类
*
*=================================================
*修改标记
*修改时间：2022/7/5 14:54:02
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ReceiveEvent 类
*
*****************************************************************************/

namespace LuBan.Wechat.Models
{
    /// <summary>
/// ReceiveEvent 类
/// </summary>
    public class ReceiveEvent : Receive
    {
        /// <summary>
        /// 事件类型，取值为 subscribe、unsubscribe、click、location 等（协议字段 Event）。
        /// </summary>
        public string Event { get; set; }

        /// <summary>
        /// 事件 KEY，扫码或点击带参菜单时承载二维码场景值（协议字段 EventKey）。
        /// </summary>
        public string EventKey { get; set; }

        /// <summary>
        /// 获取二维码事件的 Ticket，可用于换取带参二维码图片（协议字段 Ticket）。
        /// </summary>
        public string Ticket { get; set; }

        /// <summary>
        /// 上报地理位置事件的纬度（协议字段 Latitude）。
        /// </summary>
        public decimal Latitude { get; set; }

        /// <summary>
        /// 上报地理位置事件的经度（协议字段 Longitude）。
        /// </summary>
        public decimal Longitude { get; set; }

        /// <summary>
        /// 上报地理位置的精度，单位为米（协议字段 Precision）。
        /// </summary>
        public decimal Precision { get; set; }

        /// <summary>
        /// 群发消息任务的消息 ID（协议字段 MsgID）。
        /// </summary>
        public string MsgID { get; set; }

        /// <summary>
        /// 群发或模板消息任务的发送状态，取值如 send success（协议字段 Status）。
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 群发任务的目标用户总数（协议字段 TotalCount）。
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 群发任务经条件过滤后实际待发送的用户数（协议字段 FilterCount）。
        /// </summary>
        public int FilterCount { get; set; }

        /// <summary>
        /// 群发任务已成功发送的用户数（协议字段 SentCount）。
        /// </summary>
        public int SentCount { get; set; }

        /// <summary>
        /// 群发任务发送失败的用户数（协议字段 ErrorCount）。
        /// </summary>
        public int ErrorCount { get; set; }
    }
}
