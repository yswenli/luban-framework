/****************************************************************************
*Copyright @ 2023-2024 Oceania All Rights Reserved.
*CLR版本： .net8.0
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
*描述：
*
*=================================================
*修改标记
*修改时间：2022/7/5 14:51:26
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/

namespace LuBan.Wechat.Models
{
    public class ReceiveLocation : Receive
    {
        public decimal Location_X { get; set; }

        public decimal Location_Y { get; set; }

        public decimal Scale { get; set; }

        public string Label { get; set; }
    }
}
