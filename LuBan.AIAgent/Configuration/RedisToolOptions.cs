namespace LuBan.AIAgent.Configuration;

/// <summary>
/// Redis 工具配置
/// </summary>
public class RedisToolOptions
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Redis 主机
    /// </summary>
    public string Host { get; set; } = "127.0.0.1";

    /// <summary>
    /// Redis 端口
    /// </summary>
    public int Port { get; set; } = 6379;

    /// <summary>
    /// Redis 密码
    /// </summary>
    public string? Password { get; set; }
}