/****************************************************************************
*Copyright @ 2023-2024 Oceania All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Wechat.Models
*文件名： ReceiveLocation
*版本号： V1.0.0.0
*唯一标识：29889579-7430-4028-9b41-580987b5cba0
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/5 14:51:26
*描述：ReceiveLocation 类
*
*=================================================
*修改标记
*修改时间：2022/7/5 14:51:26
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ReceiveLocation 类
*
*****************************************************************************/

namespace LuBan.Wechat.Models
{
    /// <summary>
/// ReceiveLocation 类
/// </summary>
    public class ReceiveLocation : Receive
    {
        /// <summary>
        /// 地理位置消息的纬度（协议字段 Location_X）。
        /// </summary>
        public decimal Location_X { get; set; }

        /// <summary>
        /// 地理位置消息的经度（协议字段 Location_Y）。
        /// </summary>
        public decimal Location_Y { get; set; }

        /// <summary>
        /// 位置对应地图的缩放级别，取值 1 至 20（协议字段 Scale）。
        /// </summary>
        public decimal Scale { get; set; }

        /// <summary>
        /// 地理位置的文字描述，如省市区与街道名称（协议字段 Label）。
        /// </summary>
        public string Label { get; set; }
    }
}
