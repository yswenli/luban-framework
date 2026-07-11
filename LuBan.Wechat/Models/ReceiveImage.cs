/****************************************************************************
*Copyright @ 2023-2024 Oceania All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Wechat.Models
*文件名： ReceiveImage
*版本号： V1.0.0.0
*唯一标识：6c7a67b2-7e98-41fa-8114-15ed96454e94
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/5 14:50:10
*描述：
*
*=================================================
*修改标记
*修改时间：2022/7/5 14:50:10
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Wechat.Models
{
    public class ReceiveImage : Receive
    {
        public string PicUrl { get; set; }

        public string MediaId { get; set; }
    }
}
