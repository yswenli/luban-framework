/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Models
*文件名： OpenAccessUserIdExpired
*版本号： V1.0.0.0
*唯一标识：609572db-d6cf-4a5c-9077-7e742a0268f1
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/4/10 13:57:28
*描述：
*
*=================================================
*修改标记
*修改时间：2025/4/10 13:57:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Web.Core.Models;

/// <summary>
/// 开放用户id过期
/// </summary>
internal class OpenAccessUserIdExpired
{
    /// <summary>
    /// 用户id
    /// </summary>
    public long UserId { get; set; }
    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime Expired { get; set; }
}
