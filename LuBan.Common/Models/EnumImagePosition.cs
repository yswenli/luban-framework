/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.Models
*文件名： EnumImagePosition
*版本号： V1.0.0.0
*唯一标识：3a79ad36-905e-44b3-b325-5fe82900512b
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/11/19 14:12:04
*描述：
*
*=================================================
*修改标记
*修改时间：2025/11/19 14:12:04
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Common.Models;


/// <summary>
/// 图片位置枚举
/// </summary>
using System.ComponentModel;

/// <summary>
/// 图片位置枚举类
/// 用于定义图片在容器中的九个标准位置
/// </summary>
public enum EnumImagePosition
{
    /// <summary>
    /// 左上角位置
    /// </summary>
    [Description("左上角")]
    TopLeft = 1,

    /// <summary>
    /// 顶部居中位置
    /// </summary>
    [Description("顶部居中")]
    TopCenter = 2,

    /// <summary>
    /// 右上角位置
    /// </summary>
    [Description("右上角")]
    TopRight = 3,

    /// <summary>
    /// 左侧中间位置
    /// </summary>
    [Description("左侧中间")]
    MiddleLeft = 4,

    /// <summary>
    /// 正中间位置
    /// </summary>
    [Description("正中间")]
    MiddleCenter = 5,

    /// <summary>
    /// 右侧中间位置
    /// </summary>
    [Description("右侧中间")]
    MiddleRight = 6,

    /// <summary>
    /// 左下角位置
    /// </summary>
    [Description("左下角")]
    BottomLeft = 7,

    /// <summary>
    /// 底部居中位置
    /// </summary>
    [Description("底部居中")]
    BottomCenter = 8,

    /// <summary>
    /// 右下角位置
    /// </summary>
    [Description("右下角")]
    BottomRight = 9
}