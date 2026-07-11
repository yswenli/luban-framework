namespace WebApplication1.Models.Entities;

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
    [SugarColumn(ColumnDescription = "登录设备", Length = 512)]
    public string? Device { get; set; }
}
