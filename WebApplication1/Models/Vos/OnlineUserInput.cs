/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：WebApplication1.Models.Vos
*文件名： OnlineUserInput.cs
*版本号： V1.0.0.0
*唯一标识：f0f4d763-7600-47b7-b2d4-34b0dc73c004
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:30
*描述：OnlineUserInput 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：OnlineUserInput 类
*
*****************************************************************************/

namespace WebApplication1.Models.Vos;

/// <summary>
/// 在线用户分页查询
/// </summary>
public class PageOnlineUserInput : BasePageInput
{
    /// <summary>
    /// 用户姓名
    /// </summary>
    public string? UserName { get; set; }
}

/// <summary>
/// 踢用户下线
/// </summary>
public class KickOnlineUserInput : BaseIdInput
{
    /// <summary>
    /// 租户Id
    /// </summary>
    public long TenantId { get; set; }
}
