/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.LogLib.Models
*文件名： SysLog
*版本号： V1.0.0.0
*唯一标识：fa2b6f79-ab06-4ac8-9b8a-69b5e41e2060
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/13 17:06:43
*描述：异常日志表
*
*=================================================
*修改标记
*修改时间：2023/12/13 17:06:43
*修改人： yswenli
*版本号： V1.0.0.0
*描述：异常日志表
*
*****************************************************************************/

namespace LuBan.LogLib.Models;


/// <summary>
/// 异常日志表
/// </summary>
[SugarTable("db_log_error", "异常日志表")]
[SysTable]
[LogTable]
public class DbLogError : EntityTenant
{
    /// <summary>
    /// 名称
    /// </summary>
    [SugarColumn(ColumnDescription = "名称", Length = 256)]
    [MaxLength(256)]
    public string ServiceName { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [SugarColumn(ColumnDescription = "描述", Length = 1024)]
    [MaxLength(1024)]
    public string Description { get; set; }
    /// <summary>
    /// 参数
    /// </summary>
    [SugarColumn(ColumnDescription = "参数", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? Parmas { get; set; }

    /// <summary>
    /// 异常信息
    /// </summary>
    [SugarColumn(ColumnDescription = "异常信息", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? Exception { get; set; }
}

