namespace LuBan.AIAgent.Configuration;

/// <summary>
/// 工具组配置
/// </summary>
public class ToolGroupOptions
{
    /// <summary>
    /// 浏览器工具配置
    /// </summary>
    public BrowserToolOptions Browser { get; set; } = new();

    /// <summary>
    /// 文件系统工具配置
    /// </summary>
    public FileSystemToolOptions FileSystem { get; set; } = new();

    /// <summary>
    /// 脚本工具配置
    /// </summary>
    public ScriptToolOptions Script { get; set; } = new();

    /// <summary>
    /// 数据库工具配置
    /// </summary>
    public DatabaseToolOptions Database { get; set; } = new();

    /// <summary>
    /// Redis 工具配置
    /// </summary>
    public RedisToolOptions Redis { get; set; } = new();

    /// <summary>
    /// Web 工具配置
    /// </summary>
    public WebToolOptions Web { get; set; } = new();
}