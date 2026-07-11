/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm.Entities
*文件名： SysRole
*版本号： V1.0.0.0
*唯一标识：acefe550-7dcd-4f85-bef1-0a98cce557c9
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/29 19:21:35
*描述：系统角色表
*
*=================================================
*修改标记
*修改时间：2023/12/29 19:21:35
*修改人： yswenli
*版本号： V1.0.0.0
*描述：系统角色表
*
*****************************************************************************/
namespace LuBan.Orm.Entities;


/// <summary>
/// 系统角色表
/// </summary>
[SugarTable("db_role", "系统角色表")]
[SysTable]
public class DbRole : EntityTenant
{
    /// <summary>
    /// 父Id
    /// </summary>
    [SugarColumn(ColumnDescription = "父Id")]
    public long Pid { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [SugarColumn(ColumnDescription = "名称", Length = 64)]
    [Required, MaxLength(64)]
    public string Name { get; set; }

    /// <summary>
    /// 编码
    /// </summary>
    [SugarColumn(ColumnDescription = "编码", Length = 64)]
    [MaxLength(64)]
    public string? Code { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序")]
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 数据范围（1全部数据 2本部门及以下数据 3本部门数据 4仅本人数据 5自定义数据）
    /// </summary>
    [SugarColumn(ColumnDescription = "数据范围")]
    public EnumDataScope DataScope { get; set; } = EnumDataScope.Self;

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
