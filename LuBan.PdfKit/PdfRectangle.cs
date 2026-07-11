/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.PdfKit
*文件名： PdfRectangle
*版本号： V1.0.0.0
*唯一标识：16b0b162-02c7-44fd-ba27-1b1026a6dc5c
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/8/11 17:35:07
*描述：pdf矩形区域
*
*=================================================
*修改标记
*修改时间：2025/8/11 17:35:07
*修改人： yswenli
*版本号： V1.0.0.0
*描述：pdf矩形区域
*
*****************************************************************************/
namespace LuBan.PdfKit;

/// <summary>
/// pdf矩形区域
/// </summary>
public struct PdfRectangle
{
    public float Left;
    public float Top;
    public float Width;
    public float Height;
    /// <summary>
    /// pdf矩形区域
    /// </summary>
    /// <param name="left"></param>
    /// <param name="top"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public PdfRectangle(float left, float top, float width, float height)
    {
        Left = left;
        Top = top;
        Width = width;
        Height = height;
    }
}
