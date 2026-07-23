/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm.Entities
*文件名： SysUserExtOrg
*版本号： V1.0.0.0
*唯一标识：c0cc43b1-7467-4045-8d8c-e6949e8c55e9
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/29 19:24:13
*描述：系统用户扩展机构表
*
*=================================================
*修改标记
*修改时间：2023/12/29 19:24:13
*修改人： yswenli
*版本号： V1.0.0.0
*描述：系统用户扩展机构表
*
*****************************************************************************/
namespace LuBan.Orm.Entities;


/// <summary>
/// 系统用户扩展机构表
/// </summary>
[SugarTable("db_user_ext_org", "系统用户扩展机构表")]
[SysTable]
public class DbUserExtOrg : EntityDataScoreBase, IDeletedFilter
{
    /// <summary>
    /// 用户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "用户Id")]
    public long UserId { get; set; }

    /// <summary>
    /// 用户
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(UserId)), JsonIgnore]
    public DbUser User { get; set; }

    /// <summary>
    /// 机构Id
    /// </summary>
    [SugarColumn(ColumnDescription = "机构Id")]
    public long OrgId { get; set; }

    /// <summary>
    /// 机构
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(OrgId)), JsonIgnore]
    public DbOrg Organization { get; set; }

    /// <summary>
    /// 职位Id
    /// </summary>
    [SugarColumn(ColumnDescription = "职位Id")]
    public long PosId { get; set; }

    /// <summary>
    /// 职位
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(PosId)), JsonIgnore]
    public DbPos Position { get; set; }

    /// <summary>
    /// 工号
    /// </summary>
    [SugarColumn(ColumnDescription = "工号", Length = 32)]
    [MaxLength(32)]
    public string? JobNum { get; set; }

    /// <summary>
    /// 职级
    /// </summary>
    [SugarColumn(ColumnDescription = "职级", Length = 32)]
    [MaxLength(32)]
    public string? PosLevel { get; set; }

    /// <summary>
    /// 入职日期
    /// </summary>
    [SugarColumn(ColumnDescription = "入职日期")]
    public DateTime? JoinDate { get; set; }
}
