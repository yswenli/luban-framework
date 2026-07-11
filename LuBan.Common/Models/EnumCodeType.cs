/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common.Models
*文件名： EnumCodeType
*版本号： V1.0.0.0
*唯一标识：bcb4ff24-397a-4c40-8b6b-ea30c91fb080
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/6/21 16:52:24
*描述：图形码类型
*
*=================================================
*修改标记
*修改时间：2022/6/21 16:52:24
*修改人： yswenli
*版本号： V1.0.0.0
*描述：图形码类型
*
*****************************************************************************/

namespace LuBan.Common.Models;

/// <summary>
/// 图形码类型
/// </summary>
public enum EnumCodeType : byte
{
    /// <summary>
    /// 未知
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// 条形码
    /// </summary>
    Code128 = 1,
    /// <summary>
    /// 二维码
    /// </summary>
    QRCode = 2
}
