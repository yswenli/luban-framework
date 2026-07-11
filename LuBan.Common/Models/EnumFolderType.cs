/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.Models
*文件名： EnumFolderType
*版本号： V1.0.0.0
*唯一标识：a394fadb-0368-4439-accf-7f8f3aaafe9b
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/9/14 14:55:20
*描述：专用目录
*
*=================================================
*修改标记
*修改时间：2024/9/14 14:55:20
*修改人： yswenli
*版本号： V1.0.0.0
*描述：专用目录
*
*****************************************************************************/
namespace LuBan.Common.Models;

/// <summary>
/// 专用目录 
/// </summary>
public enum EnumFolderType
{
    /// <summary>
    /// 应用程序目录
    /// </summary>
    App = 0,
    /// <summary>
    /// 网站根目录
    /// </summary>
    Wwwroot = 1,
    /// <summary>
    /// 上传文件目录
    /// </summary>
    Upload = 2
}
