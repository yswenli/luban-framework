/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：
*文件名： 
*版本号： V1.0.0.0
*唯一标识：a5bb6173-b22d-4edd-852f-9b02bb075167
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/11/03 14:00:15
*描述：系统行政地区表
*
*=================================================
*修改标记
*修改时间：2023/11/03 14:00:15
*修改人： yswenli
*版本号： V1.0.0.0
*描述：系统行政地区表
*
*****************************************************************************/

namespace LuBan.Orm.Entities;

/// <summary>
/// 系统行政地区表
/// </summary>
[SugarTable("db_region", "系统行政地区表")]
[SysTable]
public class DbRegion : EntityBase
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
    /// 简称
    /// </summary>
    [SugarColumn(ColumnDescription = "简称", Length = 32)]
    [MaxLength(32)]
    public string? ShortName { get; set; }

    /// <summary>
    /// 组合名
    /// </summary>
    [SugarColumn(ColumnDescription = "组合名", Length = 64)]
    [MaxLength(64)]
    public string? MergerName { get; set; }

    /// <summary>
    /// 行政代码
    /// </summary>
    [SugarColumn(ColumnDescription = "行政代码", Length = 32)]
    [MaxLength(32)]
    public string? Code { get; set; }

    /// <summary>
    /// 邮政编码
    /// </summary>
    [SugarColumn(ColumnDescription = "邮政编码", Length = 6)]
    [MaxLength(6)]
    public string? ZipCode { get; set; }

    /// <summary>
    /// 区号
    /// </summary>
    [SugarColumn(ColumnDescription = "区号", Length = 6)]
    [MaxLength(6)]
    public string? CityCode { get; set; }

    /// <summary>
    /// 层级
    /// </summary>
    [SugarColumn(ColumnDescription = "层级")]
    public int Level { get; set; }

    /// <summary>
    /// 拼音
    /// </summary>
    [SugarColumn(ColumnDescription = "拼音", Length = 128)]
    [MaxLength(128)]
    public string? PinYin { get; set; }

    /// <summary>
    /// 经度
    /// </summary>
    [SugarColumn(ColumnDescription = "经度")]
    public float Lng { get; set; }

    /// <summary>
    /// 维度
    /// </summary>
    [SugarColumn(ColumnDescription = "维度")]
    public float Lat { get; set; }

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
    /// 机构子项
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    [Navigate(NavigateType.OneToMany, nameof(Pid)), JsonIgnore]
    public List<DbRegion> Children { get; set; }
}