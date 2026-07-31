namespace LuBan.Logging.Configuration;

/// <summary>
/// LuBan 文件日志配置选项，所有配置项均有默认值。
/// </summary>
public class LuBanLoggingOptions
{
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
    /// 是否包含作用域，默认 false。
    /// </summary>
    public bool IncludeScopes { get; set; } = false;
}
