/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm.Entities
*文件名： SysUserRole
*版本号： V1.0.0.0
*唯一标识：ec6e796e-f157-4ce5-a4c4-3b7cf9c62273
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/29 19:22:58
*描述：系统用户角色表
*
*=================================================
*修改标记
*修改时间：2023/12/29 19:22:58
*修改人： yswenli
*版本号： V1.0.0.0
*描述：系统用户角色表
*
*****************************************************************************/
namespace LuBan.Orm.Entities;


/// <summary>
/// 系统用户角色表
/// </summary>
/// <summary>
/// 系统用户角色表
/// </summary>
[SugarTable("db_user_role", "系统用户角色表")]
[SysTable]
public class DbUserRole : EntityDataScoreBase
{
    /// <summary>
    /// 用户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "用户Id")]
    public long UserId { get; set; }

    /// <summary>
    /// 用户
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(UserId))]
    public DbUser User { get; set; }

    /// <summary>
    /// 角色Id
    /// </summary>
    [SugarColumn(ColumnDescription = "角色Id")]
    public long RoleId { get; set; }

    /// <summary>
    /// 角色
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(RoleId))]
    public DbRole SysRole { get; set; }
}