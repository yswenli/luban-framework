/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.IO
*文件名： EnumFileType
*版本号： V1.0.0.0
*唯一标识：a58f8496-329f-4ef7-b850-be214f82b0f4
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/8/12 10:35:21
*描述：文件类型枚举
*
*=================================================
*修改标记
*修改时间：2024/8/12 10:35:21
*修改人： yswenli
*版本号： V1.0.0.0
*描述：文件类型枚举
*
*****************************************************************************/
namespace LuBan.Common.IO;

/// <summary>
/// 文件类型枚举
/// </summary>
public enum EnumFileType
{
    /// <summary>
    /// 未知
    /// </summary>
    [Description("未知")]
    None = 0,
    /// <summary>
    /// 图片
    /// </summary>
    [Description("图片")]
    Image = 1,
    /// <summary>
    /// 视频
    /// </summary>
    [Description("视频")]
    Video = 2,
    /// <summary>
    /// excel文件
    /// </summary>
    [Description("excel文件")]
    Excel = 3,
    /// <summary>
    /// PDF文件
    /// </summary>
    [Description("PDF文件")]
    PDF = 4,
    /// <summary>
    /// Word文件
    /// </summary>
    [Description("Word文件")]
    Word = 5,
    /// <summary>
    /// Zip文件
    /// </summary>
    [Description("Zip文件")]
    Zip = 6,
}
