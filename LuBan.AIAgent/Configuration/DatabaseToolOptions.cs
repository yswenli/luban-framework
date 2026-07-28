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
    /// 数据库连接字符串（支持 MySQL、PostgreSQL、SQL Server、SQLite）
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// 默认超时时间（毫秒）
    /// </summary>
    public int DefaultTimeout { get; set; } = 30000;
}