/****************************************************************************
*Copyright @ 2023-2024 Oceania All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Wechat.Models
*文件名： ReceiveVideo
*版本号： V1.0.0.0
*唯一标识：08be66fb-1914-431b-bd47-aab2954b065f
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/5 14:51:12
*描述：
*
*=================================================
*修改标记
*修改时间：2022/7/5 14:51:12
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/

namespace LuBan.Wechat.Models
{
    public class ReceiveVideo : Receive
    {
        public string MediaId { get; set; }

        public string ThumbMediaId { get; set; }
    }
}
