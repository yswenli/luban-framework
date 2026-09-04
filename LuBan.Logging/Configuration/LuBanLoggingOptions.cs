/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Logging.Configuration
*文件名： LuBanLoggingOptions.cs
*版本号： V1.0.0.0
*唯一标识：a8cfb829-2c7f-45c9-a652-b2562c191753
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31 14:34:57
*描述：LuBanLoggingOptions 类
*
*=================================================
*修改标记
*修改时间：2026/7/31 14:34:57
*修改人： yswenli
*版本号： V1.0.0.0
*描述：LuBanLoggingOptions 类
*
*****************************************************************************/

namespace LuBan.Logging.Configuration;

/// <summary>
/// LuBan 日志配置选项，所有配置项均有默认值。
/// </summary>
public class LuBanLoggingOptions
{
    /// <summary>
    /// 总开关：是否启用日志，默认 true。
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 日志目录，默认为 logs（当前目录下）。
    /// </summary>
    public string Directory { get; set; } = "logs";

    /// <summary>
    /// 单个日志文件最大大小（MB），超过则滚动，默认 100。
    /// </summary>
    public long MaxFileSizeMB { get; set; } = 100;

    /// <summary>
    /// 最大备份数，默认 5。
    /// </summary>
    public int MaxRollBackups { get; set; } = 5;

    /// <summary>
    /// Logger 类是否输出控制台，默认 false。
    /// </summary>
    public bool EnableConsole { get; set; } = false;

    /// <summary>
    /// 是否记录到数据库，默认 false。
    /// </summary>
    public bool EnableDb { get; set; } = false;

    /// <summary>
    /// 按类别单独控制日志输出，默认全部启用。
    /// </summary>
    public Dictionary<string, bool> Categories { get; set; } = new()
    {
        ["loginfo"] = true,
        ["logdebug"] = true,
        ["logwarn"] = true,
        ["logerror"] = true,
        ["logcall"] = true
    };

    /// <summary>
    /// 数据库日志容量与过期配置。
    /// </summary>
    public LogLimitConfig? LogLimit { get; set; }
}

/// <summary>
/// 数据库日志容量与过期配置。
/// </summary>
public class LogLimitConfig
{
    /// <summary>
    /// api日志最大条数
    /// </summary>
    public int ApiLogMaxSize { get; set; } = 10240;

    /// <summary>
    /// api日志过期时间，单位秒
    /// </summary>
    public int ApiLogExpiredSeconds { get; set; } = 604800;

    /// <summary>
    /// 错误日志最大条数
    /// </summary>
    public int ErrorLogMaxSize { get; set; } = 1024;

    /// <summary>
    /// 错误日志过期时间，单位秒
    /// </summary>
    public int ErrorLogExpiredSeconds { get; set; } = 604800;
}
