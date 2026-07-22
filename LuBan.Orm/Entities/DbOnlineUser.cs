/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm.Entities
*文件名： DbOnlineUser
*版本号： V1.0.0.0
*唯一标识：a5bb6173-b22d-4edd-852f-9b02bb075167
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/11/03 14:00:15
*描述：
*
*=================================================
*修改标记
*修改时间：2023/11/03 14:00:15
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/

namespace LuBan.Orm.Entities;

/// <summary>
/// 系统在线用户表
/// </summary>
[SugarTable("db_online_user", "系统在线用户表")]
[SysTable]
[SugarIndex("IX_db_online_user_TenantId_UserId", nameof(TenantId), OrderByType.Asc, nameof(UserId), OrderByType.Asc, IsUnique = true)]
[SugarIndex("IX_db_online_user_LastActiveTime", nameof(LastActiveTime), OrderByType.Desc)]
public class DbOnlineUser : EntityTenant
{
    /// <summary>
    /// 用户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "用户Id")]
    public long UserId { get; set; }

    /// <summary>
    /// 用户姓名
    /// </summary>
    [SugarColumn(ColumnDescription = "用户姓名", Length = 64)]
    public string? UserName { get; set; }

    /// <summary>
    /// 登录时间
    /// </summary>
    [SugarColumn(ColumnDescription = "登录时间")]
    public DateTime LoginTime { get; set; }

    /// <summary>
    /// 最后活跃时间
    /// </summary>
    [SugarColumn(ColumnDescription = "最后活跃时间")]
    public DateTime LastActiveTime { get; set; }

    /// <summary>
    /// 登录IP
    /// </summary>
    [SugarColumn(ColumnDescription = "登录IP", Length = 64)]
    public string? Ip { get; set; }

    /// <summary>
    /// 登录设备
    /// </summary>
    [SugarColumn(ColumnDescription = "登录设备", Length = 2048)]
    public string? Device { get; set; }
}