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
}
