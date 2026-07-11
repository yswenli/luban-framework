/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Reporting.Dynamic.Entities
*文件名： DbReportConfig
*版本号： V1.0.0.0
*唯一标识：
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/6/5
*描述：报表配置实体
*
*=================================================
*修改标记
*修改时间：2026/6/5
*修改人： yswenli
*版本号： V1.0.0.0
*描述：报表配置实体
*
*****************************************************************************/

namespace LuBan.Reporting.Dynamic.Entities;

/// <summary>
/// 报表配置实体
/// </summary>
[SugarTable("db_report_config")]
public class DbReportConfig : EntityBase
{
    /// <summary>
    /// 报表名称
    /// </summary>
    [SugarColumn(ColumnDescription = "报表名称", Length = 200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// SQL 模板（Lua 脚本，接收 params 返回 SQL）
    /// </summary>
    [SugarColumn(ColumnDescription = "SQL模板", ColumnDataType = "text")]
    public string SqlTemplate { get; set; } = string.Empty;

    /// <summary>
    /// 报表描述
    /// </summary>
    [SugarColumn(ColumnDescription = "报表描述", Length = 500, IsNullable = true)]
    public string? Description { get; set; }

    /// <summary>
    /// 预览行数（默认 100）
    /// </summary>
    [SugarColumn(ColumnDescription = "预览行数", DefaultValue = "100")]
    public int PreviewRows { get; set; } = 100;

    /// <summary>
    /// 列转换用的 Lua 脚本（定义转换函数）
    /// </summary>
    [SugarColumn(ColumnDescription = "列转换Lua脚本", ColumnDataType = "text", IsNullable = true)]
    public string? LuaScript { get; set; }

    /// <summary>
    /// 状态（1=启用，0=禁用）
    /// </summary>
    [SugarColumn(ColumnDescription = "状态", DefaultValue = "1")]
    public int Status { get; set; } = 1;
}
