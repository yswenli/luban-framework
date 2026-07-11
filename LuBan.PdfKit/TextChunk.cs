/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.PdfKit
*文件名： TextChunk
*版本号： V1.0.0.0
*唯一标识：b1cce00d-5ab2-4440-89a4-997a2821ac73
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/8/11 17:57:16
*描述：文本块信息
*
*=================================================
*修改标记
*修改时间：2025/8/11 17:57:16
*修改人： yswenli
*版本号： V1.0.0.0
*描述：文本块信息
*
*****************************************************************************/
namespace LuBan.PdfKit;

/// <summary>
/// 文本块信息
/// </summary>
public class TextChunk
{
    public float Left { get; set; }
    public float Bottom { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
}
