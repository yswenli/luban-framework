/****************************************************************************
*Copyright @ 2023-2024 Oceania All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Wechat.Models
*文件名： ReceiveLink
*版本号： V1.0.0.0
*唯一标识：096f24d6-72c7-449f-be89-e35971337441
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/5 14:53:28
*描述：
*
*=================================================
*修改标记
*修改时间：2022/7/5 14:53:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/

namespace LuBan.Wechat.Models
{
    public class ReceiveLink : Receive
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string Url { get; set; }
    }
}
