/****************************************************************************
*Copyright @ 2023-2024 Oceania All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Wechat.Models
*文件名： ReceiveText
*版本号： V1.0.0.0
*唯一标识：3626a7a7-153d-4224-ae14-0df6ce90583d
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/5 14:49:44
*描述：
*
*=================================================
*修改标记
*修改时间：2022/7/5 14:49:44
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Wechat.Models
{
    public class ReceiveText : Receive
    {
        public string Content { get; set; }
    }
}
