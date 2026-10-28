/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Attachments
*文件名： AttachmentKind
*版本号： V1.0.0.0
*唯一标识：c3189d2f-7da8-4f50-a2c8-d8aa5fbdd3c5
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/18
*描述：附件类型。
*
*=================================================
*修改标记
*修改时间：2026/9/18
*修改人： yswenli
*版本号： V1.0.0.0
*描述：附件类型。
*
*****************************************************************************/

namespace LuBan.AIAgent.Attachments;

/// <summary>
/// 附件类型。
/// </summary>
public enum AttachmentKind
{
    /// <summary>图片，走多模态。</summary>
    Image,

    /// <summary>文本文件，注入或按需读取。</summary>
    TextFile
}