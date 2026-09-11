/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm.Entities
*文件名： DbJobInfo
*版本号： V1.0.0.0
*唯一标识：00000000-0000-0000-0000-000000000006
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/09/11 00:00:00
*描述：作业配置实体
*
*=================================================
*修改标记
*修改时间：2026/09/11 00:00:00
*修改人： yswenli
*版本号： V1.0.0.0
*描述：作业配置实体
*
*****************************************************************************/
namespace LuBan.Orm.Entities;

/// <summary>
/// 作业配置实体
/// </summary>
[SugarTable("db_job_info", TableDescription = "作业配置")]
[SysTable]
public class DbJobInfo : EntityBase
{
    /// <summary>
    /// 作业名称
    /// </summary>
    [SugarColumn(ColumnDescription = "作业名称", Length = 256,
        IndexGroupNameList = new[] { "Idx_DbJobInfo_Name" })]
    public string Name { get; set; }

    /// <summary>
    /// Cron 表达式
    /// </summary>
    [SugarColumn(ColumnDescription = "Cron表达式", Length = 128)]
    public string Cron { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [SugarColumn(ColumnDescription = "是否启用")]
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = 512, IsNullable = true)]
    public string? Remark { get; set; }
}