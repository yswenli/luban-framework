/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Web.Core.OnlineUser
*文件名： OnlineUserSession
*版本号： V1.0.0.0
*唯一标识：0506282b-82ec-4dd9-aede-99d02655c571
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/29 18:53:58
*描述：在线用户会话信息
*
*=================================================
*修改标记
*修改时间：2023/12/29 18:53:58
*修改人： yswenli
*版本号： V1.0.0.0
*描述：在线用户会话信息
*
*****************************************************************************/

using LuBan.Orm.Enums;

namespace LuBan.Web.Core.OnlineUser;

/// <summary>
/// 在线用户会话信息
/// </summary>
public class OnlineUserSession
{
    /// <summary>
    /// 用户Id
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 租户Id
    /// </summary>
    public long TenantId { get; set; }

    /// <summary>
    /// 用户姓名
    /// </summary>
    public string UserName { get; set; } = "";

    /// <summary>
    /// 登录时间
    /// </summary>
    public DateTime LoginTime { get; set; }

    /// <summary>
    /// 最后活跃时间
    /// </summary>
    public DateTime LastActiveTime { get; set; }

    /// <summary>
    /// 登录IP
    /// </summary>
    public string Ip { get; set; } = "";

    /// <summary>
    /// 登录设备
    /// </summary>
    public string Device { get; set; } = "";

    /// <summary>
    /// 用户状态
    /// </summary>
    public EnumEnableStatus EnableStatus { get; set; } = EnumEnableStatus.Enable;
}
