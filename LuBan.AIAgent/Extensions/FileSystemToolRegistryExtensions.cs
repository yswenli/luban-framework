using LuBan.AIAgent.Abstractions;

namespace LuBan.AIAgent.Extensions.FileSystem;

/// <summary>
/// 文件系统工具注册表扩展，用于向工具注册表注册文件系统相关的工具
/// </summary>
public static class FileSystemToolRegistryExtensions
{
    /// <summary>
    /// 向工具注册表注册文件系统工具
    /// </summary>
    /// <param name="registry">工具注册表</param>
    /// <param name="configure">配置选项的委托</param>
    /// <returns>工具注册表</returns>
    public static IToolRegistry RegisterFileSystemTools(this IToolRegistry registry, Action<FileSystemToolOptions> configure)
    {
        var options = new FileSystemToolOptions();
        configure(options);
        return registry.RegisterFileSystemTools(options);
    }

    /// <summary>
    /// 向工具注册表注册文件系统工具
    /// </summary>
    /// <param name="registry">工具注册表</param>
    /// <param name="options">文件系统工具选项</param>
    /// <returns>工具注册表</returns>
    public static IToolRegistry RegisterFileSystemTools(this IToolRegistry registry, FileSystemToolOptions options)
    {
        var pathGuard = new FileSystemPathGuard(options);
        registry.Register([
            new ListDirectoryTool(pathGuard),
            new SearchFilesTool(pathGuard),
            new ReadFileTool(pathGuard, options),
            new ReadFilesBatchTool(pathGuard, options),
            new WriteFileTool(pathGuard),
            new CreateDirectoryTool(pathGuard),
            new MoveFileTool(pathGuard),
            new PatchFileTool(pathGuard),
            new DeleteFileTool(pathGuard, options),
            new DeleteDirectoryTool(pathGuard)
        ]);
        return registry;
    }
}
