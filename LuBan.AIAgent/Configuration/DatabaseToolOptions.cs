namespace LuBan.AIAgent.Configuration;

/// <summary>
/// 数据库工具配置
/// </summary>
public class DatabaseToolOptions
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 数据库连接字符串
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// 数据库引擎：sqlcmd, mysql
    /// </summary>
    public string Engine { get; set; } = "sqlcmd";

    /// <summary>
    /// 默认超时时间（毫秒）
    /// </summary>
    public int DefaultTimeout { get; set; } = 30000;
}