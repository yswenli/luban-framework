/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.LogLib.Models
*文件名： SysApiLog
*版本号： V1.0.0.0
*唯一标识：93e7857d-0bac-4a78-ad6b-aad01c5a2dac
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/13 17:15:57
*描述：系统日志表
*
*=================================================
*修改标记
*修改时间：2023/12/13 17:15:57
*修改人： yswenli
*版本号： V1.0.0.0
*描述：系统日志表
*
*****************************************************************************/


namespace LuBan.LogLib.Models;

/// <summary>
/// 系统日志表
/// </summary>
[SugarTable("db_log_api", "系统日志表")]
[SysTable]
[LogTable]
public class DbLogApi : EntityTenant
{
    /// <summary>
    /// 名称
    /// </summary>
    [SugarColumn(ColumnDescription = "名称", Length = 256)]
    [MaxLength(256)]
    public string ServiceName { get; set; }

    /// <summary>
    /// 调用方ip
    /// </summary>
    [SugarColumn(ColumnDescription = "调用方ip", Length = 1024)]
    [MaxLength(100)]
    public string? CallIp { get; set; }
    /// <summary>
    /// 请求地址
    /// </summary>
    [SugarColumn(ColumnDescription = "请求地址", Length = 2048)]
    [MaxLength(2048)]
    public string? Url { get; set; }
    /// <summary>
    /// 请求方式
    /// </summary>
    [SugarColumn(ColumnDescription = "请求方式", Length = 50)]
    [MaxLength(50)]
    public string? RequestMethod { get; set; }
    /// <summary>
    /// 请求头
    /// </summary>
    [SugarColumn(ColumnDescription = "请求头", Length = 2048)]
    [MaxLength(2048)]
    public string? Header { get; set; }
    /// <summary>
    /// 设备
    /// </summary>
    [SugarColumn(ColumnDescription = "设备", Length = 1024)]
    [MaxLength(1024)]
    public string? UserAgent { get; set; }
    /// <summary>
    /// 输入值
    /// </summary>
    [SugarColumn(ColumnDescription = "输入值", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    [MaxLength(10240)]
    public string? Input { get; set; }
    /// <summary>
    /// 输出值
    /// </summary>
    [SugarColumn(ColumnDescription = "输出值", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    [MaxLength(10240)]
    public string? Output { get; set; }
    /// <summary>
    /// 响应码
    /// </summary>
    [SugarColumn(ColumnDescription = "响应码")]
    public int? StatusCode { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    [SugarColumn(ColumnDescription = "用户ID", Length = 64)]
    [MaxLength(64)]
    public string? UserId { get; set; }

    /// <summary>
    /// 用时
    /// </summary>
    [SugarColumn(ColumnDescription = "用时")]
    public long? Cost { get; set; }

    /// <summary>
    /// 异常
    /// </summary>
    [SugarColumn(ColumnDescription = "异常", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    [MaxLength(10240)]
    public string? Exception { get; set; }

    /// <summary>
    /// 设备
    /// </summary>
    [SugarColumn(ColumnDescription = "设备", Length = 128)]
    [MaxLength(128)]
    public string? Device { get; set; }

    /// <summary>
    /// 系统
    /// </summary>
    [SugarColumn(ColumnDescription = "系统", Length = 128)]
    [MaxLength(128)]
    public string? Os { get; set; }

    /// <summary>
    /// 浏览器
    /// </summary>
    [SugarColumn(ColumnDescription = "浏览器", Length = 128)]
    [MaxLength(128)]
    public string? Ua { get; set; }
}

