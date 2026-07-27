namespace LuBan.AIAgent.Tools.FileSystem;

/// <summary>
/// 文件系统工具插件
/// </summary>
public class FileSystemToolPlugin : ILuBanToolPlugin
{
    private readonly FileSystemToolOptions _options;
    private readonly PathGuard _pathGuard;

    /// <summary>
    /// 创建 FileSystemToolPlugin 实例
    /// </summary>
    /// <param name="options">配置选项</param>
    /// <param name="pathGuard">路径守卫</param>
    public FileSystemToolPlugin(IOptions<LuBanAgentOptions> options, PathGuard pathGuard)
    {
        _options = options.Value.Tools.FileSystem;
        _pathGuard = pathGuard;
    }

    /// <summary>
    /// 工具分组名称
    /// </summary>
    public string GroupName => "filesystem";

    /// <summary>
    /// 工具分组描述
    /// </summary>
    public string? Description => "文件系统操作工具，支持读取、写入、搜索文件等操作";

    /// <summary>
    /// 获取工具函数列表
    /// </summary>
    /// <param name="sp">服务提供者</param>
    /// <returns>工具函数列表</returns>
    public IReadOnlyList<AIFunction> GetTools(IServiceProvider sp)
    {
        var toolGroup = new FileSystemToolGroup(_pathGuard);
        var tools = new List<AIFunction>();

        foreach (var method in typeof(FileSystemToolGroup).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            var func = AIFunctionFactory.Create(method, toolGroup);
            tools.Add(func);
        }

        return tools;
    }

    /// <summary>
    /// 判断插件是否启用
    /// </summary>
    /// <param name="options">配置选项</param>
    /// <returns>是否启用</returns>
    public bool IsEnabled(LuBanAgentOptions options) => options.Tools.FileSystem.Enabled;
}

/// <summary>
/// 文件系统工具分组
/// </summary>
public class FileSystemToolGroup
{
    private readonly PathGuard _pathGuard;

    /// <summary>
    /// 创建 FileSystemToolGroup 实例
    /// </summary>
    /// <param name="pathGuard">路径守卫</param>
    public FileSystemToolGroup(PathGuard pathGuard)
    {
        _pathGuard = pathGuard;
    }

    /// <summary>
    /// 读取文件内容
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <returns>文件内容</returns>
    [Description("读取文件内容")]
    public async Task<string> ReadFileAsync(string path)
    {
        if (!_pathGuard.IsAllowed(path))
            return $"错误：路径 {path} 不在允许访问的范围内";

        var fileInfo = new FileInfo(path);
        if (fileInfo.Length > 50 * 1024 * 1024)
            return $"错误：文件过大 ({fileInfo.Length / 1024 / 1024}MB)，最大支持 50MB";

        return await File.ReadAllTextAsync(path);
    }

    /// <summary>
    /// 写入文件
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <param name="content">文件内容</param>
    /// <returns>写入结果</returns>
    [Description("写入文件内容")]
    public async Task<string> WriteFileAsync(string path, string content)
    {
        if (!_pathGuard.IsAllowed(path))
            return $"错误：路径 {path} 不在允许访问的范围内";

        // 确认执行
        if (!ToolConfirmationService.RequestConfirmation("WriteFileAsync", 
            new Dictionary<string, object?> { ["path"] = path, ["content"] = content }))
        {
            return "操作已被用户取消";
        }

        await File.WriteAllTextAsync(path, content);
        return $"已写入文件 {path}";
    }

    /// <summary>
    /// 列出目录内容
    /// </summary>
    /// <param name="path">目录路径</param>
    /// <returns>目录内容</returns>
    [Description("列出目录内容")]
    public Task<string> ListDirectoryAsync(string path)
    {
        if (!_pathGuard.IsAllowed(path))
            return Task.FromResult($"错误：路径 {path} 不在允许访问的范围内");

        var entries = Directory.EnumerateFileSystemEntries(path);
        return Task.FromResult(string.Join("\n", entries));
    }
}
