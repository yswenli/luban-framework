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
*描述：系统参数配置表
*
*=================================================
*修改标记
*修改时间：2023/11/03 14:00:15
*修改人： yswenli
*版本号： V1.0.0.0
*描述：系统参数配置表
*
*****************************************************************************/

namespace LuBan.Orm.Entities;

/// <summary>
/// 系统参数配置表
/// </summary>
[SugarTable("db_config", "系统参数配置表")]
[SysTable]
public class DbConfig : EntityBase
{
    /// <summary>
    /// 名称
    /// </summary>
    [SugarColumn(ColumnDescription = "名称", Length = 150)]
    [Required(ErrorMessage = "名称不能为空"), MaxLength(150, ErrorMessage = "名称不能超过64个字符")]
    public string Name { get; set; }

    /// <summary>
    /// 编码
    /// </summary>
    [SugarColumn(ColumnDescription = "编码", Length = 150)]
    [MaxLength(150, ErrorMessage = "编码不能超过150个字符")]
    public string? Code { get; set; }

    /// <summary>
    /// 属性值
    /// </summary>
    [SugarColumn(ColumnDescription = "属性值", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    [MaxLength(int.MaxValue, ErrorMessage = "属性值不能超过5000个字符")]
    public string? Value { get; set; }

    /// <summary>
    /// 是否是内置参数（Y-是，N-否）
    /// </summary>
    [SugarColumn(ColumnDescription = "是否是内置参数")]
    public EnumYesNo SysFlag { get; set; }

    /// <summary>
    /// 分组编码
    /// </summary>
    [SugarColumn(ColumnDescription = "分组编码", Length = 150)]
    [MaxLength(64, ErrorMessage = "分组编码不能超过64个字符")]
    public string? GroupCode { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序")]
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = 256)]
    [MaxLength(256, ErrorMessage = "备注不能超过256个字符")]
    public string? Remark { get; set; }
}