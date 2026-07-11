/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm.Entities
*文件名： SysTenant
*版本号： V1.0.0.0
*唯一标识：1aef0438-5e90-43a9-87ee-1315099ed5c3
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/29 18:18:52
*描述：系统租户表
*
*=================================================
*修改标记
*修改时间：2023/12/29 18:18:52
*修改人： yswenli
*版本号： V1.0.0.0
*描述：系统租户表
*
*****************************************************************************/
namespace LuBan.Orm.Entities;

/// <summary>
/// 系统租户表
/// </summary>
[SugarTable("db_tenant", "系统租户表")]
[SysTable]
public class DbTenant : EntityBase
{
    /// <summary>
    /// 用户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "用户Id")]
    public long UserId { get; set; }

    /// <summary>
    /// 机构Id
    /// </summary>
    [SugarColumn(ColumnDescription = "机构Id")]
    public long OrgId { get; set; }

    /// <summary>
    /// 主机
    /// </summary>
    [SugarColumn(ColumnDescription = "主机", Length = 128)]
    [MaxLength(128)]
    public string? Host { get; set; }

    /// <summary>
    /// 租户类型
    /// </summary>
    [SugarColumn(ColumnDescription = "租户类型")]
    public EnumTenantType TenantType { get; set; }

    /// <summary>
    /// 数据库类型
    /// </summary>
    [SugarColumn(ColumnDescription = "数据库类型")]
    public DbType DbType { get; set; }

    /// <summary>
    /// 数据库连接
    /// </summary>
    [SugarColumn(ColumnDescription = "数据库连接", Length = 256)]
    [MaxLength(256)]
    public string? Connection { get; set; }

    /// <summary>
    /// 数据库标识
    /// </summary>
    [SugarColumn(ColumnDescription = "数据库标识", Length = 64)]
    [MaxLength(64)]
    public string? ConfigId { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序")]
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = 128)]
    [MaxLength(128)]
    public string? Remark { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public EnumEnableStatus Status { get; set; } = EnumEnableStatus.Enable;
}
