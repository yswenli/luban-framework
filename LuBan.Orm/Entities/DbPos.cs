/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm.Entities
*文件名： SysPos
*版本号： V1.0.0.0
*唯一标识：282ef68d-1a96-40fb-974e-554ce598e584
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/29 18:24:00
*描述：系统职位表
*
*=================================================
*修改标记
*修改时间：2023/12/29 18:24:00
*修改人： yswenli
*版本号： V1.0.0.0
*描述：系统职位表
*
*****************************************************************************/
namespace LuBan.Orm.Entities;


/// <summary>
/// 系统职位表
/// </summary>
[SugarTable("db_pos", "系统职位表")]
[SysTable]
public class DbPos : EntityTenant
{
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
