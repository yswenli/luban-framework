using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Core;

namespace LuBan.AIAgent.Extensions.FileSystem;

/// <summary>
/// 文件系统工具注册表工厂，用于创建文件系统工具的注册表
/// </summary>
/// <param name="listDirectoryTool">列出目录工具</param>
/// <param name="searchFilesTool">搜索文件工具</param>
/// <param name="readFileTool">读取文件工具</param>
/// <param name="readFilesBatchTool">批量读取文件工具</param>
/// <param name="writeFileTool">写入文件工具</param>
/// <param name="createDirectoryTool">创建目录工具</param>
/// <param name="moveFileTool">移动文件工具</param>
/// <param name="patchFileTool">补丁文件工具</param>
/// <param name="deleteFileTool">删除文件工具</param>
/// <param name="deleteDirectoryTool">删除目录工具</param>
public class FileSystemToolRegistryFactory(
    ListDirectoryTool listDirectoryTool,
    SearchFilesTool searchFilesTool,
    ReadFileTool readFileTool,
    ReadFilesBatchTool readFilesBatchTool,
    WriteFileTool writeFileTool,
    CreateDirectoryTool createDirectoryTool,
    MoveFileTool moveFileTool,
    PatchFileTool patchFileTool,
    DeleteFileTool deleteFileTool,
    DeleteDirectoryTool deleteDirectoryTool)
{
    /// <summary>
    /// 创建默认的工具注册表，包含所有文件系统工具
    /// </summary>
    /// <returns>工具注册表</returns>
    public IToolRegistry CreateDefaultRegistry()
    {
        var registry = new InMemoryToolRegistry();
        registry.Register([listDirectoryTool, searchFilesTool, readFileTool, readFilesBatchTool, writeFileTool, createDirectoryTool, moveFileTool, patchFileTool, deleteFileTool, deleteDirectoryTool]);
        return registry;
    }

    /// <summary>
    /// 创建包含指定工具的注册表
    /// </summary>
    /// <param name="allowedToolNames">允许的工具名称集合</param>
    /// <returns>工具注册表</returns>
    public IToolRegistry CreateRegistry(IReadOnlyCollection<string> allowedToolNames)
    {
        if (allowedToolNames.Count == 0)
        {
            return CreateDefaultRegistry();
        }

        var allowed = new HashSet<string>(allowedToolNames, StringComparer.Ordinal);
        var registry = new InMemoryToolRegistry();
        registry.Register(GetAllTools().Where(tool => allowed.Contains(tool.Name)));
        return registry;
    }

    /// <summary>
    /// 获取所有文件系统工具
    /// </summary>
    /// <returns>文件系统工具列表</returns>
    private IReadOnlyList<ITool> GetAllTools()
        => [listDirectoryTool, searchFilesTool, readFileTool, readFilesBatchTool, writeFileTool, createDirectoryTool, moveFileTool, patchFileTool, deleteFileTool, deleteDirectoryTool];
}
