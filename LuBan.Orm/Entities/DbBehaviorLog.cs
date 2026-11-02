/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.Orm.Entities
*文件名： DbBehaviorLog
*描述：用户行为记录表
*
*****************************************************************************/

namespace LuBan.Orm.Entities;

/// <summary>
/// 用户行为记录表
/// </summary>
[SugarTable("db_behavior_log", "用户行为记录表")]
public class DbBehaviorLog : EntityDataScoreBase
{
    /// <summary>
    /// 行为类型
    /// </summary>
    [SugarColumn(ColumnDescription = "行为类型", IsNullable = true)]
    public EnumBehaviorType? BehaviorType { get; set; }

    /// <summary>
    /// 操作编号（业务对象主键）
    /// </summary>
    [SugarColumn(ColumnDescription = "操作编号", IsNullable = true)]
    public long? OperationID { get; set; }

    /// <summary>
    /// 操作类型
    /// </summary>
    [SugarColumn(ColumnDescription = "操作类型", IsNullable = true)]
    public EnumBehaviorLogOperateType? OperateType { get; set; }

    /// <summary>
    /// 观看进度值
    /// </summary>
    [SugarColumn(ColumnDescription = "观看进度值", Length = 18, DecimalDigits = 8, IsNullable = true)]
    public decimal? ProcessVal { get; set; }

    /// <summary>
    /// 总进度值
    /// </summary>
    [SugarColumn(ColumnDescription = "总进度值", Length = 18, DecimalDigits = 8, IsNullable = true)]
    public decimal? TotalVal { get; set; }

    /// <summary>
    /// IP地址
    /// </summary>
    [SugarColumn(ColumnDescription = "IP地址", Length = 50, IsNullable = true)]
    [MaxLength(50)]
    public string? Ip { get; set; }

    /// <summary>
    /// 来源
    /// </summary>
    [SugarColumn(ColumnDescription = "来源", IsNullable = true)]
    public EnumBehaviorLogSource? Source { get; set; }

    /// <summary>
    /// 模块
    /// </summary>
    [SugarColumn(ColumnDescription = "模块", Length = 64, IsNullable = true)]
    [MaxLength(64)]
    public string? Module { get; set; }

    /// <summary>
    /// 变更前数据（仅变化字段）
    /// </summary>
    [SugarColumn(ColumnDescription = "变更前数据", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public string? BeforeData { get; set; }

    /// <summary>
    /// 变更后数据（仅变化字段）
    /// </summary>
    [SugarColumn(ColumnDescription = "变更后数据", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public string? AfterData { get; set; }

    /// <summary>
    /// 备注（后台变更记录对象名称）
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = 512, IsNullable = true)]
    [MaxLength(512)]
    public string? Remark { get; set; }
}