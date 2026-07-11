/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm.Entities
*文件名： DbJobLog
*版本号： V1.0.0.0
*唯一标识：00000000-0000-0000-0000-000000000003
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/01/13 00:00:00
*描述：作业运行日志实体
*
*=================================================
*修改标记
*修改时间：2026/01/13 00:00:00
*修改人： yswenli
*版本号： V1.0.0.0
*描述：作业运行日志实体
*
*****************************************************************************/
namespace LuBan.Orm.Entities;

/// <summary>
/// 作业运行日志实体
/// </summary>
[SugarTable("db_log_job", TableDescription = "作业运行日志")]
[SysTable]
public class DbLogJob : EntityBase
{
    /// <summary>
    /// 作业名称
    /// </summary>
    [SugarColumn(ColumnDescription = "作业名称", Length = 256, IndexGroupNameList = new string[] { "Idx_DbJobLog_Name" })]
    public string Name { get; set; }

    /// <summary>
    /// 运行开始时间
    /// </summary>
    [SugarColumn(ColumnDescription = "运行开始时间", IndexGroupNameList = new string[] { "Idx_DbJobLog_StartTime" })]
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 运行结束时间
    /// </summary>
    [SugarColumn(ColumnDescription = "运行结束时间", IndexGroupNameList = new string[] { "Idx_DbJobLog_EndTime" })]
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 运行状态
    /// </summary>
    [SugarColumn(ColumnDescription = "运行状态")]
    public EnumJobStatus Status { get; set; }

    /// <summary>
    /// 运行结果
    /// </summary>
    [SugarColumn(ColumnDescription = "运行结果")]
    public EnumJobResult? Result { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    [SugarColumn(ColumnDescription = "消息", Length = int.MaxValue, IsNullable = true)]
    public string? Message { get; set; }

    /// <summary>
    /// 运行时长（毫秒）
    /// </summary>
    [SugarColumn(ColumnDescription = "运行时长（毫秒）")]
    public long? Duration { get; set; }
}
