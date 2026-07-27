namespace LuBan.AIAgent.Configuration;

/// <summary>
/// 文件系统工具配置
/// </summary>
public class FileSystemToolOptions
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 允许访问的根目录列表
    /// </summary>
    public List<string> AllowedRoots { get; set; } = new();
}