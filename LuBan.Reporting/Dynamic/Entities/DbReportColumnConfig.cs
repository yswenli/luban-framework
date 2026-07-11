/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Reporting.Dynamic.Entities
*文件名： DbReportColumnConfig
*版本号： V1.0.0.0
*唯一标识：
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/6/5
*描述：报表列配置实体
*
*=================================================
*修改标记
*修改时间：2026/6/5
*修改人： yswenli
*版本号： V1.0.0.0
*描述：报表列配置实体
*
*****************************************************************************/



namespace LuBan.Reporting.Dynamic.Entities;

/// <summary>
/// 报表列配置实体
/// </summary>
[SugarTable("db_report_column_config")]
public class DbReportColumnConfig : EntityBase
{
    /// <summary>
    /// 关联报表配置ID
    /// </summary>
    [SugarColumn(ColumnDescription = "报表配置ID", IndexGroupNameList = new[] { "idx_report_column" })]
    public long ReportConfigId { get; set; }

    /// <summary>
    /// 原始列名（SQL 结果中的列名）
    /// </summary>
    [SugarColumn(ColumnDescription = "原始列名", Length = 200)]
    public string ColumnName { get; set; } = string.Empty;

    /// <summary>
    /// 显示列名（导出后的列标题）
    /// </summary>
    [SugarColumn(ColumnDescription = "显示列名", Length = 200)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 排序号
    /// </summary>
    [SugarColumn(ColumnDescription = "排序号", DefaultValue = "0")]
    public int SortNo { get; set; }

    /// <summary>
    /// 转换类型：None / ValueMap / LuaScript
    /// </summary>
    [SugarColumn(ColumnDescription = "转换类型", Length = 50)]
    public string ConverterType { get; set; } = "None";

    /// <summary>
    /// 转换配置（ValueMap 为 JSON，LuaScript 为函数调用表达式）
    /// </summary>
    [SugarColumn(ColumnDescription = "转换配置", ColumnDataType = "text", IsNullable = true)]
    public string? ConverterConfig { get; set; }

    /// <summary>
    /// 是否显示
    /// </summary>
    [SugarColumn(ColumnDescription = "是否显示", DefaultValue = "1")]
    public bool IsVisible { get; set; } = true;
}
