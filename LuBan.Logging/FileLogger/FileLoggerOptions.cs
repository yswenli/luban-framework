/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Logging.FileLogger
*文件名： FileLoggerOptions.cs
*版本号： V1.0.0.0
*唯一标识：484eefd8-c9bc-4db4-9c96-8ac8b35597ef
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31 14:35:44
*描述：FileLoggerOptions 类
*
*=================================================
*修改标记
*修改时间：2026/7/31 14:35:44
*修改人： yswenli
*版本号： V1.0.0.0
*描述：FileLoggerOptions 类
*
*****************************************************************************/

using LuBan.Logging.Configuration;

namespace LuBan.Logging.FileLogger;

/// <summary>
/// 文件日志 Provider 专用配置。
/// </summary>
internal sealed class FileLoggerOptions
{
    /// <summary>
    /// 从 LuBanLoggingOptions 创建 FileLoggerOptions。
    /// </summary>
    public static FileLoggerOptions FromLuBanOptions(LuBanLoggingOptions options)
    {
        return new FileLoggerOptions
        {
            Directory = options.Directory,
            MaxFileSizeBytes = options.MaxFileSizeMB * 1024 * 1024,
            MaxRollBackups = options.MaxRollBackups,
            Categories = options.Categories
        };
    }

    /// <summary>
    /// 日志目录。
    /// </summary>
    public string Directory { get; set; } = "logs";

    /// <summary>
    /// 单个日志文件最大大小（字节）。
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 100 * 1024 * 1024;

    /// <summary>
    /// 最大备份数。
    /// </summary>
    public int MaxRollBackups { get; set; } = 5;

    /// <summary>
    /// 按类别控制日志输出。
    /// </summary>
    public Dictionary<string, bool> Categories { get; set; } = new()
    {
        ["loginfo"] = true,
        ["logdebug"] = true,
        ["logwarn"] = true,
        ["logerror"] = true,
        ["logcall"] = true
    };
}
