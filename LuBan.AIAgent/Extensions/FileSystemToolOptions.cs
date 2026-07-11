namespace LuBan.AIAgent.Extensions.FileSystem;

/// <summary>
/// 文件系统工具选项，用于配置文件系统工具的行为
/// </summary>
public class FileSystemToolOptions
{
    /// <summary>
    /// 文件系统根路径，所有操作都限制在该路径内
    /// </summary>
    public string RootPath { get; set; } = string.Empty;

    /// <summary>
    /// 最大读取字符数，用于限制文件读取的大小
    /// </summary>
    public int MaxReadCharacters { get; set; } = 12000;

    /// <summary>
    /// 允许删除的文件扩展名列表，为空则允许所有扩展名
    /// </summary>
    public string[]? AllowedDeleteExtensions { get; set; }
}
