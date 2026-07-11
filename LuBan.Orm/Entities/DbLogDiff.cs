/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm.Entities
*文件名： SysLogDiff
*版本号： V1.0.0.0
*唯一标识：29441eea-80dc-4e10-9e05-c4489d65f9d2
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/29 19:25:18
*描述：系统差异日志表
*
*=================================================
*修改标记
*修改时间：2023/12/29 19:25:18
*修改人： yswenli
*版本号： V1.0.0.0
*描述：系统差异日志表
*
*****************************************************************************/
namespace LuBan.Orm.Entities;


/// <summary>
/// 系统差异日志表
/// </summary>
[SugarTable("db_log_diff", "系统差异日志表")]
[SysTable]
[LogTable]
public class DbLogDiff : EntityBase
{
    /// <summary>
    /// 操作前记录
    /// </summary>
    [SugarColumn(ColumnDescription = "操作前记录", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? BeforeData { get; set; }

    /// <summary>
    /// 操作后记录
    /// </summary>
    [SugarColumn(ColumnDescription = "操作后记录", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? AfterData { get; set; }

    /// <summary>
    /// Sql
    /// </summary>
    [SugarColumn(ColumnDescription = "Sql", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? Sql { get; set; }

    /// <summary>
    /// 参数  手动传入的参数
    /// </summary>
    [SugarColumn(ColumnDescription = "参数", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? Parameters { get; set; }

    /// <summary>
    /// 业务对象
    /// </summary>
    [SugarColumn(ColumnDescription = "业务对象", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? BusinessData { get; set; }

    /// <summary>
    /// 差异操作
    /// </summary>
    [SugarColumn(ColumnDescription = "差异操作", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? DiffType { get; set; }

    /// <summary>
    /// 耗时
    /// </summary>
    [SugarColumn(ColumnDescription = "耗时")]
    public long? Elapsed { get; set; }
}
